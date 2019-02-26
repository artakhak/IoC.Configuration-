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
using System.Reflection;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using OROptimizer;
using OROptimizer.DynamicCode;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace IoC.Configuration.ConfigurationFile
{
    public class AutoGeneratedServiceElement : ConfigurationFileElementAbstr, IAutoGeneratedServiceElement
    {
        #region Member Variables

        [NotNull]
        private readonly Dictionary<MethodInfo, IAutoGeneratedServiceMethodElement> _methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap = new Dictionary<MethodInfo, IAutoGeneratedServiceMethodElement>();

        [NotNull]
        [ItemNotNull]
        private readonly List<IAutoGeneratedServiceMethodElement> _methods = new List<IAutoGeneratedServiceMethodElement>(10);

        [NotNull]
        [ItemNotNull]
        private readonly List<IAutoGeneratedServicePropertyElement> _properties = new List<IAutoGeneratedServicePropertyElement>(10);

        private readonly Dictionary<PropertyInfo, IAutoGeneratedServicePropertyElement> _propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap = new Dictionary<PropertyInfo, IAutoGeneratedServicePropertyElement>();

        [NotNull]
        private readonly ITypeHelper _typeHelper;

        [NotNull]
        private readonly ITypeMemberLookupHelper _typeMemberLookupHelper;

        [NotNull]
        private readonly IValidateServiceUsageInPlugin _validateServiceUsageInPlugin;

        #endregion

        #region  Constructors

        public AutoGeneratedServiceElement([NotNull] XmlElement xmlElement, IConfigurationFileElement parent,
                                           [NotNull] ITypeHelper typeHelper,
                                           [NotNull] ITypeMemberLookupHelper typeMemberLookupHelper,
                                           [NotNull] IValidateServiceUsageInPlugin validateServiceUsageInPlugin) : base(xmlElement, parent)
        {
            _typeHelper = typeHelper;
            _typeMemberLookupHelper = typeMemberLookupHelper;
            _validateServiceUsageInPlugin = validateServiceUsageInPlugin;
        }

        #endregion

        #region IAutoGeneratedServiceElement Interface Implementation

        public override void AddChild(IConfigurationFileElement child)
        {
            base.AddChild(child);

            string getTopTextForAddingMultipleSimilarMembers(bool isForProperty)
            {
                return string.Format("If you want to add a {0} with this name in some other interface (i.e., interface '{1}' or one of its base interfaces), please use attribute '{2}', to make this explicit.",
                    isForProperty ? "property" : "method",
                    ImplementedInterfaceTypeInfo.TypeCSharpFullName, ConfigurationFileAttributeNames.DeclaringInterface);
            }

            if (child is IAutoGeneratedServicePropertyElement autoGeneratedServicePropertyElement)
            {
                if (_propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap.ContainsKey(autoGeneratedServicePropertyElement.ImplementedPropertyInfo))
                    throw new ConfigurationParseException(autoGeneratedServicePropertyElement,
                        string.Format("Property '{0}.{1}' was already added. {2}",
                            autoGeneratedServicePropertyElement.ImplementedPropertyInfo.DeclaringType.GetTypeNameInCSharpClass(),
                            autoGeneratedServicePropertyElement.Name,
                            getTopTextForAddingMultipleSimilarMembers(true)), this);

                _propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap[autoGeneratedServicePropertyElement.ImplementedPropertyInfo] = autoGeneratedServicePropertyElement;
                _properties.Add(autoGeneratedServicePropertyElement);
            }
            else if (child is IAutoGeneratedServiceMethodElement autoGeneratedServiceMethodElement)
            {
                if (_methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap.ContainsKey(autoGeneratedServiceMethodElement.ImplementedMehodInfo))
                    throw new ConfigurationParseException(autoGeneratedServiceMethodElement,
                        string.Format("Method '{0}.{1}' was already added. {2}",
                            autoGeneratedServiceMethodElement.ImplementedMehodInfo.DeclaringType.GetTypeNameInCSharpClass(),
                            autoGeneratedServiceMethodElement.Name,
                            getTopTextForAddingMultipleSimilarMembers(false)), this);

                _methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap[autoGeneratedServiceMethodElement.ImplementedMehodInfo] = autoGeneratedServiceMethodElement;
                _methods.Add(autoGeneratedServiceMethodElement);
            }
        }

        public void GenerateAutoImplementedServiceClassCSharp(IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                                              string dynamicImplementationsNamespace,
                                                              out string generatedClassFullName)
        {
            // Add assemblies referenced by service
            var className = $"{ImplementedInterfaceTypeInfo.Type.Name}_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";
            generatedClassFullName = $"{dynamicImplementationsNamespace}.{className}";

            var classStrBldr = new StringBuilder(5000);
            var classMemberVariablesStrBldr = new StringBuilder(1000);

            classStrBldr.AppendLine();
            classStrBldr.AppendLine("using System;");

            classStrBldr.AppendLine($"namespace {dynamicImplementationsNamespace}");
            classStrBldr.AppendLine("{");
            classStrBldr.AppendLine($"public sealed class {className}: {ImplementedInterfaceTypeInfo.TypeCSharpFullName}");
            classStrBldr.AppendLine("{");

            void processType(Type type, ref bool stopProcessingParam)
            {
                var typeProperties = type.GetProperties();

                foreach (var methodInfo in type.GetMethods())
                {
                    if ((methodInfo.Attributes & MethodAttributes.SpecialName) > 0)
                        continue;

                    var methodSignature = GetMethodSignature(methodInfo);

                    classStrBldr.Append(methodSignature);

                    if (_methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap.TryGetValue(methodInfo, out var autoGeneratedServiceMethodElement))
                    {
                        if (autoGeneratedServiceMethodElement.ImplementedMehodInfo == methodInfo)
                        {
                            AddMethodBodyForImplementedMethod(dynamicAssemblyBuilder, autoGeneratedServiceMethodElement,
                                classStrBldr, classMemberVariablesStrBldr);
                        }
                        else
                        {
                            AddMethodBodyForNonImplementedMethodWithSimilarImplementedMethod(dynamicAssemblyBuilder, methodInfo,
                                autoGeneratedServiceMethodElement, classStrBldr);
                        }
                    }
                    else
                    {
                        classStrBldr.AppendLine("{");

                        var parameters = methodInfo.GetParameters();

                        foreach (var outParameterInfo in methodInfo.GetParameters().Where(x => x.IsOut))
                            classStrBldr.AppendLine($"{outParameterInfo.Name}=default({outParameterInfo.ParameterType.GetTypeNameInCSharpClass()});");

                        if (methodInfo.ReturnType != typeof(void))
                        {
                            classStrBldr.Append($"return default({methodInfo.ReturnType.GetTypeNameInCSharpClass()});");
                            classStrBldr.AppendLine();
                        }

                        classStrBldr.AppendLine("}");
                    }

                    classStrBldr.AppendLine();
                }

                foreach (var propertyInfo in type.GetProperties())
                {
                    var propertyHeader = GetPropertyHeader(propertyInfo);

                    void addGetSet()
                    {
                        classStrBldr.Append(" { get;");

                        if (propertyInfo.SetMethod != null)
                            classStrBldr.Append(" set;");

                        classStrBldr.Append("}");
                    }

                    classStrBldr.Append(propertyHeader);

                    if (_propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap.TryGetValue(propertyInfo,
                        out var similarOrSameAutoGeneratedServicePropertyElement))
                    {
                        var similarPropertyInfo = similarOrSameAutoGeneratedServicePropertyElement.ImplementedPropertyInfo;
                        if (similarPropertyInfo == propertyInfo)
                        {
                            addGetSet();
                            classStrBldr.AppendLine($"={similarOrSameAutoGeneratedServicePropertyElement.ReturnValueElement.GenerateValueCSharp(dynamicAssemblyBuilder)};");
                        }
                        else
                        {
                            var privateVariableName = $"_{propertyInfo.DeclaringType.Name}_{propertyInfo.Name}_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";

                            // Not, for nullable values and reference types, we can check for null, to set if the value was initialized.
                            // however, to make it simple, and also to avoid calling the other class property every time, in case the other property 
                            // value is null, lets use the valueWasSet variable for all cases.
                            var valueWasSetVariableName = $"{privateVariableName}_ValueWasSet";


                            classStrBldr.AppendLine();
                            classStrBldr.AppendLine("{");

                            classStrBldr.AppendLine("get");
                            classStrBldr.AppendLine("{");

                            classStrBldr.AppendLine($"if ({valueWasSetVariableName}) return {privateVariableName};");

                            classStrBldr.Append($"{privateVariableName}=(({similarPropertyInfo.DeclaringType.GetTypeNameInCSharpClass()})this)");
                            classStrBldr.AppendLine($".{similarPropertyInfo.Name};");
                            classStrBldr.AppendLine($"{valueWasSetVariableName}=true;");
                            classStrBldr.AppendLine($"return {privateVariableName};");
                            classStrBldr.AppendLine("}");

                            if (propertyInfo.SetMethod != null)
                            {
                                classStrBldr.AppendLine("set");
                                classStrBldr.AppendLine("{");
                                classStrBldr.AppendLine($"{privateVariableName}=value;");
                                classStrBldr.AppendLine($"{valueWasSetVariableName}=true;");

                                classStrBldr.AppendLine("}");
                            }

                            classStrBldr.AppendLine("}");
                            classStrBldr.AppendLine($"private bool {valueWasSetVariableName};");
                            classStrBldr.AppendLine($"private {propertyInfo.PropertyType.GetTypeNameInCSharpClass()} {privateVariableName};");
                        }
                    }
                    else
                    {
                        addGetSet();
                        classStrBldr.AppendLine();
                    }
                }
            }

            ;

            var stopProcessing = false;
            _typeMemberLookupHelper.ProcessTypeImplementedInterfacesAndBaseTypes(ImplementedInterfaceTypeInfo.Type,
                processType, ref stopProcessing);

            classStrBldr.Append(classMemberVariablesStrBldr);

            // Close class
            classStrBldr.AppendLine("}");

            // Close namespace
            classStrBldr.AppendLine("}");

            dynamicAssemblyBuilder.AddCSharpFile(classStrBldr.ToString());
        }

        /// <summary>
        ///     If the property is implemented in configuration file, returns <see cref="IAutoGeneratedServicePropertyElement" />
        ///     for the implemented property.
        ///     Otherwise, if there is a similar auto-implemented property in configuration file (probably a property that
        ///     belongs to one of the interfaces of auto-implemented interface, which has a compatible return type),
        ///     returns <see cref="IAutoGeneratedServicePropertyElement" /> for that implementation.
        ///     Otherwise, returns null.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        public IAutoGeneratedServicePropertyElement GetAutoGeneratedServicePropertyElement(PropertyInfo propertyInfo)
        {
            return _propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap.TryGetValue(propertyInfo,
                out var similarOrSameAutoGeneratedServicePropertyElement)
                ? similarOrSameAutoGeneratedServicePropertyElement
                : null;
        }

        /// <summary>
        ///     If the method is implemented in configuration file, returns <see cref="IAutoGeneratedServiceMethodElement" />
        ///     for the implemented method.
        ///     Otherwise, if there is a similar auto-implemented method in configuration file (probably a method that
        ///     belongs to one of the interfaces of auto-implemented interface, which has the same signature, and a compatible
        ///     return type),
        ///     returns <see cref="IAutoGeneratedServiceMethodElement" /> for that implementation.
        ///     Otherwise, returns null.
        /// </summary>
        public IAutoGeneratedServiceMethodElement GetAutoGeneratedServicePropertyElement(MethodInfo methodInfo)
        {
            return _methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap.TryGetValue(methodInfo,
                out var SimilarOrSameAutoGeneratedServiceMethodElement)
                ? SimilarOrSameAutoGeneratedServiceMethodElement
                : null;
        }

        public ITypeInfo ImplementedInterfaceTypeInfo { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            ImplementedInterfaceTypeInfo = _typeHelper.GetTypeInfo(this, ConfigurationFileAttributeNames.Interface, ConfigurationFileAttributeNames.Assembly, ConfigurationFileAttributeNames.InterfaceRef);

            if (!ImplementedInterfaceTypeInfo.Type.IsInterface)
                throw new ConfigurationParseException(this, $"The type '{ImplementedInterfaceTypeInfo.TypeCSharpFullName}' specified in element '{ElementName}' should be an interface.");

            if (!ImplementedInterfaceTypeInfo.Type.IsPublic)
                throw new ConfigurationParseException(this, $"The interface '{ImplementedInterfaceTypeInfo.TypeCSharpFullName}' specified in element '{ElementName}' should have public access.");

            _validateServiceUsageInPlugin.Validate(this, ImplementedInterfaceTypeInfo);
        }

        public IReadOnlyList<IAutoGeneratedServiceMethodElement> Methods => _methods;
        public IReadOnlyList<IAutoGeneratedServicePropertyElement> Properties => _properties;

        public override void ValidateAfterChildrenAdded()
        {
            base.ValidateAfterChildrenAdded();

            ProcessNonImplementedMethodsAndPropertiesOnChildrenAdded();
        }

        #endregion

        #region Member Functions

        private void AddMethodBodyForImplementedMethod([NotNull] IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                                       [NotNull] IAutoGeneratedServiceMethodElement autoGeneratedServiceMethodElement,
                                                       [NotNull] StringBuilder classStrBldr,
                                                       [NotNull] StringBuilder classMembersStrBldr)
        {
            IDeserializedValue getDeserializedValue(IValueInitializer valueInitializer)
            {
                if (valueInitializer is IDeserializedValue deserializedValue)
                    return deserializedValue;

                if (valueInitializer is IValueInitializerElementDecorator valueInitializerElementDecorator)
                    return valueInitializerElementDecorator.DecoratedValueInitializerElement as IDeserializedValue;

                return null;
            }

            void addReturnedValueCSharp(IAutoGeneratedMemberReturnValuesSelectorElement autoGeneratedMemberReturnValuesSelector)
            {
                // For now only one return value is supported
                var returnedValueElement = autoGeneratedMemberReturnValuesSelector.ReturnedValueElements[0];

                var isDeserializedValueInitializer = getDeserializedValue(returnedValueElement) != null;

                if (autoGeneratedServiceMethodElement.ReuseReturnedValue || isDeserializedValueInitializer)
                {
                    var returnedVariableName = $"_{autoGeneratedServiceMethodElement.ImplementedMehodInfo.Name}_Returned_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";
                    classMembersStrBldr.Append($"private {autoGeneratedServiceMethodElement.ValueTypeInfo.TypeCSharpFullName} {returnedVariableName}");

                    if (isDeserializedValueInitializer ||
                        // If ReuseReturnedValue is true, we always pre-store the returned value, if the value type is a struct, 
                        // since we cannot compare the value with null
                        returnedValueElement.ValueTypeInfo.Type.IsValueType)
                    {
                        classMembersStrBldr.Append($"={returnedValueElement.GenerateValueCSharp(dynamicAssemblyBuilder)}");
                    }
                    else
                    {
                        // If we didn't already set the value of variable in stored the variable, lets check for null first.
                        classStrBldr.AppendLine($"if ({returnedVariableName} == null)");
                        classStrBldr.AppendLine($"{returnedVariableName} = {returnedValueElement.GenerateValueCSharp(dynamicAssemblyBuilder)};");
                    }

                    classMembersStrBldr.AppendLine(";");

                    classStrBldr.AppendLine($"return {returnedVariableName};");
                }
                else
                {
                    classStrBldr.AppendLine($"return {returnedValueElement.GenerateValueCSharp(dynamicAssemblyBuilder)};");
                }
            }

            void addParameterValueCSharp(ParameterInfo parameterInfo, IValueInitializer valueInitializer)
            {
                var deserializedValue = getDeserializedValue(valueInitializer);

                if (deserializedValue != null)
                {
                    var variableName = $"_{autoGeneratedServiceMethodElement.ImplementedMehodInfo.Name}_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";

                    classMembersStrBldr.Append($"private {parameterInfo.ParameterType.GetTypeNameInCSharpClass()} {variableName}=");
                    classMembersStrBldr.Append(valueInitializer.GenerateValueCSharp(dynamicAssemblyBuilder));
                    classMembersStrBldr.AppendLine(";");

                    classStrBldr.Append(variableName);
                }
                else
                {
                    if (parameterInfo.ParameterType != valueInitializer.ValueTypeInfo.Type)
                        classStrBldr.Append($"({parameterInfo.ParameterType.GetTypeNameInCSharpClass()})");

                    classStrBldr.Append(valueInitializer.GenerateValueCSharp(dynamicAssemblyBuilder));
                }
            }

            classStrBldr.AppendLine();
            classStrBldr.AppendLine("{");

            var methodParameters = autoGeneratedServiceMethodElement.ImplementedMehodInfo.GetParameters();

            foreach (var returnedValueSelectorsForIfCase in autoGeneratedServiceMethodElement.ReturnedValueSelectorsForIfCase)
            {
                classStrBldr.Append("if(");
                for (var i = 0; i < returnedValueSelectorsForIfCase.ParameterValueInitializers.Count; ++i)
                {
                    if (i > 0)
                        classStrBldr.Append(" && ");

                    var parameterValueInitializer = returnedValueSelectorsForIfCase.ParameterValueInitializers[i];
                    var parameterInfo = methodParameters[parameterValueInitializer.ParameterIndex];

                    if (returnedValueSelectorsForIfCase.ParameterValueInitializers.Count > 1)
                        classStrBldr.Append("(");

                    if (!parameterInfo.ParameterType.IsValueType)
                    {
                        classStrBldr.Append($"{parameterInfo.Name}!=null && ");
                    }

                    classStrBldr.Append($"{parameterInfo.Name}");

                    if (IsEqualsOperatorUsedInIfStatment(parameterInfo.ParameterType))
                    {
                        classStrBldr.Append("==");

                        addParameterValueCSharp(parameterInfo, parameterValueInitializer.ValueInitializer);
                    }
                    else
                    {
                        classStrBldr.Append(".Equals(");
                        addParameterValueCSharp(parameterInfo, parameterValueInitializer.ValueInitializer);
                        classStrBldr.Append(")");
                    }

                    if (returnedValueSelectorsForIfCase.ParameterValueInitializers.Count > 1)
                        classStrBldr.Append(")");
                }

                classStrBldr.AppendLine(")");
                classStrBldr.AppendLine("{");

                addReturnedValueCSharp(returnedValueSelectorsForIfCase);
                classStrBldr.AppendLine("}");
            }

            if (autoGeneratedServiceMethodElement.ReturnedValueSelectorsForDefaultCase != null)
            {
                addReturnedValueCSharp(autoGeneratedServiceMethodElement.ReturnedValueSelectorsForDefaultCase);
            }
            else
            {
                // We should never get here, however lets consider this case, in case we make the default element optional.
                classStrBldr.AppendLine($"return default({autoGeneratedServiceMethodElement.ImplementedMehodInfo.ReturnType.GetTypeNameInCSharpClass()})");
            }

            classStrBldr.AppendLine("}");
        }

        private void AddMethodBodyForNonImplementedMethodWithSimilarImplementedMethod([NotNull] IDynamicAssemblyBuilder dynamicAssemblyBuilder,
                                                                                      [NotNull] MethodInfo methodInfo, [NotNull] IAutoGeneratedServiceMethodElement similarAutoGeneratedServiceMethodElement,
                                                                                      [NotNull] StringBuilder classStrBldr)
        {
            var autoimplementedMethodInfo = similarAutoGeneratedServiceMethodElement.ImplementedMehodInfo;
            classStrBldr.Append($"=>(({autoimplementedMethodInfo.DeclaringType.GetTypeNameInCSharpClass()})this).");
            classStrBldr.Append($"{autoimplementedMethodInfo.Name}(");

            var parameters = methodInfo.GetParameters();

            for (var i = 0; i < parameters.Length; ++i)
            {
                if (i > 0)
                    classStrBldr.Append(", ");

                var parameterInfo = parameters[i];

                if (parameterInfo.IsOut)
                    classStrBldr.Append("out ");
                else if (parameterInfo.ParameterType.IsByRef)
                    classStrBldr.Append("ref ");
                else if (parameterInfo.IsIn)
                    classStrBldr.Append("in ");

                classStrBldr.Append(parameterInfo.Name);
            }

            classStrBldr.Append(");");
        }


        /// <summary>
        ///     Returns true if <paramref name="replacedMethodInfo" /> and <paramref name="replacingMethodInfo" /> have the same
        ///     name,
        ///     signatures, same the return type of <paramref name="replacedMethodInfo" /> is assignable from
        ///     <paramref name="replacingMethodInfo" />.
        ///     Example 1 when the return value will be true is:
        ///     <paramref name="replacedMethodInfo" />:  <see cref="System.Int32" /> IInterface1.GetValue(int param1, string
        ///     param2);
        ///     <paramref name="replacingMethodInfo" />:  <see cref="System.Int32" /> IInterface2.GetValue(int param1, string
        ///     param2);
        ///     Example 2 when the return value will be true is:
        ///     <paramref name="replacedMethodInfo" />: <see cref="System.Int64" /> IInterface1.GetValue(int param1, string
        ///     param2);
        ///     <paramref name="replacingMethodInfo" />:  <see cref="System.Int32" /> IInterface2.GetValue(int param1, string
        ///     param2);
        ///     Example 1 when the return value will be false is:
        ///     <paramref name="replacedMethodInfo" />: <see cref="System.Int64" /> IInterface1.GetValue(int param1, string
        ///     param2);
        ///     <paramref name="replacingMethodInfo" />: <see cref="System.Int64" /> IInterface2.GetValue(ref int param1, string
        ///     param2);
        ///     Example 2 when the return value will be false is:
        ///     <paramref name="replacedMethodInfo" />: <see cref="System.Int64" /> IInterface1.GetValue(int param1, string
        ///     param2);
        ///     <paramref name="replacingMethodInfo" />: void IInterface2.GetValue(int param1, string param2);
        /// </summary>
        private bool CanMethodBeReplacedWithAnotherMethod([NotNull] MethodInfo replacedMethodInfo, [NotNull] MethodInfo replacingMethodInfo)
        {
            if (replacedMethodInfo == replacingMethodInfo)
                return true;

            if (!replacedMethodInfo.Name.Equals(replacingMethodInfo.Name, StringComparison.Ordinal))
                return false;

            if (!replacedMethodInfo.ReturnType.IsTypeAssignableFrom(replacingMethodInfo.ReturnType))
                return false;

            var parameters1 = replacedMethodInfo.GetParameters();
            var parameters2 = replacingMethodInfo.GetParameters();

            if (parameters1.Length != parameters2.Length)
                return false;

            for (var i = 0; i < parameters1.Length; ++i)
            {
                var parameterInfo1 = parameters1[i];
                var parameterInfo2 = parameters2[i];

                if (parameterInfo1.ParameterType != parameterInfo2.ParameterType ||
                    // The == operator for Type most probably will return false if IsByRef properties have different
                    // values. So most probably we do not have to check for these values.
                    // Remove the next line after verifying this.
                    parameterInfo1.ParameterType.IsByRef != parameterInfo2.ParameterType.IsByRef ||
                    !(parameterInfo1.IsIn == parameterInfo2.IsIn &&
                      parameterInfo1.IsOut == parameterInfo2.IsOut &&
                      parameterInfo1.IsRetval == parameterInfo2.IsRetval &&
                      parameterInfo1.IsOptional == parameterInfo2.IsOptional))
                    return false;
            }

            return true;
        }

        private string GetMethodSignature([NotNull] MethodInfo methodInfo)
        {
            var methodSignatureStrBldr = new StringBuilder(500);

            methodSignatureStrBldr.Append($"{methodInfo.ReturnType.GetTypeNameInCSharpClass()} {methodInfo.DeclaringType.GetTypeNameInCSharpClass()}.{methodInfo.Name}(");

            var parameterInfos = methodInfo.GetParameters();
            for (var paramInd = 0; paramInd < parameterInfos.Length; ++paramInd)
            {
                var parameterInfo = parameterInfos[paramInd];
                if (paramInd > 0)
                    methodSignatureStrBldr.Append(", ");

                if (parameterInfo.IsOut)
                    methodSignatureStrBldr.Append("out ");
                else if (parameterInfo.ParameterType.IsByRef)
                    methodSignatureStrBldr.Append("ref ");
                else if (parameterInfo.IsIn)
                    methodSignatureStrBldr.Append("in ");

                methodSignatureStrBldr.Append($"{parameterInfo.ParameterType.GetTypeNameInCSharpClass()} ");
                methodSignatureStrBldr.Append(parameterInfo.Name);
            }

            methodSignatureStrBldr.Append(")");

            return methodSignatureStrBldr.ToString();
        }

        private string GetPropertyHeader([NotNull] PropertyInfo propertyInfo)
        {
            var propertyHeaderStrBldr = new StringBuilder(500);

            propertyHeaderStrBldr.Append($"{propertyInfo.PropertyType.GetTypeNameInCSharpClass()} {propertyInfo.DeclaringType.GetTypeNameInCSharpClass()}.{propertyInfo.Name}");
            return propertyHeaderStrBldr.ToString();
        }


        private bool IsEqualsOperatorUsedInIfStatment(Type methodParameterType)
        {
            if (methodParameterType.IsEnum)
                return true;

            var equalityOperatorFunc = methodParameterType.GetMethods().FirstOrDefault(methodInfo =>
            {
                if (!((methodInfo.Attributes & MethodAttributes.SpecialName) == MethodAttributes.SpecialName &&
                      methodInfo.Name.Equals("op_Equality") &&
                      methodInfo.ReturnType == typeof(bool)))
                    return false;

                var parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length != 2)
                    return false;

                if (parameterInfos[0].ParameterType == methodParameterType && parameterInfos[1].ParameterType == methodParameterType)
                    return true;

                return false;
            });

            if (equalityOperatorFunc != null)
                return true;

            // Some primitive types like System.Int32 do not have op_Equality special method for == operator
            if (methodParameterType == typeof(long) || methodParameterType == typeof(int) ||
                methodParameterType == typeof(short) || methodParameterType == typeof(sbyte) ||
                methodParameterType == typeof(bool) || methodParameterType == typeof(DateTime) ||
                methodParameterType == typeof(ulong) || methodParameterType == typeof(uint) ||
                methodParameterType == typeof(ushort) || methodParameterType == typeof(byte))
                return true;

            return false;
        }

        private void ProcessNonImplementedMethodsAndPropertiesOnChildrenAdded()
        {
            void processType(Type type, ref bool stopProcessingParam)
            {
                foreach (var methodInfo in type.GetMethods())
                {
                    if ((methodInfo.Attributes & MethodAttributes.SpecialName) == MethodAttributes.SpecialName)
                        continue;

                    if (_methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap.TryGetValue(methodInfo,
                        out var similarOrSameAutoGeneratedServiceMethodElement))
                        continue;

                    similarOrSameAutoGeneratedServiceMethodElement = _methods.FirstOrDefault(
                        x => CanMethodBeReplacedWithAnotherMethod(methodInfo, x.ImplementedMehodInfo));

                    if (similarOrSameAutoGeneratedServiceMethodElement != null)
                        _methodInfoToSimilarOrSameAutoGeneratedServiceMethodElementMap[methodInfo] = similarOrSameAutoGeneratedServiceMethodElement;
                }

                foreach (var propertyInfo in type.GetProperties())
                {
                    if ((propertyInfo.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
                        continue;

                    if (_propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap.TryGetValue(propertyInfo,
                        out var similarOrSameAutoGeneratedServicePropertyElement))
                        continue;

                    similarOrSameAutoGeneratedServicePropertyElement = _properties.FirstOrDefault(
                        x => propertyInfo.PropertyType.IsTypeAssignableFrom(x.ImplementedPropertyInfo.PropertyType));

                    if (similarOrSameAutoGeneratedServicePropertyElement != null)
                        _propertyInfoToSimilarOrSameAutoGeneratedServicePropertyElementMap[propertyInfo] = similarOrSameAutoGeneratedServicePropertyElement;
                }
            }

            var stopProcessing = false;

            _typeMemberLookupHelper.ProcessTypeImplementedInterfacesAndBaseTypes(ImplementedInterfaceTypeInfo.Type,
                processType, ref stopProcessing);
        }

        #endregion
    }
}