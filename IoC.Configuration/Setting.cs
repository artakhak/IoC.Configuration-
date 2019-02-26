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

using IoC.Configuration.ConfigurationFile;
using IoC.Configuration.DiContainer.BindingsForConfigFile;
using JetBrains.Annotations;

namespace IoC.Configuration
{
    /// <summary>
    ///     Represents a setting in configuration file.
    /// </summary>
    /// <seealso cref="IoC.Configuration.DiContainer.BindingsForConfigFile.NamedValue" />
    /// <seealso cref="IoC.Configuration.ISetting" />
    public class Setting : NamedValue, ISetting
    {
        #region Member Variables

        [NotNull]
        private readonly ISettingElement _settingElement;

        #endregion

        #region  Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Setting" /> class.
        /// </summary>
        public Setting([NotNull] ISettingElement settingElement) : base(settingElement)
        {
            _settingElement = settingElement;
        }

        #endregion

        #region ISetting Interface Implementation

        /// <summary>
        ///     The value de-serialized from ValueAsString to type in property <see cref="ITypedItem.ValueTypeInfo" />.Type.
        /// </summary>
        public object DeserializedValue => _settingElement.DeserializedValue;

        #endregion
    }
}