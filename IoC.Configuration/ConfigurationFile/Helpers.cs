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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;

namespace IoC.Configuration.ConfigurationFile
{
    public static class Helpers
    {
        #region Member Functions

        private static bool AreParametersAMatch([NotNull] [ItemNotNull] MethodInfo methodInfo,
                                                [NotNull] [ItemNotNull] Type[] parameterTypes)
        {
            var parameterInfos = methodInfo.GetParameters();

            if (parameterInfos.Length != parameterTypes.Length)
                return false;

            for (var i = 0; i < parameterInfos.Length; ++i)
            {
                var parameterInfo = parameterInfos[i];

                // For now we do not care about out, ref, in parameters.
                // Therefore, these methods will be ignored.
                if (parameterInfo.IsIn || parameterInfo.IsLcid || parameterInfo.IsOut || parameterInfo.IsRetval ||
                    parameterInfo.ParameterType.IsByRef)
                    return false;

                if (parameterInfo.ParameterType != parameterTypes[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Converts <paramref name="configurationFileElement" /> to type <typeparamref name="T" />. Throws an exception if the
        ///     conversion fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurationFileElement"></param>
        /// <returns>Converted value.</returns>
        /// <exception cref="ConfigurationParseException">Throws this exception.</exception>
        [NotNull]
        public static T ConvertTo<T>(IConfigurationFileElement configurationFileElement) where T : class, IConfigurationFileElement
        {
            var convertedValue = configurationFileElement as T;

            if (convertedValue == null)
                throw new ConfigurationParseException(configurationFileElement, $"Could not convert to {typeof(T).FullName}.");

            return convertedValue;
        }

        /// <summary>
        /// </summary>
        /// <param name="requestorFileElement"></param>
        /// <param name="assemblyAlias"></param>
        /// <returns></returns>
        /// <exception cref="ConfigurationParseException">
        ///     Throws this exception if the assembly with specified alias
        ///     does not exitst in configuration file.
        /// </exception>
        internal static IAssembly GetAssemblySettingByAssemblyAlias([NotNull] IConfigurationFileElement requestorFileElement, [NotNull] string assemblyAlias)
        {
            var assemblyElement = requestorFileElement.Configuration.Assemblies.GetAssemblyByAlias(assemblyAlias);

            if (assemblyElement == null)
                throw new ConfigurationParseException(requestorFileElement, $"No assembly with alias '{assemblyAlias}' is declared in '{ConfigurationFileElementNames.Assemblies}' element.");

            return assemblyElement;
        }

        internal static T GetAttributeEnumValue<T>(this IConfigurationFileElement configurationFileElement, string attributeName) where T : struct
        {
            var typeOfT = typeof(T);

            if (!typeOfT.IsEnum)
                throw new ConfigurationParseException(configurationFileElement, $"Type '{typeOfT.FullName}' is not an enum type.");

            var attributeValue = configurationFileElement.GetAttributeValue(attributeName)?.Trim();

            if (attributeValue == null)
                attributeValue = string.Empty;

            if (attributeValue.Length > 0)
            {
                var attributeValueCapitalized = $"{char.ToUpper(attributeValue[0])}{attributeValue.Substring(1)}";

                if (attributeValue.Length == 1)
                    attributeValueCapitalized = attributeValue.ToUpper();
                else
                    attributeValueCapitalized = $"{char.ToUpper(attributeValue[0])}{attributeValue.Substring(1)}";

                var enumValue = default(T);
                if (Enum.TryParse(attributeValueCapitalized, out enumValue))
                    return enumValue;
            }

            throw new ConfigurationParseException(configurationFileElement, $"Could not parse value '{attributeValue}' to an enum value of type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurationFileElement"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        /// <exception cref="ConfigurationParseException">
        ///     Throws this exception if the attribute value cannot be converted to type
        ///     of 'T'.
        /// </exception>
        internal static T GetAttributeValue<T>(this IConfigurationFileElement configurationFileElement, string attributeName)
        {
            var typeOfConvertedValue = typeof(T);

            var attributeValue = configurationFileElement.GetAttributeValue(attributeName);

            if (typeOfConvertedValue == typeof(string))
                return (T) (object) attributeValue;

            try
            {
                if (typeOfConvertedValue == typeof(double) || typeOfConvertedValue == typeof(float))
                    return (T) (object) double.Parse(attributeValue);

                if (typeOfConvertedValue == typeof(long))
                    return (T) (object) long.Parse(attributeValue);

                if (typeOfConvertedValue == typeof(int))
                    return (T) (object) int.Parse(attributeValue);

                if (typeOfConvertedValue == typeof(short))
                    return (T) (object) short.Parse(attributeValue);

                if (typeOfConvertedValue == typeof(byte))
                    return (T) (object) short.Parse(attributeValue);

                if (typeOfConvertedValue == typeof(bool))
                {
                    if (attributeValue.Equals("1"))
                        return (T) (object) true;

                    if (attributeValue.Equals("0"))
                        return (T) (object) false;

                    return (T) (object) bool.Parse(attributeValue);
                }

                if (typeOfConvertedValue == typeof(DateTime))
                    return (T) (object) DateTime.Parse(attributeValue);

                LogHelper.Context.Log.WarnFormat("Unhandled type '{0}' in '{1}.{2}().'", typeOfConvertedValue, typeof(Helpers).FullName, nameof(GetAttributeValue));
            }
            catch
            {
                // We will throw exception next.
            }

            // If we didn't return anything, throw.
            throw new ConfigurationParseException(configurationFileElement,
                $"Attribute '{attributeName}' should have valid non-empty value of type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// </summary>
        /// <param name="configurationFileElement"></param>
        /// <exception cref="ConfigurationParseException">
        ///     Throws this exception if the attribute value cannot be conveted to a
        ///     boolean value.
        /// </exception>
        internal static bool GetEnabledAttributeValue(this IConfigurationFileElement configurationFileElement)
        {
            return configurationFileElement.GetAttributeValue<bool>(ConfigurationFileAttributeNames.Enabled);
        }

        /// <summary>
        /// </summary>
        /// <param name="configurationFileElement"></param>
        /// <exception cref="ConfigurationParseException">Throws this exception if the attribute value is null or empty string.</exception>
        internal static string GetNameAttributeValue(this IConfigurationFileElement configurationFileElement)
        {
            return configurationFileElement.GetAttributeValue<string>(ConfigurationFileAttributeNames.Name);
        }

        /// <summary>
        /// </summary>
        /// <param name="assemblyLocator"></param>
        /// <param name="requestorFileElement"></param>
        /// <param name="assemblyElement"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        /// <exception cref="ConfigurationParseException">Throws this exception if the type is not in an assembly.</exception>
        [Obsolete("Use ITypeHelper.GetTypeInfo(). Will be removed after 5/31/2019.")]
        internal static Type GetTypeInAssembly([NotNull] IAssemblyLocator assemblyLocator, [NotNull] IConfigurationFileElement requestorFileElement, [NotNull] IAssembly assemblyElement, [NotNull] string typeFullName)
        {
            System.Reflection.Assembly assembly = null;

            try
            {
                assembly = assemblyLocator.LoadAssembly(Path.GetFileName(assemblyElement.AbsolutePath), Path.GetDirectoryName(assemblyElement.AbsolutePath));
            }
            catch
            {
                throw new ConfigurationParseException(assemblyElement, $"Failed to load assembly '{assemblyElement.AbsolutePath}'.");
            }

            var type = assembly.GetType(typeFullName, false, false);

            if (type == null)
                throw new ConfigurationParseException(requestorFileElement, $"Type '{typeFullName}' was not found in an assembly '{assemblyElement.AbsolutePath}'. Assembly is specified in an element '{ConfigurationFileElementNames.Assembly}' with the value of attribute '{ConfigurationFileAttributeNames.Alias}' equal to '{assemblyElement.Alias}'.");

            return type;
        }

        public static bool IsMethodAMatch(MethodInfo methodInfo, string methodName, Type[] parameterTypes)
        {
            return methodInfo.IsPublic &&
                   methodInfo.Name.Equals(methodName, StringComparison.Ordinal) &&
                   AreParametersAMatch(methodInfo, parameterTypes);
        }

        public static string XmlElementToString(this XmlElement xmlElement)
        {
            var xmlElementText = new StringBuilder();

            // Add details about current element
            xmlElementText.Append($"<{xmlElement.Name}");

            for (var attrIndex = 0; attrIndex < xmlElement.Attributes.Count; ++attrIndex)
            {
                var attriute = xmlElement.Attributes[attrIndex];
                xmlElementText.Append($" {attriute.Name}=\"{attriute.Value}\"");
            }

            if (!xmlElement.HasChildNodes)
                xmlElementText.Append("/");

            xmlElementText.Append(">");

            return xmlElementText.ToString();
        }

        #endregion
    }
}