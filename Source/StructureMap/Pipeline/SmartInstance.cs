using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Interceptors;

namespace StructureMap.Pipeline
{
    public class SmartInstance<T> : ConfiguredInstanceBase<SmartInstance<T>>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();

        public SmartInstance() : base(typeof (T))
        {
        }

        public SmartInstance<T> WithName(string instanceKey)
        {
            Name = instanceKey;
            return this;
        }

        public SmartInstance<T> OnCreation(Action<T> handler)
        {
            var interceptor = new StartupInterceptor<T>(handler);
            Interceptor = interceptor;

            return this;
        }

        public SmartInstance<T> EnrichWith(EnrichmentHandler<T> handler)
        {
            var interceptor = new EnrichmentInterceptor<T>(handler);
            Interceptor = interceptor;

            return this;
        }

        public SmartInstance<T> EnrichWith<PLUGINTYPE>(EnrichmentHandler<PLUGINTYPE> handler)
        {
            var interceptor = new EnrichmentInterceptor<PLUGINTYPE>(handler);
            Interceptor = interceptor;

            return this;
        }

        protected override string getDescription()
        {
            return "Smart Instance for " + getConcreteType(null).FullName;
        }

        public PropertyExpression<SmartInstance<T>> WithCtorArg(string argumentName)
        {
            return new PropertyExpression<SmartInstance<T>>(this, argumentName);
        }

        protected override object build(Type pluginType, BuildSession session)
        {
            var builtTarget = (T) base.build(pluginType, session);
            foreach (var action in _actions)
            {
                action(builtTarget);
            }

            return builtTarget;
        }

        public SmartInstance<T> SetProperty(Action<T> action)
        {
            _actions.Add(action);
            return this;
        }

        public PropertyExpression<SmartInstance<T>> WithProperty(Expression<Func<T, object>> expression)
        {
            string propertyName = ReflectionHelper.GetProperty(expression).Name;
            return WithProperty(propertyName);
        }

        public PropertyExpression<SmartInstance<T>> WithProperty(string propertyName)
        {
            return new PropertyExpression<SmartInstance<T>>(this, propertyName);
        }

        public DependencyExpression<T, CTORTYPE> CtorDependency<CTORTYPE>()
        {
            string constructorArg = getArgumentNameForType<CTORTYPE>();
            return CtorDependency<CTORTYPE>(constructorArg);
        }

        private string getArgumentNameForType<CTORTYPE>()
        {
            Plugin plugin = PluginCache.GetPlugin(getConcreteType(null));
            return plugin.FindArgumentNameForType<CTORTYPE>();
        }

        public DependencyExpression<T, CTORTYPE> CtorDependency<CTORTYPE>(string constructorArg)
        {
            return new DependencyExpression<T, CTORTYPE>(this, constructorArg);
        }

        public DependencyExpression<T, SETTERTYPE> SetterDependency<SETTERTYPE>(
            Expression<Func<T, SETTERTYPE>> expression)
        {
            string propertyName = ReflectionHelper.GetProperty(expression).Name;
            return new DependencyExpression<T, SETTERTYPE>(this, propertyName);
        }

        public DependencyExpression<T, SETTERTYPE> SetterDependency<SETTERTYPE>()
        {
            return CtorDependency<SETTERTYPE>();
        }

        public ArrayDefinitionExpression<T, CHILD> TheArrayOf<CHILD>()
        {
            if (typeof (CHILD).IsArray)
            {
                throw new ApplicationException("Please specify the element type in the call to TheArrayOf");
            }

            Plugin plugin = PluginCache.GetPlugin(typeof (T));
            string propertyName = plugin.FindArgumentNameForType(typeof (CHILD).MakeArrayType());

            return TheArrayOf<CHILD>(propertyName);
        }

        public ArrayDefinitionExpression<T, CHILD> TheArrayOf<CHILD>(string ctorOrPropertyName)
        {
            return new ArrayDefinitionExpression<T, CHILD>(this, ctorOrPropertyName);
        }

        #region Nested type: ArrayDefinitionExpression

        public class ArrayDefinitionExpression<T, ARRAY>
        {
            private readonly SmartInstance<T> _instance;
            private readonly string _propertyName;

            internal ArrayDefinitionExpression(SmartInstance<T> instance, string propertyName)
            {
                _instance = instance;
                _propertyName = propertyName;
            }

            public SmartInstance<T> Contains(Action<InstanceExpression<ARRAY>> action)
            {
                var list = new List<Instance>();

                var child = new InstanceExpression<ARRAY>(i => list.Add(i));
                action(child);

                _instance.setChildArray(_propertyName, list.ToArray());

                return _instance;
            }

            public SmartInstance<T> Contains(params Instance[] arrayInstances)
            {
                _instance.setChildArray(_propertyName, arrayInstances);

                return _instance;
            }
        }

        #endregion

        #region Nested type: DependencyExpression

        public class DependencyExpression<T, CHILD>
        {
            private readonly SmartInstance<T> _instance;
            private readonly string _propertyName;

            internal DependencyExpression(SmartInstance<T> instance, string propertyName)
            {
                _instance = instance;
                _propertyName = propertyName;
            }

            public SmartInstance<T> Is(Action<InstanceExpression<CHILD>> action)
            {
                var expression = new InstanceExpression<CHILD>(i => _instance.setChild(_propertyName, i));
                action(expression);

                return _instance;
            }

            public SmartInstance<T> Is(Instance instance)
            {
                _instance.setChild(_propertyName, instance);
                return _instance;
            }

            public SmartInstance<T> Is(CHILD value)
            {
                return Is(new LiteralInstance(value));
            }

            public SmartInstance<T> IsTheDefault()
            {
                return Is(new DefaultInstance());
            }

            public SmartInstance<T> Is<CONCRETETYPE>() where CONCRETETYPE : CHILD
            {
                return Is(new SmartInstance<CONCRETETYPE>());
            }
        }

        #endregion
    }
}