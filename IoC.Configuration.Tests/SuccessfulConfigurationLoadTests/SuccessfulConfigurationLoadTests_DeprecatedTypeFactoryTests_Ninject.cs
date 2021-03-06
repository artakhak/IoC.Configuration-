﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsSharedLibrary.DependencyInjection;

namespace IoC.Configuration.Tests.SuccessfulConfigurationLoadTests
{
    [TestClass]
    public class SuccessfulConfigurationLoadTests_DeprecatedTypeFactoryTests_Ninject : SuccessfulConfigurationLoadTests_DeprecatedTypeFactoryTests
    {
        #region Member Functions

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ClassCleanupCommon();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassInitializeCommon(DiImplementationType.Ninject, null);
        }

        #endregion
    }
}