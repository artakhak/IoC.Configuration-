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

namespace IoC.Configuration.ConfigurationFile
{
    public static class ConfigurationFileElementNames
    {
        #region Member Variables

        public const string AdditionalAssemblyProbingPaths = "additionalAssemblyProbingPaths";
        public const string AppDataDir = "appDataDir";
        public const string Assemblies = "assemblies";
        public const string Assembly = "assembly";

        public const string AutoGeneratedMemberReturnValuesDefaultSelector = "default";
        public const string AutoGeneratedMemberReturnValuesIfSelector = "if";

        public const string AutoGeneratedServices = "autoGeneratedServices";
        public const string AutoMethod = "autoMethod";
        public const string AutoProperty = "autoProperty";

        public const string AutoService = "autoService";
        public const string ClassMember = "classMember";
        public const string Collection = "collection";
        public const string ConstructedValue = "constructedValue";
        public const string ControllerAssemblies = "controllerAssemblies";
        public const string ControllerAssembly = "controllerAssembly";
        public const string DependencyInjection = "dependencyInjection";
        public const string DiManager = "diManager";

        public const string DiManagers = "diManagers";
        public const string GenericTypeParameters = "genericTypeParameters";
        public const string Implementation = "implementation";
        public const string InjectedProperties = "injectedProperties";
        public const string MethodSignature = "methodSignature";
        public const string Module = "module";
        public const string Modules = "modules";

        public const string Parameters = "parameters";
        public const string ParameterSerializer = "parameterSerializer";

        public const string ParameterSerializers = "parameterSerializers";
        public const string Plugin = "plugin";
        public const string PluginImplementation = "pluginImplementation";
        public const string Plugins = "plugins";
        public const string PluginSetup = "pluginSetup";

        public const string PluginsSetup = "pluginsSetup";
        public const string ProbingPath = "probingPath";
        public const string ProxyService = "proxyService";
        public const string RootElement = "iocConfiguration";
        public const string SelfBoundService = "selfBoundService";
        public const string Serializers = "serializers";
        public const string Service = "service";

        public const string Services = "services";
        public const string ServiceToProxy = "serviceToProxy";
        public const string Settings = "settings";

        public const string SettingsRequestor = "settingsRequestor";
        public const string SettingValue = "settingValue";
        public const string StartupAction = "startupAction";
        public const string StartupActions = "startupActions";
        public const string TypeDefinition = "typeDefinition";

        public const string TypeDefinitions = "typeDefinitions";

        [Obsolete("Will be removed after 5/31/2019")]
        public const string TypeFactory = "typeFactory";

        [Obsolete("Will be removed after 5/31/2019")]
        public const string TypeFactoryReturnedType = "returnedType";

        [Obsolete("Will be removed after 5/31/2019")]
        public const string TypeFactoryReturnedTypesDefaultSelector = "default";

        [Obsolete("Use AutoGeneratedMemberReturnValuesIfSelectorElement")]
        public const string TypeFactoryReturnedTypesIfSelector = "if";

        public const string ValueBoolean = "boolean";
        public const string ValueByte = "byte";
        public const string ValueDateTime = "datetime";
        public const string ValueDouble = "double";
        public const string ValueImplementation = "valueImplementation";
        public const string ValueInjectedObject = "injectedObject";

        public const string ValueInt16 = "int16";
        public const string ValueInt32 = "int32";
        public const string ValueInt64 = "int64";
        public const string ValueObject = "object";
        public const string ValueString = "string";

        public const string WebApi = "webApi";

        #endregion
    }
}