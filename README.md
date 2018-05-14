The main functions of IoC.Configuration library are:
1)  Container agnostic configuration of dependency injection using XML configuration file. The file has section where container can be specified, that will be handling dependency injection resolutions.
Currently two popular containers are supported, **Ninject** and **Autofac**, via extension libraries **IoC.Configuration.Ninject** and **IoC.Configuration.Autofac**, that are available in Nuget.org.
The dependency injection container (e.g., **Autofac**, **Ninject**) can be easily switched in configuration file.
In addition, the configuration file has sections for settings, plugins, startup actions, dynamically generated implementations of interfaces (see element **iocConfiguration/dependencyInjection/autoGeneratedServices/typeFactory** in example configuration files in GitHub test project [IoC.Configuration.Tests](https://github.com/artakhak/IoC.Configuration/tree/master/IoC.Configuration.Tests)).
  
2) Container agnostic configuration of dependency injection in code. 
***
The bindings are specified using **IoC.Configuration** chained methods, however the actual resolutions are done using one of the popular dependency injection containers, **Ninject** and **Autofac**, via extension libraries **IoC.Configuration.Ninject** and **IoC.Configuration.Autofac**.
***

## Configuration
Dependency injection can be configured either using file based or code based configuration builder.


Bindings can be specified in three different ways:

-In configuration file (for file based configuration only).

-Using native modules (for example using Autofac and Ninject modules).
 
-By extending the class  IoC.Configuration.DiContainer.ModuleAbstr and overriding the method AddServiceRegistrations() or by implementing IoC.Configuration.IDiModule.

In file based configuration, the modules (Autofac, Ninject, or implementations of IoC.Configuration.IDiModule) can be specified in configuration file in element iocConfiguration/dependencyInjection/modules/module

In code based configuration, the modules can be passed as parameters in chained configuration methods (see the section Code Based Configuration).
 
**Below is an example of specifying bindings in AddServiceRegistrations() method in a subclass of IoC.Configuration.DiContainer.ModuleAbstr (note the example is taken from test project so the class names are not user friendly).**
![](http://oroptimizer.com/IoC.Configuration/GitHubDocs/ExampleDIModule1.png)
      
## File Based Configuration
Example of configuration file is shown below.

Here is an example of configuring and starting the container:

![](http://oroptimizer.com/IoC.Configuration/GitHubDocs/FileBasedConfiguration1.png)

**The configuration file IoCConfiguration1.xml**

<?xml version="1.0" encoding="utf-8"?>
<!--
   The XML configuration file is validated against schema file IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd, 
   which can be found in folder IoC.Configuration.Content in output directory. 
   The schema file can also be downloaded from 
   http://oroptimizer.com/ioc.configuration/IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd or in source code 
   project in Github.com.
   
   To use Visual Studio code completion based on schema contents, right click Properties on this file in Visual Studio, and in Schemas 
   field pick the schema IoC.Configuration.Schema.2F7CE7FF-CB22-40B0-9691-EAC689C03A36.xsd.

    Before running the test make sure to execute IoC.Configuration\Tests\IoC.Configuration.Tests\PostBuildCommands.bat to copy the dlls into 
    folders specified in this configuration file.
    Also, modify the batch file to copy the Autofac and Ninject assemblies from Nuget packages folder on machine, where the test is run.
-->

<iocConfiguration>

    <!--The application should have write permissions to path specified in appDataDir. This is where dynamically generated DLLs are saved.-->
    <appDataDir
        path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\bin\TestFiles\DynamicFiles" />

    <plugins pluginsDirPath="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\PluginDlls">

        <!--        
        Plugin assemblies will be in a folder with similar name under pluginsDirPath folder.
        The plugin folders will be included in assembly resolution mechanism.        
        -->

        <!--A folder K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\Tests\IoC.Configuration.Tests\TestDlls\PluginDlls\Plugin1 should exist.  -->
        <plugin name="Plugin1" />
        <plugin name="Plugin2" />
        <plugin name="Plugin3" enabled="false" />
    </plugins>

    <additionalAssemblyProbingPaths>
        <probingPath
            path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\ThirdPartyLibs" />
        <probingPath
            path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\ContainerImplementations\Autofac" />
        <probingPath
            path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\ContainerImplementations\Ninject" />

        <probingPath
            path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\DynamicallyLoadedDlls" />
        <probingPath
            path="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\TestAssemblyResolution" />
    </additionalAssemblyProbingPaths>

    <assemblies>
        <!--Assemblies should be in one of the following locations:
        1) Executable's folder
        2) In folder specified in additionalAssemblyProbingPaths element.
        3) In one of the plugin folders specified in plugins element (only for assemblies with plugin attribute) -->

        <!--
        Use loadAlways to make sure a reference to assembly is added to dynamically generated assembly for dependencies,
        even if the assembly is not referenced anywhere in configuration file.
        In general, this is not necessary, however in case if generating dynamic assembly fails because of missing .NET assemblies,
        using this field might help.
        Use "overrideDirectory" attribute, to make the assembly path explicit, rather then searching for an assembly in predefined folders, which also include
        probing paths specified in additionalAssemblyProbingPaths element.
        -->

        <assembly name="TestProjects.TestForceLoadAssembly" alias="TestForceLoadAssembly" loadAlways="true"
                  overrideDirectory="K:\Projects\OROptimizer\MyGitHubProjects\IoC.Configuration\IoC.Configuration.Tests\TestDlls\DynamicallyLoadedDlls" />

        <assembly name="OROptimizer.Shared" alias="oroptimizer_shared" />
        <assembly name="IoC.Configuration" alias="ioc_config" />
        <assembly name="IoC.Configuration.Autofac" alias="autofac_ext" />
        <assembly name="IoC.Configuration.Ninject" alias="ninject_ext" />

        <assembly name="TestProjects.Modules" alias="modules" />
        <assembly name="TestProjects.DynamicallyLoadedAssembly1" alias="dynamic1" />
        <assembly name="TestProjects.DynamicallyLoadedAssembly2" alias="dynamic2" />

        <assembly name="TestProjects.TestPluginAssembly1" alias="pluginassm1" plugin="Plugin1" />
        <assembly name="TestProjects.ModulesForPlugin1" alias="modules_plugin1" plugin="Plugin1" />

        <assembly name="TestProjects.TestPluginAssembly2" alias="pluginassm2" plugin="Plugin2" />
        <assembly name="TestProjects.TestPluginAssembly3" alias="pluginassm3" plugin="Plugin3" />

        <assembly name="TestProjects.SharedServices" alias="shared_services" />
        <assembly name="IoC.Configuration.Tests" alias="tests" />
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
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerDouble"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerLong"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerInt"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerShort"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerByte"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerBoolean"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerDateTime"
                                 assembly="oroptimizer_shared" />
            <parameterSerializer type="OROptimizer.Serializer.TypeBasedSimpleSerializerString"
                                 assembly="oroptimizer_shared" />

            <parameterSerializer type="TestPluginAssembly1.Implementations.DoorSerializer" assembly="pluginassm1" />
            <parameterSerializer type="TestPluginAssembly2.Implementations.WheelSerializer" assembly="pluginassm2" />

            <parameterSerializer type="TestPluginAssembly1.Implementations.UnsignedIntSerializerWithParameters"
                                 assembly="pluginassm1">
                <parameters>
                    <int32 name="param1" value="25" />
                    <double name="param2" value="36.5" />
                </parameters>
            </parameterSerializer>
        </serializers>
    </parameterSerializers>

    <!--The value of type attribute should be a type that implements IoC.Configuration.DiContainer.IDiManager-->
    <diManagers activeDiManagerName="Autofac">
        <diManager name="Ninject" type="IoC.Configuration.Ninject.NinjectDiManager" assembly="ninject_ext">
            <!--
            Use parameters element to specify constructor parameters, if the type specified in 'type' attribute
            has non-default constructor.-->
            <!--<parameters>
            </parameters>-->
        </diManager>

        <diManager name="Autofac" type="IoC.Configuration.Autofac.AutofacDiManager" assembly="autofac_ext">
        </diManager>
    </diManagers>

    <!--
    If settingsRequestor element is used, the type in type attribute should specify a type that implements
    SharedServices.ISettingsRequestor. The implementation specifies a collection of requierd settings that shiuld be present
    in settings element.
    Note, the type specified in type attribute is fully integrated into a dependency injection framework. In other words, constructor
    parameters will be injected using bindings specified in dependencyInjection element.
    -->
    <settingsRequestor type="SharedServices.FakeSettingsRequestor" assembly="shared_services">
    </settingsRequestor>

    <settings>
        <int32 name="SynchronizerFrequencyInMilliseconds" value="5000" />
        <double name="MaxCharge" value="155.7" />
        <string name="DisplayValue" value="Some display value" />
    </settings>

    <dependencyInjection>
        <modules>
            <module type="IoC.Configuration.Tests.PrimitiveTypeDefaultBindingsModule" assembly="tests">
                <parameters>
                    <datetime name="defaultDateTime" value="1915-04-24 00:00:00.000" />
                    <double name="defaultDouble" value="0" />
                    <int16 name="defaultInt16" value="0" />
                    <int32 name="defaultInt32" value="0" />
                </parameters>
            </module>

            <module type="Modules.Autofac.AutofacModule1" assembly="modules">
                <parameters>
                    <int32 name="param1" value="1" />
                </parameters>
            </module>

            <module type="Modules.IoC.DiModule1" assembly="modules">
                <parameters>
                    <int32 name="param1" value="2" />
                </parameters>
            </module>

            <module type="Modules.Ninject.NinjectModule1" assembly="modules">
                <parameters>
                    <int32 name="param1" value="3" />
                </parameters>
            </module>
        </modules>
        <services>
            <service type="DynamicallyLoadedAssembly1.Interfaces.IInterface1" assembly="dynamic1">
                <implementation type="DynamicallyLoadedAssembly1.Implementations.Interface1_Impl1" assembly="dynamic1"
                                scope="singleton">
                </implementation>
            </service>

            <service type="DynamicallyLoadedAssembly1.Interfaces.IInterface2" assembly="dynamic1">
                <implementation type="DynamicallyLoadedAssembly1.Implementations.Interface2_Impl1" assembly="dynamic1"
                                scope="transient">
                </implementation>
            </service>

            <service type="DynamicallyLoadedAssembly1.Interfaces.IInterface3" assembly="dynamic1">
                <implementation type="DynamicallyLoadedAssembly1.Implementations.Interface3_Impl1" assembly="dynamic1"
                                scope="scopeLifetime">
                </implementation>
            </service>

            <!--
            Test DI picking the default constructor when instantiating the implementation, if parameters element is present, and using non-default
            constructor otherwise, with injected parameters)
            -->
            <service type="SharedServices.Interfaces.IInterface9" assembly="shared_services">
                <implementation type="SharedServices.Implementations.Interface9_Impl1" assembly="shared_services"
                                scope="singleton" />
            </service>
            <service type="SharedServices.Interfaces.IInterface8" assembly="shared_services">
                <implementation type="SharedServices.Implementations.Interface8_Impl1" assembly="shared_services"
                                scope="singleton">
                    <!--
                    Since parameters is present, a default constructor will be used to construct an object, even though 
                    Interface8_Impl1 has also a non default constructor
                    -->
                    <parameters>
                    </parameters>
                </implementation>

                <implementation type="SharedServices.Implementations.Interface8_Impl2" assembly="shared_services"
                                scope="singleton">
                    <!--
                    Since parameters is not present, DI will pick a constructor with maximum number of parameters.
                    Note, Interface8_Impl2 has two constructors, a default one, and a constructor with parameters.
                    -->
                </implementation>
            </service>

            <!--Injected constructor parameters with self bound services-->
            <selfBoundService type="DynamicallyLoadedAssembly1.Implementations.SelfBoundService1" assembly="dynamic1"
                              scope="singleton">
                <parameters>
                    <int32 name="param1" value="14" />
                    <double name="param2" value="15.3" />
                    <injectedObject name="param3" type="DynamicallyLoadedAssembly1.Interfaces.IInterface1"
                                    assembly="dynamic1" />
                </parameters>
            </selfBoundService>

            <!--Injected properties with self bound services-->
            <selfBoundService type="DynamicallyLoadedAssembly1.Implementations.SelfBoundService2" assembly="dynamic1"
                              scope="transient">
                <injectedProperties>
                    <int32 name="Property1" value="17" />
                    <double name="Property2" value="18.1" />
                    <injectedObject name="Property3" type="DynamicallyLoadedAssembly1.Interfaces.IInterface1"
                                    assembly="dynamic1" />
                </injectedProperties>
            </selfBoundService>

            <!--Life time scope with self bound services-->
            <selfBoundService type="DynamicallyLoadedAssembly1.Implementations.SelfBoundService3" assembly="dynamic1"
                              scope="scopeLifetime">
            </selfBoundService>

            <!--Test circular references between SharedServices.Interfaces.IInterface3 and SharedServices.Interfaces.IInterface4-->
            <service type="SharedServices.Interfaces.IInterface3" assembly="shared_services">
                <implementation type="SharedServices.Implementations.Interface3_Impl1" assembly="shared_services"
                                scope="singleton">
                    <injectedProperties>
                        <injectedObject name="Property2" type="SharedServices.Interfaces.IInterface4"
                                        assembly="shared_services" />
                    </injectedProperties>
                </implementation>
            </service>
            <service type="SharedServices.Interfaces.IInterface4" assembly="shared_services">
                <implementation type="SharedServices.Implementations.Interface4_Impl1" assembly="shared_services"
                                scope="singleton">
                </implementation>
            </service>

            <!--Injected constructor parameters-->
            <service type="SharedServices.Interfaces.IInterface2" assembly="shared_services">
                <!--Test constructor parameters-->
                <implementation type="SharedServices.Implementations.Interface2_Impl1" assembly="shared_services"
                                scope="singleton">
                    <parameters>
                        <!--The value will be de-serialized using serializer TypeBasedSimpleSerializerDateTime in parameterSerializers section.-->
                        <datetime name="param1" value="2014-10-29 23:59:59.099" />
                        <double name="param2" value="125.1" />
                        <injectedObject name="param3" type="SharedServices.Interfaces.IInterface3"
                                        assembly="shared_services" />
                    </parameters>
                </implementation>

                <!--Test injected properties-->
                <implementation type="SharedServices.Implementations.Interface2_Impl2" assembly="shared_services"
                                scope="singleton">
                    <injectedProperties>
                        <!--The value of param2 will be de-serialized using serializer TypeBasedSimpleSerializerDateTime in parameterSerializers section.-->
                        <datetime name="Property1" value="1915-04-24 00:00:00.001" />
                        <double name="Property2" value="365.41" />
                        <injectedObject name="Property3" type="SharedServices.Interfaces.IInterface3"
                                        assembly="shared_services" />
                    </injectedProperties>
                </implementation>

                <!--Test constructor parameters with injected properties. Constructor values will be overridden by injected properties.-->
                <implementation type="SharedServices.Implementations.Interface2_Impl3" assembly="shared_services"
                                scope="singleton">
                    <parameters>
                        <!--The value will be de-serialized using serializer TypeBasedSimpleSerializerDateTime in parameterSerializers section.-->
                        <datetime name="param1" value="2017-10-29 23:59:59.099" />
                        <double name="param2" value="138.3" />

                        <!--
                        Inject specific implementation. Note, there is no binding IInterface3 to Interface3_Impl2.
                        Using injectedObject, we can specify the injected type other than the one normally injected for 
                        SharedServices.Interfaces.IInterface3.
                        -->
                        <injectedObject name="param3" type="SharedServices.Implementations.Interface3_Impl2"
                                        assembly="shared_services" />
                    </parameters>
                    <injectedProperties>
                        <double name="Property2" value="148.3" />
                        <!--
                        Inject specific implementation. Note, there is no binding IInterface3 to Interface3_Impl3.
                        Using injectedObject, we can specify the injected type other than the one normally injected for 
                        SharedServices.Interfaces.IInterface3.
                        -->
                        <injectedObject name="Property3" type="SharedServices.Implementations.Interface3_Impl3"
                                        assembly="shared_services" />
                    </injectedProperties>
                </implementation>

                <!--Test injected constructor parameters. Primitive type constructor parameters, such as DateTime and double,
                    will be injected with default values specified in module: IoC.Configuration.Tests.PrimitiveTypeDefaultBindingsModule.
                 -->
                <implementation type="SharedServices.Implementations.Interface2_Impl4" assembly="shared_services"
                                scope="singleton">
                </implementation>
            </service>

            <selfBoundService type="DynamicallyLoadedAssembly2.ActionValidator3" assembly="dynamic2" scope="transient">
                <parameters>
                    <int32 name="intParam" value="5" />
                </parameters>
            </selfBoundService>

            <selfBoundService type="DynamicallyLoadedAssembly1.Implementations.CleanupJob2" assembly="dynamic1"
                              scope="transient">
                <parameters>
                    <injectedObject name="cleanupJobData"
                                    type="DynamicallyLoadedAssembly1.Implementations.CleanupJobData2"
                                    assembly="dynamic1" />
                </parameters>
            </selfBoundService>

            <selfBoundService type="DynamicallyLoadedAssembly1.Implementations.CleanupJob3" assembly="dynamic1"
                              scope="singleton">
                <injectedProperties>
                    <injectedObject name="CleanupJobData"
                                    type="DynamicallyLoadedAssembly1.Implementations.CleanupJobData2"
                                    assembly="dynamic1" />
                </injectedProperties>
            </selfBoundService>

            <service type="SharedServices.Interfaces.ICleanupJobData" assembly="shared_services">
                <implementation type="DynamicallyLoadedAssembly1.Implementations.CleanupJobData" assembly="dynamic1"
                                scope="singleton">
                </implementation>
            </service>

            <!--Service implemented by plugins-->
            <service type="SharedServices.Interfaces.IInterface5" assembly="shared_services">
                <implementation type="SharedServices.Implementations.Interface5_Impl1" assembly="shared_services"
                                scope="singleton" />
                <implementation type="TestPluginAssembly1.Implementations.Interface5_Plugin1Impl"
                                assembly="pluginassm1" scope="singleton" />
                <implementation type="TestPluginAssembly2.Implementations.Interface5_Plugin2Impl"
                                assembly="pluginassm2" scope="transient" />
                <implementation type="TestPluginAssembly3.Implementations.Interface5_Plugin3Impl"
                                assembly="pluginassm3" scope="transient" />
            </service>

            <!--
            Test registerIfNotRegistered. Note, SharedServices.Interfaces.IInterface6 is already registered in module  Modules.IoC.DiModule1 
            for implementation SharedServices.Implementations.Interface6_Impl1.
            Therefore, implementation SharedServices.Implementations.Interface6_Impl2 will not be registered.            
            -->
            <service type="SharedServices.Interfaces.IInterface6" assembly="shared_services"
                     registerIfNotRegistered="true">
                <implementation type="SharedServices.Implementations.Interface6_Impl2" assembly="shared_services"
                                scope="singleton" />
            </service>

            <!--
            Note, service SharedServices.Interfaces.IInterface7 was not registered beforw. Therefore its implementations
            registered below will be registered.
            -->
            <service type="SharedServices.Interfaces.IInterface7" assembly="shared_services"
                     registerIfNotRegistered="true">
                <implementation type="SharedServices.Implementations.Interface7_Impl1" assembly="shared_services"
                                scope="singleton" />
            </service>

            <selfBoundService type="SharedServices.Implementations.SelfBoundService1" assembly="shared_services"
                              registerIfNotRegistered="true" scope="singleton">
            </selfBoundService>
        </services>
        <autoGeneratedServices>
            <!--The scope for typeFactory implementations is always singleton -->
            <!--The function in DynamicallyLoadedAssembly2.IActionValidatorFactory1 that this configuration implements has the following signature
            
            IEnumerable<DynamicallyLoadedAssembly1.IActionValidator> GetInstances(int param1, string param2);
            The type attribute value in returnedType element should be a concrete class (non-abstract and non-interface), that implements
            DynamicallyLoadedAssembly1.IActionValidator.
            Attributes parameter1 and parameter2 can be set to specify conditions when specific type instances will be returned.            
            -->
            <typeFactory interface="DynamicallyLoadedAssembly2.IActionValidatorFactory1" assembly="dynamic2">
                <if parameter2="project1" parameter1="1">
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator3" assembly="dynamic2" />
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator1" assembly="dynamic2" />
                </if>
                <if parameter1="1" parameter2="project2">
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator1" assembly="dynamic2" />
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator2" assembly="dynamic2" />
                </if>
                <if parameter1="2">
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator1" assembly="dynamic2" />
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator2" assembly="dynamic2" />
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator3" assembly="dynamic2" />
                </if>
                <default>
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator2" assembly="dynamic2" />
                    <returnedType type="DynamicallyLoadedAssembly2.ActionValidator1" assembly="dynamic2" />
                </default>
            </typeFactory>

            <!--The scope for typeFactory implementations is always singleton -->
            <!--
            The function in SharedServices.Interfaces.ICleanupJobFactory that this configuration implements has the following signature            
            IEnumerable<SharedServices.Interfaces.ICleanupJob> GetCleanupJobs(int projectId);
                        
            The type attribute value in returnedType element should be a concrete class (non-abstract and non-interface), that implements
            SharedServices.Interfaces.ICleanupJob.
            Attribute parameter1 can be set to specify conditions when specific type instances will be returned.            
            -->

            <typeFactory interface="SharedServices.Interfaces.ICleanupJobFactory" assembly="shared_services">
                <if parameter1="1">
                    <returnedType type="DynamicallyLoadedAssembly1.Implementations.CleanupJob1" assembly="dynamic1" />
                    <returnedType type="DynamicallyLoadedAssembly1.Implementations.CleanupJob2" assembly="dynamic1" />
                </if>
                <if parameter1="2">
                    <returnedType type="DynamicallyLoadedAssembly1.Implementations.CleanupJob2" assembly="dynamic1" />
                </if>
                <default>
                    <returnedType type="DynamicallyLoadedAssembly1.Implementations.CleanupJob1" assembly="dynamic1" />
                    <returnedType type="DynamicallyLoadedAssembly1.Implementations.CleanupJob3" assembly="dynamic1" />
                </default>
            </typeFactory>
        </autoGeneratedServices>
    </dependencyInjection>

    <startupActions>
        <startupAction type="DynamicallyLoadedAssembly1.Implementations.StartupAction1" assembly="dynamic1"></startupAction>
        <startupAction type="DynamicallyLoadedAssembly1.Implementations.StartupAction2" assembly="dynamic1"></startupAction>
    </startupActions>

    <pluginsSetup>
        <pluginSetup plugin="Plugin1">
            <!--type in pluginImplementation should be a concrete class that implements IoC.Configuration.IPlugin-->
            <pluginImplementation type="TestPluginAssembly1.Implementations.Plugin1" assembly="pluginassm1">
                <parameters>
                    <int64 name="param1" value="25" />
                </parameters>
            </pluginImplementation>
            <settings>
                <int32 name="Int32Setting1" value="25" />
                <int64 name="Int64Setting1" value="38" />
                <string name="StringSetting1" value="String Value 1" />
            </settings>
            <dependencyInjection>
                <modules>
                    <!--TODO: Test the case when the module is not in an assembly in Plugin folder.-->
                    <module type="ModulesForPlugin1.Ninject.NinjectModule1" assembly="modules_plugin1">
                        <parameters>
                            <int32 name="param1" value="101" />
                        </parameters>
                    </module>

                    <module type="ModulesForPlugin1.Autofac.AutofacModule1" assembly="modules_plugin1">
                        <parameters>
                            <int32 name="param1" value="102" />
                        </parameters>
                    </module>

                    <module type="ModulesForPlugin1.IoC.DiModule1" assembly="modules_plugin1">
                        <parameters>
                            <int32 name="param1" value="103" />
                        </parameters>
                    </module>
                </modules>
                <services>
                    <service type="TestPluginAssembly1.Interfaces.IDoor" assembly="pluginassm1">
                        <implementation type="TestPluginAssembly1.Implementations.Door" assembly="pluginassm1"
                                        scope="transient">
                            <parameters>
                                <int32 name="Color" value="3" />
                                <double name="Height" value="180" />
                            </parameters>
                        </implementation>
                    </service>
                    <service type="TestPluginAssembly1.Interfaces.IRoom" assembly="pluginassm1">
                        <implementation type="TestPluginAssembly1.Implementations.Room" assembly="pluginassm1"
                                        scope="transient">
                            <parameters>
                                <object name="door1" type="TestPluginAssembly1.Interfaces.IDoor" assembly="pluginassm1"
                                        value="5,185.1" />
                                <injectedObject name="door2" type="TestPluginAssembly1.Interfaces.IDoor"
                                                assembly="pluginassm1" />
                            </parameters>
                            <injectedProperties>
                                <object name="Door2" type="TestPluginAssembly1.Interfaces.IDoor" assembly="pluginassm1"
                                        value="7,187.3" />
                            </injectedProperties>
                        </implementation>
                    </service>
                </services>
                <autoGeneratedServices>
                    <!--The scope for typeFactory implementations is always singleton -->
                    <!--The function in TestPluginAssembly1.Interfaces.IResourceAccessValidatorFactory that this configuration implements has the following signature
            
                        IEnumerable<TestPluginAssembly1.Interfaces.IResourceAccessValidator> GetValidators(string resourceName);
                        The type attribute value in returnedType element should be a concrete class (non-abstract and non-interface), that implements
                        TestPluginAssembly1.Interfaces.IResourceAccessValidator.
                        Attribute parameter1 can be set to specify conditions when specific type instances will be returned.            
                    -->
                    <typeFactory interface="TestPluginAssembly1.Interfaces.IResourceAccessValidatorFactory"
                                 assembly="pluginassm1">
                        <if parameter1="public_pages">
                            <returnedType type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"
                                          assembly="pluginassm1" />
                        </if>
                        <if parameter1="admin_pages">
                            <returnedType type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"
                                          assembly="pluginassm1" />
                            <returnedType type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"
                                          assembly="pluginassm1" />
                        </if>
                        <default>
                            <returnedType type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"
                                          assembly="pluginassm1" />
                            <returnedType type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"
                                          assembly="pluginassm1" />
                        </default>
                    </typeFactory>
                </autoGeneratedServices>
            </dependencyInjection>
        </pluginSetup>

        <pluginSetup plugin="Plugin2">
            <pluginImplementation type="TestPluginAssembly2.Implementations.Plugin2" assembly="pluginassm2">
                <parameters>
                    <boolean name="param1" value="true" />
                    <double name="param2" value="25.3" />
                    <string name="param3" value="String value" />
                </parameters>
                <injectedProperties>
                    <double name="Property2" value="5.3" />
                </injectedProperties>
            </pluginImplementation>
            <settings>

            </settings>
            <dependencyInjection>
                <modules>

                </modules>
                <services>
                    <service type="TestPluginAssembly2.Interfaces.IWheel" assembly="pluginassm2">
                        <implementation type="TestPluginAssembly2.Implementations.Wheel" assembly="pluginassm2"
                                        scope="transient">
                            <parameters>
                                <int32 name="Color" value="5" />
                                <double name="Height" value="48" />
                            </parameters>
                        </implementation>
                    </service>
                    <service type="TestPluginAssembly2.Interfaces.ICar" assembly="pluginassm2">
                        <implementation type="TestPluginAssembly2.Implementations.Car" assembly="pluginassm2"
                                        scope="transient">
                            <parameters>
                                <object name="wheel1" type="TestPluginAssembly2.Interfaces.IWheel"
                                        assembly="pluginassm2" value="248,40" />
                            </parameters>
                            <injectedProperties>
                                <object name="Wheel1" type="TestPluginAssembly2.Interfaces.IWheel"
                                        assembly="pluginassm2" value="27,45" />
                                <injectedObject name="Wheel2" type="TestPluginAssembly2.Interfaces.IWheel"
                                                assembly="pluginassm2" />
                            </injectedProperties>
                        </implementation>
                    </service>
                </services>
                <autoGeneratedServices>

                </autoGeneratedServices>
            </dependencyInjection>
        </pluginSetup>

        <pluginSetup plugin="Plugin3">
            <pluginImplementation type="TestPluginAssembly3.Implementations.Plugin3" assembly="pluginassm3">

            </pluginImplementation>
            <settings></settings>
            <dependencyInjection>
                <modules>
                </modules>
                <services>
                </services>
                <autoGeneratedServices>
                </autoGeneratedServices>
            </dependencyInjection>
        </pluginSetup>
    </pluginsSetup>
</iocConfiguration>

## Code Based Configuration
Code based configuration is pretty similar to file based configuration, except there is no configuration file. 
All dependencies are bound in IoC.Configuration modules (i.e., instances IoC.Configuration.DiContainer.IDiModule) native modules (e.g., instances of  Autofac.AutofacModule or Ninject.Modules.NinjectModule)

Here is an example ofcode based configuration.

![](http://oroptimizer.com/IoC.Configuration/GitHubDocs/CodeBasedConfiguration1.png)

## Native and IoC.Configuration Modules in Configuration File.
Both native modules (e.g., subclasses of Autofac.AutofacModule or Ninject.Modules.NinjectModule) and IoC.Configuration modules can be specified in configuration files. 

Here is an xample from configuration file above which has both native and container agnostic IoC.Configuration modules.

![](http://oroptimizer.com/IoC.Configuration/GitHubDocs/NativeModulesInConfigurationFile.png)