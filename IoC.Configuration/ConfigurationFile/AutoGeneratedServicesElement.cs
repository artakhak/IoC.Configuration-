// This software is part of the IoC.Configuration library
// Copyright � 2018 IoC.Configuration Contributors
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
using System.Xml;
using JetBrains.Annotations;

namespace IoC.Configuration.ConfigurationFile
{
    public class AutoGeneratedServicesElement : ConfigurationFileElementAbstr, IAutoGeneratedServices
    {
        #region Member Variables

        [NotNull]
        [ItemNotNull]
        private readonly LinkedList<IAutoGeneratedServiceElement> _autoGeneratedServices = new LinkedList<IAutoGeneratedServiceElement>();

        [NotNull]
        private readonly Dictionary<Type, IAutoGeneratedServiceElement> _implmentedInterfaceTypeToAutogenerateServiceElementMap = new Dictionary<Type, IAutoGeneratedServiceElement>();

#pragma warning disable CS0612, CS0618
        [NotNull]
        private readonly Dictionary<Type, ITypeFactory> _implmentedInterfaceTypeToTypeFactoryMap = new Dictionary<Type, ITypeFactory>();
#pragma warning restore CS0612, CS0618

        #endregion

        #region  Constructors

        public AutoGeneratedServicesElement([NotNull] XmlElement xmlElement, IConfigurationFileElement parent) : base(xmlElement, parent)
        {
        }

        #endregion

        #region IAutoGeneratedServices Interface Implementation

        public IEnumerable<IAutoGeneratedServiceElement> Services => _autoGeneratedServices;

        [Obsolete("Will be removed after 5/31/2019")]
        IEnumerable<ITypeFactory> IAutoGeneratedServices.TypeFactories => _implmentedInterfaceTypeToTypeFactoryMap.Values;

        #endregion

        #region Member Functions

        public override void AddChild(IConfigurationFileElement child)
        {
            base.AddChild(child);

            if (child is IAutoGeneratedServiceElement autoGeneratedServiceElement)
            {
                if (_implmentedInterfaceTypeToAutogenerateServiceElementMap.ContainsKey(autoGeneratedServiceElement.ImplementedInterfaceTypeInfo.Type))
                    throw new ConfigurationParseException(autoGeneratedServiceElement, $"Multiple occurrences of type factories implementing the same interface '{autoGeneratedServiceElement.ImplementedInterfaceTypeInfo.TypeCSharpFullName}'.", this);

                _implmentedInterfaceTypeToAutogenerateServiceElementMap[autoGeneratedServiceElement.ImplementedInterfaceTypeInfo.Type] = autoGeneratedServiceElement;
                _autoGeneratedServices.AddLast(autoGeneratedServiceElement);
            }
#pragma warning disable CS0612, CS0618
            else if (child is ITypeFactory typeFactory)
            {
                if (_implmentedInterfaceTypeToTypeFactoryMap.ContainsKey(typeFactory.ImplementedMethodInfo.DeclaringType))
                    throw new ConfigurationParseException(typeFactory, $"Multiple occurrences of type factories implementing the same interface '{typeFactory.ImplementedMethodInfo.DeclaringType.FullName}'.", this);

                _implmentedInterfaceTypeToTypeFactoryMap[typeFactory.ImplementedMethodInfo.DeclaringType] = typeFactory;
            }
#pragma warning restore CS0612, CS0618
        }

        #endregion
    }
}