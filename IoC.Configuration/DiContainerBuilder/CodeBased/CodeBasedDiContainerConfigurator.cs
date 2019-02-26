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

using IoC.Configuration.DiContainer;
using JetBrains.Annotations;

namespace IoC.Configuration.DiContainerBuilder.CodeBased
{
    public class CodeBasedDiContainerConfigurator : CodeBasedConfiguratorAbstr, ICodeBasedDiContainerConfigurator
    {
        #region  Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeBasedDiContainerConfigurator" /> class.
        /// </summary>
        /// <param name="codeBasedConfiguration">The code based configuration.</param>
        public CodeBasedDiContainerConfigurator([NotNull] CodeBasedConfiguration codeBasedConfiguration) : base(codeBasedConfiguration)
        {
        }

        #endregion

        #region ICodeBasedDiContainerConfigurator Interface Implementation

        /// <summary>
        ///     Creates an instance of <see cref="ICodeBasedDiModulesConfigurator" /> using a preset instance of
        ///     <see cref="IDiContainer" />.
        ///     Use the method <see cref="WithDiContainer(IDiContainer)" /> if possible.
        /// </summary>
        /// <param name="diContainer">An instance of <see cref="IDiContainer" />.</param>
        /// <returns>Returns an instance of <see cref="ICodeBasedDiModulesConfigurator" /></returns>
        public ICodeBasedDiModulesConfigurator WithDiContainer(IDiContainer diContainer)
        {
            _codeBasedConfiguration.DiContainer = diContainer;
            return new CodeBasedDiModulesConfigurator(_codeBasedConfiguration);
        }

        /// <summary>
        ///     This is the preferred method. An instance of <see cref="IDiContainer" /> will be initialized using the
        ///     method <see cref="IDiManager.CreateDiContainer" />() in <see cref="IDiManager" />.
        /// </summary>
        /// <returns>Returns an instance of <see cref="ICodeBasedDiModulesConfigurator" /></returns>
        public ICodeBasedDiModulesConfigurator WithoutPresetDiContainer()
        {
            return new CodeBasedDiModulesConfigurator(_codeBasedConfiguration);
        }

        #endregion

        #region Member Functions

        /// <summary>
        ///     Registers the modules.
        /// </summary>
        /// <returns></returns>
        public ICodeBasedContainerStarter RegisterModules()
        {
            _codeBasedConfiguration.RegisterModulesWithDiManager();
            return new CodeBasedContainerStarter(_codeBasedConfiguration);
        }

        #endregion
    }
}