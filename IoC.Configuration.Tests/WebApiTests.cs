﻿using IoC.Configuration.ConfigurationFile;
using IoC.Configuration.DiContainerBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using TestsSharedLibrary;
using TestsSharedLibrary.Diagnostics.Log;

namespace IoC.Configuration.Tests
{
    [TestClass]
    public class WebApiTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TestsHelper.SetupLogger();
            Log4Tests.LogLevel = LogLevel.Debug;
        }

        [TestMethod]
        public void TestWebApiAssemblies()
        {
            LoadConfiguration((containerInfo, loadedConfiguration) =>
            {
                Assert.IsTrue(loadedConfiguration.WebApi != null && loadedConfiguration.WebApi.ControllerAssemblies != null &&
                              loadedConfiguration.WebApi.ControllerAssemblies.Assemblies != null);

                var controllerAssembliesList = loadedConfiguration.WebApi.ControllerAssemblies.Assemblies.ToList();

                Assert.AreEqual(1, controllerAssembliesList.Count);

                var controllerAssembly = controllerAssembliesList[0];

                var assemblyName = "TestProjects.DynamicallyLoadedAssembly1";

                Assert.AreEqual(assemblyName, controllerAssembly.Assembly.Name);
                Assert.IsNotNull(controllerAssembly.LoadedAssembly);
                Assert.AreEqual(assemblyName, controllerAssembly.LoadedAssembly.GetName().Name);
            },
            (sender, e) =>
            {
            });
        }


        [TestMethod]
        public void TestPluginWebApiAssemblies()
        {
            LoadConfiguration((containerInfo, loadedConfiguration) =>
            {
                var pluginSetup = loadedConfiguration.PluginsSetup.GetPluginSetup("Plugin1");

                Assert.IsTrue(pluginSetup.WebApi != null && pluginSetup.WebApi.ControllerAssemblies != null &&
                              pluginSetup.WebApi.ControllerAssemblies.Assemblies != null);

                var controllerAssembliesList = pluginSetup.WebApi.ControllerAssemblies.Assemblies.ToList();
                var controllerAssembly = controllerAssembliesList[0];

                var assemblyName = "TestProjects.TestPluginAssembly1";

                Assert.AreEqual(assemblyName, controllerAssembly.Assembly.Name);
                Assert.IsNotNull(controllerAssembly.LoadedAssembly);
                Assert.AreEqual(assemblyName, controllerAssembly.LoadedAssembly.GetName().Name);
            },
            (sender, e) =>
            {
            });
        }

        [TestMethod]
        public void TestPluginWebApiAssembliesForDisabledPlugin()
        {
            LoadConfiguration((containerInfo, loadedConfiguration) =>
            {
                // Lets make sure the disabled plugin cannot be accessed in the first place, which means
                // the controller assembles cannot be used.
                Assert.IsNull(loadedConfiguration.PluginsSetup.GetPluginSetup("Plugin3"));
                Assert.IsNull(loadedConfiguration.PluginsSetup.AllPluginSetups.FirstOrDefault(x => "Plugin3".Equals(x.Plugin.Name, StringComparison.Ordinal)));
               
                // Lets make sure that the assembly TestProjects.Plugin1WebApiControllers is not loaded into current domain.
                // TODO: Enable the next lines once we make improvements to load the configuration in two phases.
                // The first phase will load even the disabled plugin assemblies for validation purposes.
                // The second phase will load only enabled plugin assemblies.
                //Assert.IsNull(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => "TestProjects.Plugin3WebApiControllers".Equals(x.GetName().Name, StringComparison.OrdinalIgnoreCase)));
            },
            (sender, e) =>
            {
                //var element = e.XmlDocument.SelectElement("/iocConfiguration/plugins/plugin", x => x.GetAttribute(ConfigurationFileAttributeNames.Name) == "Plugin1");
                //element.SetAttributeValue(ConfigurationFileAttributeNames.Enabled, "false");

                //e.XmlDocument.SelectElement("/iocConfiguration/plugins/plugin",
                //    (element => element.GetAttribute(ConfigurationFileAttributeNames.Name).Equals("Plugin2", StringComparison.Ordinal)))
                //    .SetAttributeValue(ConfigurationFileAttributeNames.Enabled, "false");

                // Remove all Plugin2 parameter serializers since the configuration will fail if there are parameter serializers for disabled plugin
             
            });
        }

        private void LoadConfiguration(Action<IContainerInfo, IConfiguration> testConfiguration,
                                       ConfigurationFileXmlDocumentLoadedEventHandler configurationFileXmlDocumentLoaded)
        {
            var diContainerBuilder = new DiContainerBuilder.DiContainerBuilder();
            using (var containerInfo = diContainerBuilder.StartFileBasedDi(
                                                   new FileBasedConfigurationFileContentsProvider(
                                                   Path.Combine(Helpers.TestsEntryAssemblyFolder, "IoCConfiguration_Overview.xml")),
                                                   Helpers.TestsEntryAssemblyFolder,
                                                   out var loadedConfiguration,
                                                   configurationFileXmlDocumentLoaded)
                                               .WithoutPresetDiContainer()
                                               //.AddAdditionalDiModules(new SuccessfulConfigurationLoadTests.SuccessfulConfigurationLoadTests.TestModule2())
                                               .RegisterModules()
                                               .Start())
            {
                testConfiguration(containerInfo, loadedConfiguration);
            }
        }
    }
}
