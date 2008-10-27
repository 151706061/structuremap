using System;
using System.Collections.Generic;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Interceptors;
using StructureMap.Pipeline;

namespace StructureMap.Configuration.DSL
{
    /// <summary>
    /// A Registry class provides methods and grammars for configuring a Container or ObjectFactory.
    /// Using a Registry subclass is the recommended way of configuring a StructureMap Container.
    /// </summary>
    /// <example>
    /// public class MyRegistry : Registry
    /// {
    ///     public MyRegistry()
    ///     {
    ///         ForRequestedType(typeof(IService)).TheDefaultIsConcreteType(typeof(Service));
    ///     }
    /// }
    /// </example>
    public class Registry
    {
        private readonly List<Action<PluginGraph>> _actions = new List<Action<PluginGraph>>();
        private readonly List<Action> _basicActions = new List<Action>();

        public Registry()
        {
            configure();
        }

        /// <summary>
        /// You can overide this method as a place to put the Registry DSL
        /// declarations.  This is not mandatory.
        /// </summary>
        protected virtual void configure()
        {
            // no-op;
        }

        protected void registerAction(Action action)
        {
            _basicActions.Add(action);
        }

        internal void addExpression(Action<PluginGraph> alteration)
        {
            _actions.Add(alteration);
        }

        internal void ConfigurePluginGraph(PluginGraph graph)
        {
            if (graph.Registries.Contains(this)) return;

            graph.Log.StartSource("Registry:  " + TypePath.GetAssemblyQualifiedName(GetType()));

            _basicActions.ForEach(action => action());
            _actions.ForEach(action => action(graph));

            graph.Registries.Add(this);
        }


        /// <summary>
        /// Expression Builder used to define policies for a PluginType including
        /// Scoping, the Default Instance, and interception.  BuildInstancesOf()
        /// and ForRequestedType() are synonyms
        /// </summary>
        /// <typeparam name="PLUGINTYPE"></typeparam>
        /// <returns></returns>
        public CreatePluginFamilyExpression<PLUGINTYPE> BuildInstancesOf<PLUGINTYPE>()
        {
            return new CreatePluginFamilyExpression<PLUGINTYPE>(this);
        }

        /// <summary>
        /// Expression Builder used to define policies for a PluginType including
        /// Scoping, the Default Instance, and interception.  This method is specifically
        /// meant for registering open generic types
        /// </summary>
        /// <typeparam name="PLUGINTYPE"></typeparam>
        /// <returns></returns>
        public GenericFamilyExpression ForRequestedType(Type pluginType)
        {
            return new GenericFamilyExpression(pluginType, this);
        }

        /// <summary>
        /// This method is a shortcut for specifying the default constructor and 
        /// setter arguments for a ConcreteType.  ForConcreteType is shorthand for:
        /// ForRequestedType[T]().TheDefault.Is.OfConcreteType[T].**************
        /// when the PluginType and ConcreteType are the same Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BuildWithExpression<T> ForConcreteType<T>()
        {
            SmartInstance<T> instance = ForRequestedType<T>().TheDefault.Is.OfConcreteType<T>();
            return new BuildWithExpression<T>(instance);
        }

        /// <summary>
        /// Expression Builder used to define policies for a PluginType including
        /// Scoping, the Default Instance, and interception.  BuildInstancesOf()
        /// and ForRequestedType() are synonyms
        /// </summary>
        /// <typeparam name="PLUGINTYPE"></typeparam>
        /// <returns></returns>
        public CreatePluginFamilyExpression<PLUGINTYPE> ForRequestedType<PLUGINTYPE>()
        {
            return new CreatePluginFamilyExpression<PLUGINTYPE>(this);
        }

        /// <summary>
        /// Uses the configuration expressions of this Registry to create a PluginGraph
        /// object that could be used to initialize a Container.  This method is 
        /// mostly for internal usage, but might be helpful for diagnostics
        /// </summary>
        /// <returns></returns>
        public PluginGraph Build()
        {
            var graph = new PluginGraph();
            ConfigurePluginGraph(graph);
            graph.Seal();

            return graph;
        }

        /// <summary>
        /// Adds an additional, non-Default Instance to the PluginType T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IsExpression<T> InstanceOf<T>()
        {
            return new InstanceExpression<T>(instance =>
            {
                Action<PluginGraph> alteration = g => g.FindFamily(typeof (T)).AddInstance(instance);
                _actions.Add(alteration);
            });
        }

        /// <summary>
        /// Adds an additional, non-Default Instance to the designated pluginType
        /// This method is mostly meant for open generic types
        /// </summary>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        public GenericIsExpression InstanceOf(Type pluginType)
        {
            return
                new GenericIsExpression(
                    instance => { _actions.Add(graph => { graph.FindFamily(pluginType).AddInstance(instance); }); });
        }

        /// <summary>
        /// Expression Builder to define the defaults for a named Profile.  Each call
        /// to CreateProfile is additive.
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public ProfileExpression CreateProfile(string profileName)
        {
            var expression = new ProfileExpression(profileName, this);

            return expression;
        }

        /// <summary>
        /// An alternative way to use CreateProfile that uses ProfileExpression
        /// as a Nested Closure.  This usage will result in cleaner code for 
        /// multiple declarations
        /// </summary>
        /// <param name="profileName"></param>
        /// <param name="action"></param>
        public void CreateProfile(string profileName, Action<ProfileExpression> action)
        {
            var expression = new ProfileExpression(profileName, this);
            action(expression);
        }

        internal static bool IsPublicRegistry(Type type)
        {
            if (!typeof (Registry).IsAssignableFrom(type))
            {
                return false;
            }

            if (type.IsInterface || type.IsAbstract || type.IsGenericType)
            {
                return false;
            }

            return (type.GetConstructor(new Type[0]) != null);
        }

        /// <summary>
        /// Registers a new TypeInterceptor object with the Container
        /// </summary>
        /// <param name="interceptor"></param>
        public void RegisterInterceptor(TypeInterceptor interceptor)
        {
            addExpression(pluginGraph => pluginGraph.InterceptorLibrary.AddInterceptor(interceptor));
        }

        /// <summary>
        /// Allows you to define a TypeInterceptor inline with Lambdas or anonymous delegates
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        /// <example>
        /// IfTypeMatches( ... ).InterceptWith( o => new ObjectWrapper(o) );
        /// </example>
        public MatchedTypeInterceptor IfTypeMatches(Predicate<Type> match)
        {
            var interceptor = new MatchedTypeInterceptor(match);
            _actions.Add(graph => graph.InterceptorLibrary.AddInterceptor(interceptor));

            return interceptor;
        }


        /// <summary>
        /// Designates a policy for scanning assemblies to auto
        /// register types
        /// </summary>
        /// <returns></returns>
        public void Scan(Action<IAssemblyScanner> action)
        {
            var scanner = new AssemblyScanner();
            action(scanner);

            _actions.Add(graph => graph.AddScanner(scanner));
        }

        public bool Equals(Registry obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return GetType().Equals(obj.GetType());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj is Registry) return false;


            if (obj.GetType() != typeof (Registry)) return false;
            return Equals((Registry) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// Directs StructureMap to always inject dependencies into any and all public Setter properties
        /// of the type PLUGINTYPE.
        /// </summary>
        /// <typeparam name="PLUGINTYPE"></typeparam>
        /// <returns></returns>
        public CreatePluginFamilyExpression<PLUGINTYPE> FillAllPropertiesOfType<PLUGINTYPE>()
        {
            PluginCache.AddFilledType(typeof (PLUGINTYPE));
            return ForRequestedType<PLUGINTYPE>();
        }

        #region Nested type: BuildWithExpression

        /// <summary>
        /// Define the constructor and setter arguments for the default T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class BuildWithExpression<T>
        {
            private readonly SmartInstance<T> _instance;

            public BuildWithExpression(SmartInstance<T> instance)
            {
                _instance = instance;
            }

            public SmartInstance<T> Configure
            {
                get { return _instance; }
            }
        }

        #endregion
    }
}