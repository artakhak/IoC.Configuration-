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
using IoC.Configuration.ConfigurationFile;
using IoC.Configuration.DiContainer;
using IoC.Configuration.DiContainer.BindingsForCode;
using JetBrains.Annotations;

namespace IoC.Configuration
{
    public delegate void ProcessTypeInfo(ITypeInfo typeInfo, ref bool stopProcessing);

    public delegate void ProcessConfigurationFileElement(IConfigurationFileElement configurationFileElement, ref bool stopProcessing);

    public delegate void LifeTimeScopeTerminatedEventHandler([CanBeNull] object sender, [NotNull] LifeTimeScopeTerminatedEventArgs e);

    public delegate void BindingConfigurationAddedEventHandler([CanBeNull] object sender, [NotNull] BindingConfigurationAddedEventArgs e);

    public delegate void BindingImplementationConfigurationAddedEventHandler<TBindingImplementationConfiguration>([CanBeNull] object sender,
                                                                                                                  [NotNull] BindingImplementationConfigurationAddedEventArgs<TBindingImplementationConfiguration> e) where TBindingImplementationConfiguration : BindingImplementationConfiguration;

    public delegate void ConfigurationFileXmlDocumentLoadedEventHandler([CanBeNull] object sender, [NotNull] ConfigurationFileXmlDocumentLoadedEventArgs e);

    /// <summary>
    ///     Returns a code to resolves service specified in parameter "service" to implementation.
    /// </summary>
    [NotNull]
    [ItemNotNull]
    public delegate string GenerateCodeToResolveRequiredDependency([NotNull] [ItemNotNull] Type serviceType);
}