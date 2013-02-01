using System;
using System.Reflection;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace StructureMap.Configuration
{
    public class GraphBuilder : IGraphBuilder
    {
        private readonly PluginGraph _pluginGraph;
        private readonly Container _systemContainer;
        private readonly PluginGraph _systemGraph;


        public GraphBuilder(Registry[] registries)
            : this(registries, new PluginGraph())
        {
        }

        public GraphBuilder(Registry[] registries, PluginGraph pluginGraph)
        {
            _pluginGraph = pluginGraph;

            foreach (Registry registry in registries)
            {
                registry.As<IPluginGraphConfiguration>().Configure(_pluginGraph);
            }

            _systemGraph = new SystemRegistry().Build();
            _systemContainer = new Container(_systemGraph);
        }

        #region IGraphBuilder Members

        public void FinishFamilies()
        {
            _pluginGraph.Seal();
        }

        public PluginGraph PluginGraph { get { return _pluginGraph; } }

        public void AddRegistry(string registryTypeName)
        {
            _pluginGraph.Log.Try(() =>
            {
                Type type = new TypePath(registryTypeName).FindType();
                var registry = (Registry) Activator.CreateInstance(type);
                registry.As<IPluginGraphConfiguration>().Configure(_pluginGraph);
            }).AndReportErrorAs(290, registryTypeName);
        }

        public void ConfigureFamily(TypePath pluginTypePath, Action<PluginFamily> action)
        {
            try
            {
                Type pluginType = pluginTypePath.FindType();
                PluginFamily family = _pluginGraph.FindFamily(pluginType);
                action(family);
            }
            catch (Exception ex)
            {
                _pluginGraph.Log.RegisterError(103, ex, pluginTypePath.AssemblyQualifiedName);
            }
        }

        #endregion

    }
}