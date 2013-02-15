using System;
using System.Collections.Generic;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Query;

namespace StructureMap
{
    public interface IPipelineGraph
    {
        Instance GetDefault(Type pluginType);
        bool HasDefaultForPluginType(Type pluginType);
        bool HasInstance(Type pluginType, string instanceKey);
        void EachInstance(Action<Type, Instance> action);
        IEnumerable<Instance> GetAllInstances();
        IEnumerable<Instance> GetAllInstances(Type pluginType);
        Instance FindInstance(Type pluginType, string name);
        bool IsUnique(Type pluginType);

        // TODO -- going to add the Instance here.
        IObjectCache FindCache(Type pluginType);
        void SetDefault(Type pluginType, Instance instance);

        IPipelineGraph ForProfile(string profile);

        MissingFactoryFunction OnMissingFactory { set; }

        [Obsolete("This needs to go away.  We'll just have Container.Configure write directly to the PluginGraph")]
        void ImportFrom(PluginGraph graph);

        IEnumerable<IPluginTypeConfiguration> GetPluginTypes(IContainer container);
        void EjectAllInstancesOf<T>();
        void Dispose();
        void Remove(Func<Type, bool> filter);
        void Remove(Type pluginType);
        IPipelineGraph ToNestedGraph();
    }
}