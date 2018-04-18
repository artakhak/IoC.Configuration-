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
using IoC.Configuration.DynamicCode;
using JetBrains.Annotations;
using OROptimizer.Serializer;

namespace IoC.Configuration
{
    /// <summary>
    /// A service factory.
    /// </summary>
    public interface IIoCServiceFactory
    {
        #region Current Type Interface

        /// <summary>
        ///     Creates an instance of <see cref="IAssemblyLocator" />.
        /// </summary>
        /// <param name="getConfugurationFunc">
        ///     A <see cref="System.Func{IConfiguration}" /> objects that returns an instance of
        ///     <see cref="IConfiguration" />
        /// </param>
        /// <param name="entryAssemblyFolder"></param>
        IAssemblyLocator CreateAssemblyLocator([NotNull] Func<IConfiguration> getConfugurationFunc, [NotNull] string entryAssemblyFolder);

        /// <summary>
        ///     Creates an instance of <see cref="ITypesListFactoryTypeGenerator" />
        /// </summary>
        ITypesListFactoryTypeGenerator CreateTypesListFactoryTypeGenerator([NotNull] ITypeBasedSimpleSerializerAggregator typeBasedSimpleSerializerAggregator);

        /// <summary>
        ///     Returns instance of <see cref="IProhibitedServiceTypesInServicesElementChecker" />.
        /// </summary>
        IProhibitedServiceTypesInServicesElementChecker GetProhibitedServiceTypesInServicesElementChecker();

        #endregion
    }
}