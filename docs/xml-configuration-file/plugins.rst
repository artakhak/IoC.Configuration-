=======
Plugins
=======

XML Configuration file has elements to specify plugins.
Plugins are extensions that allow specifying additional type bindings, plugin specific settings and some other functionality:

.. contents::
  :local:
  :depth: 2


Adding a Plugin
===============
To add a plugin do the following:

1) Add a child element **plugin** to **iocConfiguration/plugins** and specify the plugin name using the **name** attribute. See the example below:

    .. note::
        All the types related to plugins should be in assemblies that are in a directory **[plugins directoy]\[plugin name]**, where **[plugins directoy]** is the directory specified in attribute **pluginsDirPath** of element **iocConfiguration/plugins** and **[plugin name]** is the value of attribute **name** of element **iocConfiguration/plugins/plugin**.

    .. code-block:: xml

        <plugins pluginsDirPath="K:\...\TestDlls\PluginDlls">

            <!--
            Plugin assemblies will be in a folder with similar name under pluginsDirPath folder.
            The plugin folders will be included in assembly resolution mechanism.
            -->

            <!--A folder K:\...\TestDlls\PluginDlls\Plugin1 should exist.  -->
            <plugin name="Plugin1" />
            <plugin name="Plugin2" />
            <plugin name="Plugin3" enabled="false" />
        </plugins>

2) Add a child element **pluginSetup** to **iocConfiguration/pluginsSetup** (**iocConfiguration/pluginsSetup** is next to **iocConfiguration/startupActions** element), and make sure to use the same value for **plugin** attribute as the value of attribute **name** in **plugin** element mentioned in step 1).

- Here is an example of **iocConfiguration/pluginsSetup/pluginSetup** element with explanation of some elements in **pluginSetup** element.

.. code-block:: xml
    :linenos:

    <pluginsSetup>
        <pluginSetup plugin="Plugin1">
            <!--The type in pluginImplementation should be non-abstract class
                      that implements IoC.Configuration.IPlugin and which has a public constructor-->
            <pluginImplementation type="TestPluginAssembly1.Implementations.Plugin1">
                <parameters>
                    <int64 name="param1" value="25" />
                </parameters>
                <injectedProperties>
                    <int64 name="Property2" value="35"/>
                </injectedProperties>
            </pluginImplementation>

            <settings>
                <int32 name="Int32Setting1" value="25" />
                <int64 name="Int64Setting1" value="38" />
                <string name="StringSetting1" value="String Value 1" />
            </settings>

            <webApi>
                <controllerAssemblies>
                    <!--
                    Specify assemblies with API controllers.
                    The user of IoC.Configuration should add the assemblies to MVC using
                    IMvcBuilder.AddApplicationPart(System.Reflection.Assembly)
                    -->
                    <controllerAssembly assembly="pluginassm1" />
                    <controllerAssembly assembly="plugin1api" />
                </controllerAssemblies>
            </webApi>

            <dependencyInjection>
                <modules>
                    <module type="ModulesForPlugin1.Ninject.NinjectModule1">
                        <parameters>
                            <int32 name="param1" value="101" />
                        </parameters>
                    </module>
                    <module type="ModulesForPlugin1.Autofac.AutofacModule1">
                        <parameters>
                            <int32 name="param1" value="102" />
                        </parameters>
                    </module>
                    <module type="ModulesForPlugin1.IoC.DiModule1">
                        <parameters>
                            <int32 name="param1" value="103" />
                        </parameters>
                    </module>
                </modules>

                <services>
                    <service type="TestPluginAssembly1.Interfaces.IDoor">
                        <implementation type="TestPluginAssembly1.Implementations.Door"
                                        scope="transient">
                            <parameters>
                                <int32 name="Color" value="3" />
                                <double name="Height" value="180" />
                            </parameters>
                        </implementation>
                    </service>

                    <service type="TestPluginAssembly1.Interfaces.IRoom">
                        <implementation type="TestPluginAssembly1.Implementations.Room"
                                        scope="transient">
                            <parameters>
                                <object name="door1" type="TestPluginAssembly1.Interfaces.IDoor"
                                        value="5,185.1" />
                                <injectedObject name="door2" type="TestPluginAssembly1.Interfaces.IDoor" />
                            </parameters>
                            <injectedProperties>
                                <object name="Door2" type="TestPluginAssembly1.Interfaces.IDoor"
                                        value="7,187.3" />
                            </injectedProperties>
                        </implementation>
                    </service>
                </services>

                <autoGeneratedServices>
                    <!--The scope for autoService implementations is always singleton -->
                    <autoService interface="TestPluginAssembly1.Interfaces.IResourceAccessValidatorFactory">
                        <autoMethod name="GetValidators"
                                    returnType="System.Collections.Generic.IEnumerable[TestPluginAssembly1.Interfaces.IResourceAccessValidator]"
                                    reuseValue="true" >
                            <methodSignature>
                                <string paramName="resourceName"/>
                            </methodSignature>
                            <if parameter1="public_pages">
                                <collection>
                                    <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                                </collection>
                            </if>
                            <if parameter1="admin_pages">
                                <collection>
                                    <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                                    <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"/>
                                </collection>
                            </if>
                            <default>
                                <collection>
                                    <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"/>
                                    <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                                </collection>
                            </default>
                        </autoMethod>
                    </autoService>
                </autoGeneratedServices>
            </dependencyInjection>
        </pluginSetup>
    </pluginsSetup>

Element **pluginImplementation**
================================

The element **iocConfiguration/pluginsSetup/pluginSetup/pluginImplementation** is used to specify an implementation of interface **IoC.Configuration.IPlugin** for the plugin.
The easiest way to provide an implementation of **IoC.Configuration.IPlugin** is to extend the abstract class **IoC.Configuration.PluginAbstr** and to override the abstract methods **IoC.Configuration.PluginAbstr.Initialize()** and **IoC.Configuration.PluginAbstr.Dispose()**. **PluginAbstr** implements **IoC.Configuration.IPlugin**.

.. note::
    Plugins are integrated into dependency injection mechanism. Therefore, the constructor parameters of **IoC.Configuration.IPlugin** implementations specified in **pluginImplementation** elements will be injected using the bindings specified in XML Configuration file or in modules referenced by the configuration file. Also, **parameters** and **injectedProperties** elements can used with **pluginImplementation** element to specify constructor parameters or property injection.

Here is an example of implementation of **IoC.Configuration.IPlugin** interface that is referenced in element **iocConfiguration/pluginsSetup/pluginSetup/pluginImplementation** in example above:

.. code-block:: csharp

    public class Plugin1 : IoC.Configuration.PluginAbstr
    {
        private readonly List<SettingInfo> _requiredSettings;

        public Plugin1(long param1)
        {
            Property1 = param1;
            _requiredSettings = new List<SettingInfo>();
            _requiredSettings.Add(new SettingInfo("Int32Setting1", typeof(int)));
            _requiredSettings.Add(new SettingInfo("StringSetting1", typeof(string)));
        }

        public override IEnumerable<SettingInfo> RequiredSettings => _requiredSettings;

        public override void Dispose()
        {
            // Dispose resources
        }

        public override void Initialize()
        {
            // Do initialization here
        }

        public long Property1 { get; }
        public long Property2 { get; set; }
    }

Getting Plugin Data at Runtime
------------------------------

- To access an instance of **IoC.Configuration.IPlugin** for specific plugin, inject type **IoC.Configuration.IPluginDataRepository** (using constructor or property injection), and use the method **IPluginData GetPluginData(string pluginName)** in interface **IoC.Configuration.IPluginDataRepository**.

An example is demonstrated below:

.. code-block:: csharp

    public class AccessPluginDataExample
    {
        public AccessPluginDataExample(IoC.Configuration.IPluginDataRepository pluginDataRepository)
        {
            var pluginData = pluginRepository.GetPluginData("Plugin1");
            Assert.AreEqual(35, pluginData.Property2);
            Assert.AreEqual(25,
                pluginData.Settings.GetSettingValueOrThrow<int>("Int32Setting1"));
        }
    }

Element **typeDefinition** in plugin section
=============================================

Element **iocConfiguration/pluginsSetup/pluginSetup/typeDefinitions/typeDefinition** can be used in plugin section to reference types by type alias, the same way this element is used in non-plugin section.

Refer to :doc:`./using-types-in-configuration` for more details on **typeDefinition** element.

Example of **typeDefintion** elements in **pluginSetup** element:
-----------------------------------------------------------------

.. code-block:: xml
    :linenos:

    <pluginsSetup>
        <pluginSetup plugin="Plugin1">
            <pluginImplementation type="TestPluginAssembly1.Implementations.Plugin1_Simple" />
            <typeDefinitions>
                <!--Generic1_1_of_Interface1_Impl1 type definition overrides the
                    definition in non-plugins section.-->
                <typeDefinition alias="ReadOnlyListOfGenericType"
                                type="System.Collections.Generic.IReadOnlyList" assembly="corlib">
                    <genericTypeParameters>
                        <typeDefinition type="SharedServices.Implementations.Generic.Generic3_1" >
                            <genericTypeParameters>
                                <typeDefinition
                                    type="SharedServices.Implementations.Interface1_Impl1" />
                            </genericTypeParameters>
                        </typeDefinition>
                    </genericTypeParameters>
                </typeDefinition>
                <typeDefinition alias="IDoor" type="TestPluginAssembly1.Interfaces.IDoor" />
                <typeDefinition alias="Door" type="TestPluginAssembly1.Implementations.Door" />
                <typeDefinition alias="plugin1Module" type="ModulesForPlugin1.IoC.DiModule2" />
            </typeDefinitions>
        </pluginSetup>
    <pluginsSetup>

Plugin Settings
===============

An element **iocConfiguration/pluginsSetup/pluginSetup/settings** can be used to specify plugin specific settings. The format of plugin settings is similar to settings in general area (i.e., in element **iocConfiguration/settings**). For more details on settings in general refer to :doc:`./settings`.

.. code-block:: xml

    <!--...-->
    <pluginSetup plugin="Plugin1">
        <!--...-->
        <settings>
            <int32 name="Int32Setting1" value="25" />
            <int64 name="Int64Setting1" value="38" />
            <string name="StringSetting1" value="String Value 1" />
        </settings>
        <!--...-->
    </pluginSetup>

Here is an example of how to access plugin setting values at runtime:

.. code-block:: csharp

    public class AccessPluginDataExample
    {
        public AccessPluginDataExample(IoC.Configuration.IPluginDataRepository pluginDataRepository)
        {
            var pluginData = pluginRepository.GetPluginData("Plugin1");

            Assert.AreEqual(25,
                            pluginData.Settings.GetSettingValueOrThrow<int>("Int32Setting1"));
            Assert.AreEqual("String Value 1",
                            pluginData.Settings.GetSettingValueOrThrow<string>("StringSetting1"));
        }
    }

- If a setting is not found in plugin settings element **iocConfiguration/pluginsSetup/pluginSetup/settings**, **IoC.Configuration** will search for a setting in general settings area (i.e., in settings defined in element **iocConfiguration/settings**).
- To specify required settings, implement the property **IEnumerable<SettingInfo> RequiredSettings { get; }** in interface **IoC.Configuration.IPlugin**, or override the virtual property with the same name in **IoC.Configuration.PluginAbstr**, if the plugin implementation is a subclass of **IoC.Configuration.PluginAbstr** class.

Plugin Modules
==============
- Plugin modules can be specified in **module** elements under element **iocConfiguration/pluginsSetup/pluginSetup/dependencyInjection/modules**.
- The format of **iocConfiguration/pluginsSetup/pluginSetup/dependencyInjection/modules** is similar to the format of element **iocConfiguration/dependencyInjection/modules**, except that plugin modules are used to specify type bindings for plugin related types. See :doc:`./modules` for more details on **iocConfiguration/dependencyInjection/modules** element.

Plugin Type Bindings
====================

Plugin related type binding can be specified either under element **iocConfiguration/dependencyInjection/services** to provide plugin related implementations for non plugin interfaces, or under element **iocConfiguration/pluginsSetup/pluginSetup/dependencyInjection/services** for cases when both the service and the implementation are plugin specific.

.. note::
    If the plugin is disabled by setting the value of attribute **enabled** in element **iocConfiguration/plugins/plugin** for the specific plugin, the type bindings for the given plugin will be ignored.

Example 1: Plugin Specific Implementation for non Plugin Type
-------------------------------------------------------------

Here is an example of binding a non-plugin service **SharedServices.Interfaces.IInterface5** to plugin specific type **TestPluginAssembly1.Implementations.Interface5_Plugin1Impl**.


.. code-block:: xml

    <iocConfiguration>
        <!--...-->
        </dependencyInjection>
            <!--...-->
            <services>
                <service type="SharedServices.Interfaces.IInterface5" assembly="shared_services">
                    <implementation type="SharedServices.Implementations.Interface5_Impl1"
                                    assembly="shared_services"
                                    scope="singleton" />
                    <implementation type="TestPluginAssembly1.Implementations.Interface5_Plugin1Impl"
                                    assembly="pluginassm1" scope="singleton" />
                    <implementation type="TestPluginAssembly2.Implementations.Interface5_Plugin2Impl"
                                    assembly="pluginassm2" scope="transient" />
                    <implementation type="TestPluginAssembly3.Implementations.Interface5_Plugin3Impl"
                                    assembly="pluginassm3" scope="transient" />
                </service>
                <!--...-->
            </services>
            <!--...-->
        </dependencyInjection>
    </iocConfiguration>

Example 2: Plugin Specific Implementation for Plugin Type
---------------------------------------------------------

Here is an example of binding a plugin service **TestPluginAssembly1.Interfaces.IDoor** to plugin specific type **TestPluginAssembly1.Implementations.Door**.

.. code-block:: xml

    <iocConfiguration>
        <!--...-->
        <pluginsSetup>
            <pluginSetup plugin="Plugin1">
                <!--...-->
                <dependencyInjection>
                    <services>
                        <service type="TestPluginAssembly1.Interfaces.IDoor"
                                 assembly="pluginassm1">
                            <implementation type="TestPluginAssembly1.Implementations.Door"
                                            assembly="pluginassm1"
                                            scope="transient">
                                <parameters>
                                    <int32 name="Color" value="3" />
                                    <double name="Height" value="180" />
                                </parameters>
                            </implementation>
                        </service>
                        <!--...-->
                    </services>
                    <!--...-->
                <dependencyInjection>
            </pluginSetup>
            <!--...-->
        <pluginsSetup>
        <!--...-->
    <iocConfiguration>

Plugin types in **collection** in non-plugin section
====================================================

Plugin types can be used in value initializer elements for specifying items in **collection** element plugin as well as in non-plugin section.

.. note::
   Refer to :doc:`./value-initialization-elements/collection` for more details on **collection** element.

.. note::
   Refer to :doc:`./value-initialization-elements/index` for more details on value intialization elements.

If value of plugin type is used in a collection item in non-plugin section, and the plugin is disabled by setting the value of attribute **enabled** in element **iocConfiguration/plugins/plugin** for the specific plugin, the item will not be included in a collection generated by **IoC.Configuration**.

Autogenerated Services
======================

An interface with auto-generated implementations can be specified in element **iocConfiguration/pluginsSetup/pluginSetup/dependencyInjection/autoGeneratedServices**. For more information on autogenerated services see :doc:`./autogenerated-services/index`.

Here is an example of **autoGeneratedServices** for a plugin. In this example, **IoC.Configuration** will generate an implementation of **TestPluginAssembly1.Interfaces.IResourceAccessValidatorFactory** and will configure a type binding, that ,apts the interface **IResourceAccessValidatorFactory** to auto-generated type.

.. code-block:: xml
    :linenos:

    <autoGeneratedServices>
        <autoService interface="TestPluginAssembly1.Interfaces.IResourceAccessValidatorFactory">
            <autoMethod name="GetValidators"
                        returnType="System.Collections.Generic.IEnumerable[TestPluginAssembly1.Interfaces.IResourceAccessValidator]"
                        reuseValue="true" >
                <methodSignature>
                    <string paramName="resourceName"/>
                </methodSignature>

                <if parameter1="public_pages">
                    <collection>
                        <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                    </collection>
                </if>

                <if parameter1="admin_pages">
                    <collection>
                        <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                        <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"/>
                    </collection>
                </if>

                <default>
                    <collection>
                        <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator2"/>
                        <injectedObject type="TestPluginAssembly1.Interfaces.ResourceAccessValidator1"/>
                    </collection>
                </default>
            </autoMethod>
        </autoService>
    </autoGeneratedServices>

The definition of interface **IResourceAccessValidatorFactory** is shown below

.. code-block:: csharp

    public interface IResourceAccessValidatorFactory
    {
        IEnumerable<IResourceAccessValidator> GetValidators(string resourceName);
    }