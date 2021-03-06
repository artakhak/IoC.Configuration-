==========================
XML Configuration Template
==========================

Here is a template XML configuration file that can be used to get started. This file can be found also in folder **IoC.Configuration.Content**, under the folder, where Nuget package **IoC.Configuration** is downloaded (see the screenshot below).
This file can also be downloaded from `IoC.Configuration.Template.xml <https://github.com/artakhak/IoC.Configuration/blob/master/IoC.Configuration/IoC.Configuration.Content/IoC.Configuration.Template.xml>`_

.. image:: ioc.configuration-files.jpg

**Template file:**

.. code-block:: xml
   :linenos:

        <?xml version="1.0" encoding="utf-8"?>
        <!--
           This is a simple sample configuration file to use with IoC.Configuration library.
           Some elements and attributes in this XML file should be modified per specific project.
           For more complete example, look at files IoCConfiguration_Overview.xml and some ther configuration files in test project
           IoC.Configuration.Tests at https://github.com/artakhak/IoC.Configuration/tree/master/IoC.Configuration.Tests.

           The XML configuration file is validated against schema file IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd,
           which can be found in folder IoC.Cnfiguration.Content in output directory.
           The schema file can also be downloaded from
           http://oroptimizer.com/ioc.configuration/IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd or in source code
           project in Github.com.

           To use Visual Studio code completion based on schema contents, right click Properties on this file in Visual Studio, and in Schemas
           field pick the schema IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd.
        -->

        <iocConfiguration>

            <!--The application should have write permissions to path specified in appDataDir. This is where dynamically generated DLLs are saved.-->
            <appDataDir path="C:\Users\user1\AppData\Local\MyApplication" />

            <plugins pluginsDirPath="c:\Program Files\MyApplication\DLLs\PluginDlls">
                <!--
                Plugin assemblies will be in a folder with similar name under pluginsDirPath folder.
                The plugin folders will be included in assembly resolution mechanism.
                -->

                <!--If Plugin1 is enabled, a folder c:\Program Files\MyApplication\DLLs\PluginDlls\Plugin1 should exist  -->
                <!--<plugin name="Plugin1" />-->
                <!--<plugin name="Plugin2" enabled="false" />-->

            </plugins>

            <additionalAssemblyProbingPaths>
                <probingPath path="c:\Program Files\MyApplication\DLLs\ThirdPartyLibs" />
            </additionalAssemblyProbingPaths>

            <assemblies>
                <!--Assemblies should be in one of the following locations:
                1) Executable's folder
                2) In folder specified in additionalAssemblyProbingPaths element.
                3) In one of the plugin folders specified in plugins element (only for assemblies with plugin attribute) -->
                <assembly name="OROptimizer.Shared" alias="oroptimizer_shared" />
                <assembly name="IoC.Configuration.Autofac" alias="autofac_ext" />
                <assembly name="IoC.Configuration.Ninject" alias="ninject_ext" />
            </assemblies>

            <parameterSerializers serializerAggregatorType="OROptimizer.Serializer.TypeBasedSimpleSerializerAggregator"
                                  assembly="oroptimizer_shared">
                <!--
                Use parameters element to specify constructor parameters, if the type specified in 'serializerAggregatorType' attribute
                has non-default constructor.
                -->
                <!--<parameters>
                </parameters>-->
                <serializers>
                </serializers>
            </parameterSerializers>

            <!--The value of type attribute should be a type that implements IoC.Configuration.DiContainer.IDiManager-->
            <diManagers activeDiManagerName="Autofac">
                <diManager name="Ninject" type="IoC.Configuration.Ninject.NinjectDiManager" assembly="ninject_ext">
                    <!--
                    Use parameters element to specify constructor parameters, if the type specified in 'type' attribute
                    has non-default constructor.
                    -->
                    <!--<parameters>
                    </parameters>-->
                </diManager>
                <diManager name="Autofac" type="IoC.Configuration.Autofac.AutofacDiManager" assembly="autofac_ext">
                </diManager>
            </diManagers>

            <!--
            If settingsRequestor element is used, the type in type attribute should specify a type that implements
            IoC.Configuration.ISettingsRequestor. The implementation specifies a collection of required settings that should be present
            in settings element.
            Note, the type specified in type attribute is fully integrated into a dependency injection framework. In other words, constructor
            parameters will be injected using bindings specified in dependencyInjection element.
            -->
            <!--<settingsRequestor type="MySettingsRequestor" assembly="my_assembly">
            </settingsRequestor>-->

            <settings>
                <!--Example:
                <int32 name="MySetting1" value="15"/>
                -->
            </settings>

            <dependencyInjection>
                <modules>
                </modules>
                <services>
                </services>
                <autoGeneratedServices>

                </autoGeneratedServices>
            </dependencyInjection>

            <startupActions>
            </startupActions>

            <pluginsSetup>
            </pluginsSetup>
        </iocConfiguration>