using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using StructureMap.Configuration.DSL;
using StructureMap.Diagnostics;
using StructureMap.Interceptors;

namespace StructureMap.Graph
{
    /// <summary>
    /// A PluginGraph models the entire listing of all PluginFamily�s and Plugin�s controlled by 
    /// StructureMap within the current AppDomain. The scope of the PluginGraph is controlled by 
    /// a combination of custom attributes and the StructureMap.config class
    /// </summary>
    [Serializable]
    public class PluginGraph
    {
        private readonly AssemblyGraphCollection _assemblies;
        private readonly InstanceDefaultManager _defaultManager = new InstanceDefaultManager();
        private readonly InterceptorLibrary _interceptorLibrary = new InterceptorLibrary();
        private readonly PluginFamilyCollection _pluginFamilies;
        private bool _sealed = false;
        private readonly bool _useExternalRegistries = true;
        private readonly GraphLog _log = new GraphLog();

        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginGraph() : base()
        {
            _assemblies = new AssemblyGraphCollection(this);
            _pluginFamilies = new PluginFamilyCollection(this);
        }


        public PluginGraph(bool useExternalRegistries) : this()
        {
            _useExternalRegistries = useExternalRegistries;
        }

        public AssemblyGraphCollection Assemblies
        {
            get { return _assemblies; }
        }

        public PluginFamilyCollection PluginFamilies
        {
            get { return _pluginFamilies; }
        }


        public GraphLog Log
        {
            get { return _log; }
        }

        #region seal

        public bool IsSealed
        {
            get { return _sealed; }
        }

        public InstanceDefaultManager DefaultManager
        {
            get { return _defaultManager; }
        }

        public InterceptorLibrary InterceptorLibrary
        {
            get { return _interceptorLibrary; }
        }

        /// <summary>
        /// Closes the PluginGraph for adding or removing members.  Searches all of the
        /// AssemblyGraph's for implicit Plugin and PluginFamily's
        /// </summary>
        public void Seal()
        {
            if (_sealed)
            {
                return;
            }

            if (_useExternalRegistries)
            {
                searchAssembliesForRegistries();
            }

            foreach (AssemblyGraph assembly in _assemblies)
            {
                addImplicitPluginFamilies(assembly);
            }

            foreach (PluginFamily family in _pluginFamilies)
            {
                attachImplicitPlugins(family);
                family.DiscoverImplicitInstances();
            }

            _sealed = true;
        }


        private void searchAssembliesForRegistries()
        {
            List<Registry> list = new List<Registry>();
            foreach (AssemblyGraph assembly in _assemblies)
            {
                list.AddRange(assembly.FindRegistries());
            }

            foreach (Registry registry in list)
            {
                registry.ConfigurePluginGraph(this);
            }
        }

        private void attachImplicitPlugins(PluginFamily family)
        {
            foreach (AssemblyGraph assembly in _assemblies)
            {
                family.FindPlugins(assembly);
            }
        }


        private void addImplicitPluginFamilies(AssemblyGraph assemblyGraph)
        {
            PluginFamily[] families = assemblyGraph.FindPluginFamilies();

            foreach (PluginFamily family in families)
            {
                if (_pluginFamilies.Contains(family.PluginType))
                {
                    continue;
                }

                _pluginFamilies.Add(family);
            }
        }

        #endregion

        public static PluginGraph BuildGraphFromAssembly(Assembly assembly)
        {
            PluginGraph pluginGraph = new PluginGraph();
            pluginGraph.Assemblies.Add(assembly);

            pluginGraph.Seal();

            return pluginGraph;
        }

        public TypePath LocateOrCreateFamilyForType(string fullName)
        {
            Type pluginType = findTypeByFullName(fullName);
            buildFamilyIfMissing(pluginType);

            return new TypePath(pluginType);
        }

        private void buildFamilyIfMissing(Type pluginType)
        {
            if (!_pluginFamilies.Contains(pluginType))
            {
                PluginFamily family = _pluginFamilies.Add(pluginType);
                attachImplicitPlugins(family);
            }
        }

        public PluginFamily LocateOrCreateFamilyForType(Type pluginType)
        {
            buildFamilyIfMissing(pluginType);
            return PluginFamilies[pluginType];
        }

        private Type findTypeByFullName(string fullName)
        {
            foreach (AssemblyGraph assembly in _assemblies)
            {
                Type type = assembly.FindTypeByFullName(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            throw new StructureMapException(300, fullName);
        }

        [Obsolete]
        public void ReadDefaults()
        {
            _defaultManager.ReadDefaultsFromPluginGraph(this);
        }
    }
}