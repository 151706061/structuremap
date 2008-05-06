using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using StructureMap.Attributes;
using StructureMap.Interceptors;
using StructureMap.Pipeline;

namespace StructureMap.Graph
{
    /// <summary>
    /// Conceptually speaking, a PluginFamily object represents a point of abstraction or variability in 
    /// the system.  A PluginFamily defines a CLR Type that StructureMap can build, and all of the possible
    /// Plugin�s implementing the CLR Type.
    /// </summary>
    public class PluginFamily : IPluginFamily
    {
        public const string CONCRETE_KEY = "CONCRETE";
        private readonly List<InstanceMemento> _mementoList = new List<InstanceMemento>();
        private readonly PluginCollection _plugins;
        private string _defaultKey = string.Empty;
        private InstanceInterceptor _instanceInterceptor = new NulloInterceptor();
        private PluginGraph _parent;
        private readonly Type _pluginType;
        private readonly string _pluginTypeName;
        private readonly List<Instance> _instances = new List<Instance>();
        private IBuildPolicy _buildPolicy = new BuildPolicy();

        private readonly Predicate<Type> _explicitlyMarkedPluginFilter;
        private readonly Predicate<Type> _implicitPluginFilter;
        private Predicate<Type> _pluginFilter;


        // TODO:  Need to unit test the scope from the attribute
        /// <summary>
        /// Testing constructor
        /// </summary>
        /// <param name="pluginType"></param>
        public PluginFamily(Type pluginType)
        {
            _pluginType = pluginType;
            _pluginTypeName = TypePath.GetAssemblyQualifiedName(_pluginType);
            _plugins = new PluginCollection(this);

            PluginFamilyAttribute.ConfigureFamily(this);

            _explicitlyMarkedPluginFilter = delegate(Type type) { return Plugin.IsAnExplicitPlugin(PluginType, type); };
            _implicitPluginFilter = delegate(Type type) { return Plugin.CanBeCast(PluginType, type); };
            _pluginFilter = _explicitlyMarkedPluginFilter;
        }


        public PluginGraph Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public InstanceInterceptor InstanceInterceptor
        {
            get { return _instanceInterceptor; }
            set { _instanceInterceptor = value; }
        }

        // TODO:  This code sucks.  What's going on here?
        public PluginFamily CreateTemplatedClone(params Type[] templateTypes)
        {
            Type templatedType = _pluginType.MakeGenericType(templateTypes);
            PluginFamily templatedFamily = new PluginFamily(templatedType);
            templatedFamily._defaultKey = _defaultKey;
            templatedFamily.Parent = Parent;
            templatedFamily._buildPolicy = _buildPolicy.Clone();

            // Add Plugins
            foreach (Plugin plugin in _plugins)
            {
                if (plugin.CanBePluggedIntoGenericType(_pluginType, templateTypes))
                {
                    Plugin templatedPlugin = plugin.CreateTemplatedClone(templateTypes);
                    templatedFamily.Plugins.Add(templatedPlugin);
                }
            }

            // TODO -- Got a big problem here.  Intances need to be copied over
            foreach (IDiagnosticInstance instance in GetAllInstances())
            {
                if (instance.CanBePartOfPluginFamily(templatedFamily))
                {
                    templatedFamily.AddInstance((Instance)instance);
                }
            }

            // Need to attach the new PluginFamily to the old PluginGraph
            Parent.PluginFamilies.Add(templatedFamily);

            return templatedFamily;
        }



        public void AddInstance(InstanceMemento memento)
        {
            _mementoList.Add(memento);
        }

        public void AddInstance(Instance instance)
        {
            _instances.Add(instance);
        }

        public void AddMementoSource(MementoSource source)
        {
            _mementoList.AddRange(source.GetAllMementos());
        }

        // For testing
        public InstanceMemento GetMemento(string instanceKey)
        {
            return _mementoList.Find(delegate(InstanceMemento m) { return m.InstanceKey == instanceKey; });
        }



        #region properties

        /// <summary>
        /// The CLR Type that defines the "Plugin" interface for the PluginFamily
        /// </summary>
        public Type PluginType
        {
            get { return _pluginType; }
        }

        /// <summary>
        /// The InstanceKey of the default instance of the PluginFamily
        /// </summary>
        public string DefaultInstanceKey
        {
            get { return _defaultKey; }
            set { _defaultKey = value ?? string.Empty; }
        }

        public PluginCollection Plugins
        {
            get { return _plugins; }
        }

        public string PluginTypeName
        {
            get { return _pluginTypeName; }
        }

        public bool IsGenericTemplate
        {
            get { return _pluginType.IsGenericTypeDefinition; }
        }

        public bool IsGenericType
        {
            get { return _pluginType.IsGenericType; }
        }

        public bool SearchForImplicitPlugins
        {
            get
            {
                return ReferenceEquals(_pluginFilter, _implicitPluginFilter);
            }
            set
            {
                _pluginFilter = value ? _implicitPluginFilter : _explicitlyMarkedPluginFilter;
            }
        }

        public IBuildPolicy Policy
        {
            get { return _buildPolicy; }
        }

        public int PluginCount
        {
            get { return _plugins.Count; }
        }

        #endregion

        public void Seal()
        {
            discoverImplicitInstances();

            foreach (InstanceMemento memento in _mementoList)
            {
                Instance instance = memento.ReadInstance(Parent, _pluginType);
                _instances.Add(instance);
            }
        }

        private void discoverImplicitInstances()
        {
            List<Plugin> list = _plugins.FindAutoFillablePlugins();
            foreach (InstanceMemento memento in _mementoList)
            {
                Plugin plugin = memento.FindPlugin(this);
                list.Remove(plugin);
            }

            foreach (Plugin plugin in list)
            {
                AddInstance(plugin.CreateImplicitMemento());
            }
        }

        public Instance[] GetAllInstances()
        {
            return _instances.ToArray();
        }

        public Instance GetInstance(string name)
        {
            return _instances.Find(delegate(Instance i) { return i.Name == name; });
        }

        public void SetScopeTo(InstanceScope scope)
        {
            switch(scope)
            {
                case InstanceScope.Singleton:
                    AddInterceptor(new SingletonPolicy());
                    break;

                case InstanceScope.HttpContext:
                    AddInterceptor(new HttpContextBuildPolicy());
                    break;

                case InstanceScope.ThreadLocal:
                    AddInterceptor(new ThreadLocalStoragePolicy());
                    break;

                case InstanceScope.Hybrid:
                    AddInterceptor(new HybridBuildPolicy());
                    break;
            }
        }

        public void AddInterceptor(IInstanceInterceptor interceptor)
        {
            interceptor.InnerPolicy = _buildPolicy;
            _buildPolicy = interceptor;
        }


        public void AnalyzeTypeForPlugin(Type pluggedType)
        {
            if (_pluginFilter(pluggedType))
            {
                if (!HasPlugin(pluggedType))
                {
                    Plugin plugin = Plugin.CreateImplicitPlugin(pluggedType);
                    _plugins.Add(plugin);
                }
            }
        }

        public bool HasPlugin(Type pluggedType)
        {
            return _plugins.HasPlugin(pluggedType);
        }

        public void AddPlugin(Type pluggedType)
        {
            if (!HasPlugin(pluggedType))
            {
                _plugins.Add(Plugin.CreateImplicitPlugin(pluggedType));
            }
        }

        public void AddPlugin(Type pluggedType, string key)
        {
            Plugin plugin = Plugin.CreateExplicitPlugin(pluggedType, key, string.Empty);
            _plugins.Add(plugin);
            
        }
    }
}