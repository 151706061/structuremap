using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StructureMap.Emitting;
using StructureMap.Util;

namespace StructureMap.Graph
{
    public static class PluginCache
    {
        private static readonly Cache<Type, InstanceBuilder> _builders;
        private static readonly Cache<Type, Plugin> _plugins;
        private static readonly List<Predicate<PropertyInfo>> _setterRules;

        static PluginCache()
        {
            _setterRules = new List<Predicate<PropertyInfo>>();
            _plugins = new Cache<Type, Plugin>(t =>
            {
                var plugin = new Plugin(t);
                foreach (var rule in _setterRules)
                {
                    plugin.UseSetterRule(rule);
                }

                return plugin;
            });


            _builders = new Cache<Type, InstanceBuilder>(t =>
            {
                try
                {
                    Plugin plugin = _plugins[t];
                    return new InstanceBuilderAssembly(new[] {plugin}).Compile()[0];
                }
                catch (Exception e)
                {
                    throw new StructureMapException(245, t.AssemblyQualifiedName, e);
                }
            });
        }

        public static Plugin GetPlugin(Type pluggedType)
        {
            return _plugins[pluggedType];
        }

        public static InstanceBuilder FindBuilder(Type pluggedType)
        {
            return _builders[pluggedType];
        }

        public static void Compile()
        {
            lock (typeof (PluginCache))
            {
                IEnumerable<Plugin> plugins =
                    _plugins.Where(plugin => pluginHasNoBuilder(plugin) && plugin.CanBeCreated());
                createAndStoreBuilders(plugins);
            }
        }

        private static void createAndStoreBuilders(IEnumerable<Plugin> plugins)
        {
            var assembly = new InstanceBuilderAssembly(plugins);
            assembly.Compile().ForEach(b => _builders[b.PluggedType] = b);
        }

        private static bool pluginHasNoBuilder(Plugin plugin)
        {
            return !_builders.Has(plugin.PluggedType);
        }

        public static void Store(Type pluggedType, InstanceBuilder builder)
        {
            _builders[pluggedType] = builder;
        }

        internal static void ResetAll()
        {
            lock (typeof (PluginCache))
            {
                _setterRules.Clear();
                _builders.Clear();
                _plugins.Clear();
            }
        }

        public static void AddFilledType(Type type)
        {
            Predicate<PropertyInfo> predicate = prop => prop.PropertyType == type;
            UseSetterRule(predicate);
        }

        public static void UseSetterRule(Predicate<PropertyInfo> predicate)
        {
            _setterRules.Add(predicate);
            _plugins.Each(plugin => plugin.UseSetterRule(predicate));
        }
    }
}