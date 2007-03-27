using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using StructureMap.Configuration;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace StructureMap
{
    public static class StructureMapConfiguration
    {
        private const string CONFIG_FILE_NAME = "StructureMap.config";
        private static ConfigurationParserCollection _collection = new ConfigurationParserCollection();
        private static Registry _registry = new Registry();
        private static List<Registry> _registries = new List<Registry>();

        static StructureMapConfiguration()
        {
            ResetAll();
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


        public static void ResetAll()
        {
            _collection = new ConfigurationParserCollection();
            _registry = new Registry();
            _registries = new List<Registry>();
            _registries.Add(_registry);
        }

        public static PluginGraph GetPluginGraph()
        {
            PluginGraphBuilder builder = createBuilder();
            return builder.Build();
        }

        private static PluginGraphBuilder createBuilder()
        {
            ConfigurationParser[] parsers = _collection.GetParsers();
            return new PluginGraphBuilder(parsers, _registries.ToArray());
        }

        public static PluginGraph GetDiagnosticPluginGraph()
        {
            PluginGraphBuilder builder = createBuilder();
            return builder.BuildDiagnosticPluginGraph();
        }

        public static void IncludeConfigurationFromFile(string filename)
        {
            _collection.IncludeFile(filename);
        }

        public static void IncludeConfigurationFrom(FetchNodeDelegate fetcher)
        {
            _collection.IncludeNode(fetcher);
        }

        public static void IncludeConfigurationFromNode(XmlNode node)
        {
            _collection.IncludeNode(
                delegate { return node; }
                );
        }

        public static bool UseDefaultStructureMapConfigFile
        {
            get { return _collection.UseDefaultFile; }
            set { _collection.UseDefaultFile = value; }
        }

        public static ScanAssembliesExpression ScanAssemblies()
        {
            ScanAssembliesExpression expression = new ScanAssembliesExpression();
            _registry.addExpression(expression);

            return expression;
        }

        public static CreatePluginFamilyExpression BuildInstancesOf<T>()
        {
            return _registry.BuildInstancesOf<T>();
        }

        public static InstanceExpression.InstanceTypeExpression AddInstanceOf<T>()
        {
            return _registry.AddInstanceOf<T>();
        }

        public static InstanceExpression.InstanceTypeExpression Instance<T>()
        {
            return Registry.Instance<T>();
        }

        public static PrototypeExpression<T> Prototype<T>(T prototype)
        {
            return new PrototypeExpression<T>(prototype);
        }

        public static LiteralExpression<T> Object<T>(T instance)
        {
            return new LiteralExpression<T>(instance);
        }

        public static LiteralExpression<T> AddInstanceOf<T>(T target)
        {
            return _registry.AddInstanceOf(target);
        }

        public static PrototypeExpression<T> AddPrototypeInstanceOf<T>(T prototype)
        {
            return _registry.AddPrototypeInstanceOf(prototype);
        }

        public static ProfileExpression CreateProfile(string profileName)
        {
            return _registry.CreateProfile(profileName);
        }

        public static void AddRegistry(Registry registry)
        {
            _registries.Add(registry);
        }
    }
}