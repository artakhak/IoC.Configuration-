﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoC.Configuration.DiContainer;
using IoC.Configuration.DiContainer.BindingsForConfigFile;
using JetBrains.Annotations;
using Ninject;
using Ninject.Modules;
using OROptimizer.DynamicCode;

namespace IoC.Configuration.Ninject
{
    public class NinjectDiManager : IDiManager
    {
        #region IDiManager Interface Implementation

        public void BuildServiceProvider(IDiContainer diContainer, IEnumerable<object> modules)
        {
            var ninjectContainer = ConvertToNinjectContainer(diContainer);
            LoadModules(ninjectContainer.Kernel, modules);
        }

        public IDiContainer CreateDiContainer()
        {
            return new NinjectDiContainer();
        }

        public string DiContainerName => "Ninject";

        public string GenerateModuleClassCode(IDynamicAssemblyBuilder dynamicAssemblyBuilder, IAssemblyLocator assemblyLocator, string moduleClassNamespace, string moduleClassName,
                                              IEnumerable<BindingConfigurationForFile> moduleServiceConfigurationElements)
        {
            // Lets add this assembly, since we are referencing DiHelper in auto-generated code.
            dynamicAssemblyBuilder.AddReferencedAssembly(typeof(NinjectDiManager));
            dynamicAssemblyBuilder.AddReferencedAssembly(typeof(NinjectModule));

            var moduleClassContents = new StringBuilder();

            moduleClassContents.AppendLine("using System.Linq;");

            moduleClassContents.AppendLine($"namespace {moduleClassNamespace}");
            moduleClassContents.AppendLine("{");

            moduleClassContents.AppendLine($"public class {moduleClassName} : {typeof(NinjectModule).FullName}");
            moduleClassContents.AppendLine("{");

            DiManagerImplementationHelper.AddCodeForOnDiContainerReadyMethod(moduleClassContents);
            // Add Load() method
            moduleClassContents.AppendLine($"public override void Load()");
            moduleClassContents.AppendLine("{");

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
            return new NinjectModuleWrapper(module);
        }

        public object GetRequiredBindingsModule()
        {
            // Return NinjectModule module to set some required bindings.
            return null;
        }

        public Type ModuleType => typeof(NinjectModule);

        public void StartServiceProvider(IDiContainer diContainer)
        {
            // Do nothing. Already started.
        }

        #endregion

        #region Member Functions

        private void AddServiceBindings([NotNull] StringBuilder moduleClassContents, [NotNull] BindingConfigurationForFile serviceElement)
        {
            if (serviceElement.RegisterIfNotRegistered)
            {
                moduleClassContents.AppendLine($"if (!Kernel.GetBindings(typeof({serviceElement.ServiceType.FullName})).Any())");
                moduleClassContents.AppendLine("{");
            }

            foreach (var serviceImplementation in serviceElement.Implementations)
            {
                moduleClassContents.Append($"Bind<{serviceElement.ServiceType.FullName}>()");

                if (serviceImplementation.Parameters == null)
                {
                    if (serviceElement.IsSelfBoundService)
                        moduleClassContents.Append(".ToSelf()");
                    else
                        moduleClassContents.Append($".To<{serviceImplementation.ImplementationType.FullName}>()");
                }
                else
                {
                    moduleClassContents.Append($".ToMethod(context => new {serviceImplementation.ImplementationType.FullName}(");

                    for (var i = 0; i < serviceImplementation.Parameters.Length; ++i)
                    {
                        if (i > 0)
                            moduleClassContents.Append(", ");

                        var parameter = serviceImplementation.Parameters[i];

                        switch (parameter.ValueInstantiationType)
                        {
                            case ValueInstantiationType.ResolveFromDiContext:
                                moduleClassContents.Append($"({parameter.ValueType.FullName})context.Kernel.GetService(typeof({parameter.ValueType.FullName}))");
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

                // Add WhenInjected code as in example below
                switch (serviceImplementation.ConditionalInjectionType)
                {
                    case ConditionalInjectionType.None:
                        // No conditional injection.
                        break;
                    case ConditionalInjectionType.WhenInjectedInto:
                        moduleClassContents.Append($".WhenInjectedInto<{serviceImplementation.WhenInjectedIntoType.FullName}>()");
                        break;
                    case ConditionalInjectionType.WhenInjectedExactlyInto:
                        moduleClassContents.Append($".WhenInjectedExactlyInto<{serviceImplementation.WhenInjectedIntoType.FullName}>()");
                        break;
                    default:
                        DiManagerImplementationHelper.ThrowUnsuportedEnumerationValue(serviceImplementation.ConditionalInjectionType);
                        break;
                }

                // Add resolution
                moduleClassContents.Append(".");
                switch (serviceImplementation.ResolutionScope)
                {
                    case DiResolutionScope.Singleton:
                        moduleClassContents.Append("InSingletonScope()");
                        break;
                    case DiResolutionScope.Transient:
                        moduleClassContents.Append("InTransientScope()");
                        break;
                    case DiResolutionScope.ScopeLifetime:
                        moduleClassContents.Append($"InScope(context => _diContainer.CurrentLifeTimeScope)");
                        break;
                    // Thread scope is not supported by AUtofac, and therefore not used in Ninject as well.
                    //case DiResolutionScope.Thread:
                    //    moduleClassContents.Append("InThreadScope()");
                    //    break;
                    default:
                        DiManagerImplementationHelper.ThrowUnsupportedResolutionScope(serviceImplementation);
                        break;
                }

                // Add injected properties
                if (serviceImplementation.InjectedProperties?.Any() ?? false)
                {
                    moduleClassContents.AppendLine($".OnActivation<{serviceImplementation.ImplementationType.FullName}>(activatedObject =>");
                    moduleClassContents.AppendLine("{");

                    foreach (var injectedProperty in serviceImplementation.InjectedProperties)
                    {
                        moduleClassContents.Append($"activatedObject.{injectedProperty.Name}=");

                        switch (injectedProperty.ValueInstantiationType)
                        {
                            case ValueInstantiationType.ResolveFromDiContext:
                                moduleClassContents.Append($"_diContainer.Resolve<{injectedProperty.ValueType.FullName}>();");
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

            if (serviceElement.RegisterIfNotRegistered)
                moduleClassContents.AppendLine("}");
        }

        private NinjectDiContainer ConvertToNinjectContainer([NotNull] IDiContainer diContainer)
        {
            var ninjectDiContainer = diContainer as NinjectDiContainer;

            if (ninjectDiContainer == null)
                throw new ArgumentException($"Invalid value of parameter '{nameof(diContainer)}' in '{GetType().FullName}.{nameof(BuildServiceProvider)}(...)'. Expected an object of type '{typeof(NinjectDiContainer).FullName}'. Actual object type is {diContainer.GetType().FullName}.");

            return ninjectDiContainer;
        }

        private void LoadModules([NotNull] IKernel kernel, [NotNull] [ItemNotNull] IEnumerable<object> modules)
        {
            var ninjectModulesList = new List<INinjectModule>();

            foreach (var moduleObject in modules)
            {
                var ninjectModule = moduleObject as INinjectModule;
                if (ninjectModule == null)
                    throw new Exception($"Invalid type of module object: '{moduleObject.GetType().FullName}'. Expected an object of type '{typeof(INinjectModule)}'.");

                ninjectModulesList.Add(ninjectModule);
            }

            if (ninjectModulesList.Count > 0)
                kernel.Load(ninjectModulesList);
        }

        #endregion
    }
}