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

using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using OROptimizer;

namespace IoC.Configuration.ConfigurationFile
{
    public class AutoGeneratedMemberReturnValuesSelectorElement : ConfigurationFileElementAbstr,
                                                                  IAutoGeneratedMemberReturnValuesSelectorElement,
                                                                  ICanHaveCollectionChildElement
    {
        #region Member Variables

        [NotNull]
        [ItemNotNull]
        private readonly List<IReturnValueElement> _returnedValueElements = new List<IReturnValueElement>();

        #endregion

        #region  Constructors

        public AutoGeneratedMemberReturnValuesSelectorElement([NotNull] XmlElement xmlElement, [NotNull] IAutoGeneratedServiceMethodElement parentAutoGeneratedServiceMethodElement) : base(xmlElement, parentAutoGeneratedServiceMethodElement)
        {
            ParentAutoGeneratedServiceMethodElement = parentAutoGeneratedServiceMethodElement;
        }

        #endregion

        #region IAutoGeneratedMemberReturnValuesSelectorElement Interface Implementation

        public override void AddChild(IConfigurationFileElement child)
        {
            base.AddChild(child);

            if (child is IReturnValueElement returnValueElement)
            {
                _returnedValueElements.Add(returnValueElement);

                if (!ExpectedChildTypeInfo.Type.IsTypeAssignableFrom(returnValueElement.ValueTypeInfo.Type))
                    throw new ConfigurationParseException(child,
                        string.Format("Method '{0}' in interface '{1}' has a type '{2}' which is not assignable from type '{3}'.",
                            ParentAutoGeneratedServiceMethodElement.Name,
                            ParentAutoGeneratedServiceMethodElement.ImplementedMehodInfo.DeclaringType.GetTypeNameInCSharpClass(),
                            ParentAutoGeneratedServiceMethodElement.ImplementedMehodInfo.ReturnType.GetTypeNameInCSharpClass(),
                            returnValueElement.ValueTypeInfo.Type.GetTypeNameInCSharpClass()), this);
            }
        }

        public IReadOnlyList<IReturnValueElement> ReturnedValueElements => _returnedValueElements;

        #endregion

        #region ICanHaveCollectionChildElement Interface Implementation

        public ITypeInfo ExpectedChildTypeInfo => ParentAutoGeneratedServiceMethodElement.ValueTypeInfo;

        #endregion

        #region Member Functions

        [NotNull]
        protected IAutoGeneratedServiceMethodElement ParentAutoGeneratedServiceMethodElement { get; }

        #endregion
    }
}