using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using StructureMap.Configuration;
using StructureMap.Configuration.DSL;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Diagnostics;
using StructureMap.Graph;
using StructureMap.Interceptors;
using StructureMap.Pipeline;

namespace StructureMap
{
    public static class StructureMapConfiguration
    {
        private const string CONFIG_FILE_NAME = "StructureMap.config";
        private static GraphLog _log;
        private static List<Registry> _registries;
        private static Registry _registry;
        private static ConfigurationParserBuilder _parserBuilder;

        static StructureMapConfiguration()
        {
            ResetAll();
        }

        /// <summary>
        /// Flag to enable or disable the usage of the default StructureMap.config
        /// If set to false, StructureMap will not look for a StructureMap.config file
        /// </summary>
        public static bool UseDefaultStructureMapConfigFile
        {
            get { return _parserBuilder.UseAndEnforceExistenceOfDefaultFile; }
            set { _parserBuilder.UseAndEnforceExistenceOfDefaultFile = value; }
        }


        public static bool IgnoreStructureMapConfig
        {
            get { return _parserBuilder.IgnoreDefaultFile; }
            set { _parserBuilder.IgnoreDefaultFile = value; }
        }

        public static bool PullConfigurationFromAppConfig
        {
            get { return _parserBuilder.PullConfigurationFromAppConfig; }
            set { _parserBuilder.PullConfigurationFromAppConfig = value; }
        }


        /// <summary>
        /// Returns the path to the StructureMap.config file
        /// </summary>
        /// <returns></returns>
        public static string GetStructureMapConfigurationPath()
        {
            string basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string configPath = Path.Combine(basePath, CONFIG_FILE_NAME);

            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(basePath, "bin");
                configPath = Path.Combine(configPath, CONFIG_FILE_NAME);

                if (!File.Exists(configPath))
                {
                    configPath = Path.Combine(basePath, @"..\" + CONFIG_FILE_NAME);
                }
            }

            return configPath;
        }

        /// <summary>
        /// Clears StructureMapConfiguration of all configuration options.  Returns StructureMap
        /// to only using the default StructureMap.config file for configuration.
        /// </summary>
        public static void ResetAll()
        {
            _log = new GraphLog();
            _parserBuilder = new ConfigurationParserBuilder(_log);
            _registry = new Registry();
            _registries = new List<Registry>();
            _registries.Add(_registry);
            UseDefaultStructureMapConfigFile = false;
            IgnoreStructureMapConfig = false;
        }

        public static void RegisterInterceptor(TypeInterceptor interceptor)
        {
            _registry.RegisterInterceptor(interceptor);
        }

        /// <summary>
        /// Builds a PluginGraph object for the current configuration.  Used by ObjectFactory.
        /// </summary>
        /// <returns></returns>
        public static PluginGraph GetPluginGraph()
        {
            ConfigurationParser[] parsers = _parserBuilder.GetParsers();
            PluginGraphBuilder pluginGraphBuilder = new PluginGraphBuilder(parsers, _registries.ToArray(), _log);
            return pluginGraphBuilder.Build();
        }

        /// <summary>
        /// Directs StructureMap to include Xml configuration information from a separate file
        /// </summary>
        /// <param name="filename"></param>
        public static void IncludeConfigurationFromFile(string filename)
        {
            _parserBuilder.IncludeFile(filename);
        }

        /// <summary>
        /// Programmatically adds a &lt;StructureMap&gt; node containing Xml configuration
        /// </summary>
        /// <param name="node"></param>
        /// <param name="description">A description of this node source for troubleshooting purposes</param>
        public static void IncludeConfigurationFromNode(XmlNode node, string description)
        {
            _parserBuilder.IncludeNode(node, string.Empty);
        }

        /// <summary>
        /// Programmatically determine Assembly's to be scanned for attribute configuration
        /// </summary>
        /// <returns></returns>
        public static ScanAssembliesExpression ScanAssemblies()
        {
            return new ScanAssembliesExpression(_registry);
        }

        /// <summary>
        /// Direct StructureMap to create instances of Type T
        /// </summary>
        /// <typeparam name="PLUGINTYPE">The Type to build</typeparam>
        /// <returns></returns>
        public static CreatePluginFamilyExpression<PLUGINTYPE> BuildInstancesOf<PLUGINTYPE>()
        {
            return _registry.BuildInstancesOf<PLUGINTYPE>();
        }

        /// <summary>
        /// Direct StructureMap to create instances of Type T
        /// </summary>
        /// <typeparam name="PLUGINTYPE">The Type to build</typeparam>
        /// <returns></returns>
        public static CreatePluginFamilyExpression<PLUGINTYPE> ForRequestedType<PLUGINTYPE>()
        {
            return _registry.BuildInstancesOf<PLUGINTYPE>();
        }

        public static GenericFamilyExpression ForRequestedType(Type pluginType)
        {
            return _registry.ForRequestedType(pluginType);
        }

        /// <summary>
        /// Adds a new configured instance of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ConfiguredInstance AddInstanceOf<T>()
        {
            return _registry.AddInstanceOf<T>();
        }

        public static void AddInstanceOf<T>(Instance instance)
        {
            _registry.ForRequestedType<T>().AddInstance(instance);
        }


        /// <summary>
        /// Adds a preconfigured instance of Type T to StructureMap.  When this instance is requested,
        /// StructureMap will always return the original object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static LiteralInstance AddInstanceOf<T>(T target)
        {
            return _registry.AddInstanceOf(target);
        }

        /// <summary>
        /// Adds a Prototype (GoF) instance of Type T.  The actual prototype object must implement the
        /// ICloneable interface.  When this instance of T is requested, StructureMap will
        /// return a cloned copy of the originally registered prototype object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public static PrototypeInstance AddPrototypeInstanceOf<T>(T prototype)
        {
            return _registry.AddPrototypeInstanceOf(prototype);
        }

        /// <summary>
        /// Starts the definition of a configuration Profile. 
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public static ProfileExpression CreateProfile(string profileName)
        {
            return _registry.CreateProfile(profileName);
        }

        /// <summary>
        /// Directs StructureMap to use a Registry class to construct the
        /// PluginGraph
        /// </summary>
        /// <param name="registry"></param>
        public static void AddRegistry(Registry registry)
        {
            _registries.Add(registry);
        }

        public static void TheDefaultProfileIs(string profileName)
        {
            _registry.addExpression(delegate(PluginGraph graph)
            {
                graph.ProfileManager.DefaultProfileName = profileName;
            });
        }


    }
}