﻿// This software is part of the IoC.Configuration library
// Copyright © 2018 IoC.Configuration Contributors
// http://oroptimizer.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using IoC.Configuration.DiContainer;
using IoC.Configuration.DiContainer.BindingsForConfigFile;
using JetBrains.Annotations;
using OROptimizer.DynamicCode;

namespace IoC.Configuration.Autofac
{
    public class AutofacDiManager : IDiManager
    {
        #region IDiManager Interface Implementation

        public void BuildServiceProvider(IDiContainer diContainer, IEnumerable<object> modules)
        {
            var autofacDiContainer = ConvertToAutofacContainer(diContainer);

            if (autofacDiContainer == null)
                throw new ArgumentException($"Invalid value of parameter '{nameof(diContainer)}' in '{GetType().FullName}.{nameof(BuildServiceProvider)}(...)'. Expected an object of type '{typeof(AutofacDiContainer).FullName}'. Actual object type is {diContainer.GetType().FullName}.");

            RegisterModules(autofacDiContainer.ContainerBuilder, modules);
        }

        public IDiContainer CreateDiContainer()
        {
            return new AutofacDiContainer();
        }

        public string DiContainerName => "Autofac";

        public string GenerateModuleClassCode(IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                              IAssemblyLocator assemblyLocator,
                                              string moduleClassNamespace, string moduleClassName,
                                              IEnumerable<BindingConfigurationForFile> moduleServiceConfigurationElements)
        {
            // Lets add this assembly, since we are referencing DiHelper in auto-generated code.
            dynamicAssemblyBuilder.AddReferencedAssembly(typeof(AutofacDiManager));
            dynamicAssemblyBuilder.AddReferencedAssembly(typeof(ContainerBuilder));
            dynamicAssemblyBuilder.AddReferencedAssembly(typeof(AutofacRegistration));

            var moduleClassContents = new StringBuilder();

            moduleClassContents.AppendLine($"using Autofac;");

            moduleClassContents.AppendLine($"namespace {moduleClassNamespace}");
            moduleClassContents.AppendLine("{");

            moduleClassContents.AppendLine($"public class {moduleClassName} : {typeof(Module).FullName}");
            moduleClassContents.AppendLine("{");

            DiManagerImplementationHelper.AddCodeForOnDiContainerReadyMethod(moduleClassContents);

            // Add Load() method
            moduleClassContents.AppendLine($"protected override void Load({typeof(ContainerBuilder).FullName} builder)");
            moduleClassContents.AppendLine("{");
            moduleClassContents.AppendLine("base.Load(builder);");

            foreach (var service in moduleServiceConfigurationElements)
                AddServiceBindings(moduleClassContents, service);

            moduleClassContents.AppendLine("}");
            // End Load() method

            moduleClassContents.AppendLine("}");
            moduleClassContents.AppendLine("}");
            return moduleClassContents.ToString();
        }

        public object GenerateNativeModule(IDiModule module)
        {
            return new AutofacModuleWrapper(module);
        }

        public object GetRequiredBindingsModule()
        {
            // Return Autofac module to set some required bindings.
            return null;
        }

        public Type ModuleType => typeof(Module);

        public void StartServiceProvider(IDiContainer diContainer)
        {
            var autofacDiContainer = ConvertToAutofacContainer(diContainer);

            if (autofacDiContainer.Container == null)
            {
                autofacDiContainer.ContainerBuilder.Build();

                if (autofacDiContainer.Container == null)
                    throw new Exception($"The value of  '{typeof(AutofacDiContainer).FullName}.{nameof(AutofacDiContainer.Container)}' was not properly initialized.");
            }
        }

        #endregion

        #region Member Functions

        private void AddServiceBindings([NotNull] StringBuilder moduleClassContents, [NotNull] BindingConfigurationForFile serviceElement)
        {
            foreach (var serviceImplementation in serviceElement.Implementations)
            {
                moduleClassContents.Append("builder.");
                if (serviceImplementation.Parameters == null)
                {
                    moduleClassContents.Append($"RegisterType<{serviceImplementation.ImplementationType.FullName}>()");
                }
                else
                {
                    moduleClassContents.Append($"Register(context => new {serviceImplementation.ImplementationType.FullName}(");

                    for (var i = 0; i < serviceImplementation.Parameters.Length; ++i)
                    {
                        if (i > 0)
                            moduleClassContents.Append(", ");

                        var parameter = serviceImplementation.Parameters[i];

                        switch (parameter.ValueInstantiationType)
                        {
                            case ValueInstantiationType.ResolveFromDiContext:
                                moduleClassContents.Append($"context.Resolve<{parameter.ValueType.FullName}>()");
                                break;
                            case ValueInstantiationType.DeserializeFromStringValue:
                                moduleClassContents.Append(DiManagerImplementationHelper.GenerateCodeForDeserializedParameterValue(parameter));
                                break;
                            default:
                                DiManagerImplementationHelper.ThrowUnsuportedEnumerationValue(parameter.ValueInstantiationType);
                                break;
                        }
                    }

                    moduleClassContents.Append("))");
                }

                // Add service
                if (serviceElement.IsSelfBoundService)
                    moduleClassContents.Append(".AsSelf()");
                else
                    moduleClassContents.Append($".As<{serviceElement.ServiceType.FullName}>()");

                if (serviceElement.RegisterIfNotRegistered)
                    moduleClassContents.Append($".IfNotRegistered(typeof({serviceElement.ServiceType.FullName}))");

                // Add resolution
                moduleClassContents.Append(".");
                switch (serviceImplementation.ResolutionScope)
                {
                    case DiResolutionScope.Singleton:
                        moduleClassContents.Append("SingleInstance()");
                        break;
                    case DiResolutionScope.Transient:
                        moduleClassContents.Append("InstancePerDependency()");
                        break;
                    case DiResolutionScope.ScopeLifetime:
                        moduleClassContents.Append("InstancePerLifetimeScope()");
                        break;
                    default:
                        DiManagerImplementationHelper.ThrowUnsupportedResolutionScope(serviceImplementation);
                        break;
                }

                // Add injected properties

                if (serviceImplementation.InjectedProperties?.Any() ?? false)
                {
                    moduleClassContents.AppendLine(".OnActivated(e =>");
                    moduleClassContents.AppendLine("{");

                    foreach (var injectedProperty in serviceImplementation.InjectedProperties)
                    {
                        moduleClassContents.Append($"e.Instance.{injectedProperty.Name}=");

                        switch (injectedProperty.ValueInstantiationType)
                        {
                            case ValueInstantiationType.ResolveFromDiContext:
                                moduleClassContents.Append($"e.Context.Resolve<{injectedProperty.ValueType.FullName}>();");
                                break;
                            case ValueInstantiationType.DeserializeFromStringValue:
                                moduleClassContents.Append(DiManagerImplementationHelper.GenerateCodeForDeserializedParameterValue(injectedProperty));
                                moduleClassContents.Append(";");
                                break;
                            default:
                                DiManagerImplementationHelper.ThrowUnsuportedEnumerationValue(injectedProperty.ValueInstantiationType);
                                break;
                        }

                        moduleClassContents.AppendLine();
                    }

                    moduleClassContents.Append("})");
                }

                moduleClassContents.AppendLine(";");
            }
        }

        private AutofacDiContainer ConvertToAutofacContainer([NotNull] IDiContainer diContainer)
        {
            var autofacDiContainer = diContainer as AutofacDiContainer;

            if (autofacDiContainer == null)
                throw new ArgumentException($"Invalid value of parameter '{nameof(diContainer)}' in '{GetType().FullName}.{nameof(BuildServiceProvider)}(...)'. Expected an object of type '{typeof(AutofacDiContainer).FullName}'. Actual object type is {diContainer.GetType().FullName}.");

            return autofacDiContainer;
        }

        private void RegisterModules([NotNull] ContainerBuilder containerBuilder, [NotNull] [ItemNotNull] IEnumerable<object> modules)
        {
            foreach (var moduleObject in modules)
            {
                var autofacModule = moduleObject as Module;
                if (autofacModule == null)
                    throw new Exception($"Invalid type of module object: '{moduleObject.GetType().FullName}'. Expected an object of type '{typeof(Module)}'.");

                containerBuilder.RegisterModule(autofacModule);
            }
        }

        #endregion
    }
}