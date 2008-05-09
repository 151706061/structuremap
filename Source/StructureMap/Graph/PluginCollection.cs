using System;
using System.Collections;
using System.Collections.Generic;

namespace StructureMap.Graph
{
    /// <summary>
    /// Custom collection for Plugin objects
    /// </summary>
    public class PluginCollection : IEnumerable<Plugin>
    {
        private readonly PluginFamily _family;
        private readonly Dictionary<string, Plugin> _plugins = new Dictionary<string, Plugin>();

        public PluginCollection(PluginFamily family)
        {
            _family = family;
        }

        public Plugin[] All
        {
            get
            {
                Plugin[] returnValue = new Plugin[_plugins.Count];
                _plugins.Values.CopyTo(returnValue, 0);

                return returnValue;
            }
        }

        public int Count
        {
            get { return _plugins.Count; }
        }

        /// <summary>
        /// Gets a Plugin by its PluggedType
        /// </summary>
        /// <param name="PluggedType"></param>
        /// <returns></returns>
        public Plugin this[Type PluggedType]
        {
            get
            {
                Plugin returnValue = null;

                foreach (Plugin plugin in _plugins.Values)
                {
                    if (plugin.PluggedType.Equals(PluggedType))
                    {
                        returnValue = plugin;
                        break;
                    }
                }

                return returnValue;
            }
        }

        /// <summary>
        /// Retrieves a Plugin by its ConcreteKey
        /// </summary>
        /// <param name="concreteKey"></param>
        /// <returns></returns>
        public Plugin this[string concreteKey]
        {
            get
            {
                if (_plugins.ContainsKey(concreteKey))
                {
                    return _plugins[concreteKey] as Plugin;
                }

                return null;
            }
        }

        /// <summary>
        /// Adds a new Plugin by the PluggedType
        /// </summary>
        /// <param name="pluggedType"></param>
        /// <param name="concreteKey"></param>
        // TODO -- not wild about this method.
        [Obsolete("Get rid of this")]
        public void Add(Type pluggedType, string concreteKey)
        {
            Plugin plugin = new Plugin(pluggedType, concreteKey);
            Add(plugin);
        }

        public void Add(Plugin plugin)
        {
            // Reject if a duplicate ConcreteKey
            if (_plugins.ContainsKey(plugin.ConcreteKey))
            {
                // Don't duplicate, but merge setters
                Plugin peer = this[plugin.ConcreteKey];
                if (peer.PluggedType == plugin.PluggedType)
                {
                    peer.MergeSetters(plugin);
                    return;
                }
                else
                {
                    throw new StructureMapException(113, plugin.ConcreteKey, _family.PluginType.AssemblyQualifiedName);
                }
            }

            // Reject if the PluggedType cannot be upcast to the PluginType
            if (!Plugin.CanBeCast(_family.PluginType, plugin.PluggedType))
            {
                throw new StructureMapException(114, plugin.PluggedType.FullName, _family.PluginType.AssemblyQualifiedName);
            }

            _plugins.Add(plugin.ConcreteKey, plugin);
        }

        /// <summary>
        /// Does the PluginFamily contain a Plugin
        /// </summary>
        /// <param name="concreteKey"></param>
        /// <returns></returns>
        public bool HasPlugin(string concreteKey)
        {
            return _plugins.ContainsKey(concreteKey);
        }


        public void Remove(string concreteKey)
        {
            _plugins.Remove(concreteKey);
        }

        public Plugin FindOrCreate(Type pluggedType, bool createDefaultInstanceOfType)
        {
            Plugin plugin = new Plugin(pluggedType);
            Add(plugin);

            return plugin;
        }

        IEnumerator<Plugin> IEnumerable<Plugin>.GetEnumerator()
        {
            return _plugins.Values.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<Plugin>) this).GetEnumerator();
        }

        public List<Plugin> FindAutoFillablePlugins()
        {
            List<Plugin> list = new List<Plugin>();
            foreach (Plugin plugin in _plugins.Values)
            {
                if (plugin.CanBeAutoFilled)
                {
                    list.Add(plugin);
                }
            }

            return list;
        }

        public bool HasPlugin(Type pluggedType)
        {
            foreach (KeyValuePair<string, Plugin> pair in _plugins)
            {
                if (pair.Value.PluggedType == pluggedType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}