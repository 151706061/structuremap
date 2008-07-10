using System;
using System.Collections.Generic;
using StructureMap.Graph;

namespace StructureMap.Pipeline
{
    public class Profile
    {
        private Dictionary<Type, Instance> _instances = new Dictionary<Type, Instance>();
        private string _name;

        public Profile(string name)
        {
            _name = name;
        }


        public string Name
        {
            get { return _name; }
        }

        public void SetDefault(Type pluginType, Instance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (_instances.ContainsKey(pluginType))
            {
                _instances[pluginType] = instance;
            }
            else
            {
                _instances.Add(pluginType, instance);
            }
        }

        public Instance GetDefault(Type pluginType)
        {
            if (_instances.ContainsKey(pluginType))
            {
                return _instances[pluginType];
            }

            return null;
        }

        public void FillTypeInto(Type pluginType, Instance instance)
        {
            if (!_instances.ContainsKey(pluginType))
            {
                _instances.Add(pluginType, instance);
            }
        }

        public void FillAllTypesInto(Profile destination)
        {
            foreach (KeyValuePair<Type, Instance> pair in _instances)
            {
                destination.FillTypeInto(pair.Key, pair.Value);
            }
        }

        public void Merge(Profile destination)
        {
            foreach (KeyValuePair<Type, Instance> pair in _instances)
            {
                destination.SetDefault(pair.Key, pair.Value);
            }
        }

        public void FindMasterInstances(PluginGraph graph)
        {
            Dictionary<Type, Instance> master = new Dictionary<Type, Instance>();

            foreach (KeyValuePair<Type, Instance> pair in _instances)
            {
                PluginFamily family = graph.FindFamily(pair.Key);
                Instance masterInstance = ((IDiagnosticInstance) pair.Value)
                    .FindInstanceForProfile(family, _name, graph.Log);
                
                master.Add(pair.Key, masterInstance);
            }

            _instances = master;
        }

        public static string InstanceKeyForProfile(string profileName)
        {
            return "Default Instance for Profile " + profileName;
        }

        public void CopyDefault(Type sourceType, Type destinationType)
        {
            if (_instances.ContainsKey(sourceType))
            {
                Instance instance = _instances[sourceType];
                _instances.Add(destinationType, instance);
            }
        }


    }
}