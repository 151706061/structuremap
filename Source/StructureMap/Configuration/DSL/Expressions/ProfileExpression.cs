using System;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace StructureMap.Configuration.DSL.Expressions
{
    /// <summary>
    /// Expression class to help define a runtime Profile
    /// </summary>
    public class ProfileExpression
    {
        private readonly string _profileName;
        private readonly Registry _registry;

        public ProfileExpression(string profileName, Registry registry)
        {
            _profileName = profileName;
            _registry = registry;
        }


        /// <summary>
        /// Starts the definition of the default instance for the containing Profile
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public InstanceDefaultExpression<T> For<T>()
        {
            return new InstanceDefaultExpression<T>(this);
        }

        /// <summary>
        /// Use statement to define the Profile defaults for a Generic type
        /// </summary>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        public GenericDefaultExpression For(Type pluginType)
        {
            return new GenericDefaultExpression(this, pluginType);
        }

        #region Nested type: InstanceDefaultExpression

        public class InstanceDefaultExpression<T>
        {
            private readonly ProfileExpression _parent;
            private readonly string _profileName;
            private readonly Registry _registry;

            public InstanceDefaultExpression(ProfileExpression parent)
            {
                _parent = parent;
                _registry = parent._registry;
                _profileName = parent._profileName;
            }

            /// <summary>
            /// Use a named, preconfigured instance as the default instance for this profile 
            /// </summary>
            /// <param name="instanceKey"></param>
            /// <returns></returns>
            public ProfileExpression UseNamedInstance(string instanceKey)
            {
                _registry.addExpression(delegate(PluginGraph graph)
                {
                    graph.SetDefault(_profileName, typeof(T), new ReferencedInstance(instanceKey));
                });

                return _parent;
            }

            /// <summary>
            /// Define the default instance of the PluginType for the containing Profile
            /// </summary>
            /// <param name="mementoBuilder"></param>
            /// <returns></returns>
            public ProfileExpression Use(Instance instance)
            {
                instance.Name = "Default Instance for Profile " + _profileName;

                _registry.addExpression(delegate (PluginGraph graph)
                {
                    graph.SetDefault(_profileName, typeof(T), instance);
                });

                return _parent;
            }

            public ProfileExpression Use(Func<T> func)
            {
                ConstructorInstance instance = new ConstructorInstance(delegate { return func(); });
                return Use(instance);
            }

            public ProfileExpression Use(T t)
            {
                LiteralInstance instance = new LiteralInstance(t);
                return Use(instance);
            }

            public ProfileExpression UseConcreteType<CONCRETETYPE>()
            {
                ConfiguredInstance instance = new ConfiguredInstance(typeof(CONCRETETYPE));
                return Use(instance);
            }

            public ProfileExpression UsePrototypeOf(T template)
            {
                PrototypeInstance instance = new PrototypeInstance((ICloneable) template);
                return Use(instance);
            }
        }

        #endregion

        public class GenericDefaultExpression
        {
            private readonly ProfileExpression _parent;
            private readonly Type _pluginType;
            private readonly Registry _registry;

            internal GenericDefaultExpression(ProfileExpression parent, Type pluginType)
            {
                _parent = parent;
                _registry = parent._registry;
                _pluginType = pluginType;
            }

            public ProfileExpression UseConcreteType(Type concreteType)
            {
                ConfiguredInstance instance = new ConfiguredInstance(concreteType);
                return Use(instance);
            }

            public ProfileExpression Use(Instance instance)
            {
                _registry.addExpression(delegate(PluginGraph graph)
                {
                    graph.SetDefault(_parent._profileName, _pluginType, instance);
                });

                return _parent;
            }

            public ProfileExpression UseNamedInstance(string name)
            {
                ReferencedInstance instance = new ReferencedInstance(name);
                return Use(instance);
            }
        }
    }
}