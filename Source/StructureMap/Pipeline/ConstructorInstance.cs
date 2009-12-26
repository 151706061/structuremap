using System;
using System.Collections.Generic;
using System.ComponentModel;
using StructureMap.Construction;
using StructureMap.Graph;
using StructureMap.TypeRules;
using StructureMap.Util;
using System.Linq;

namespace StructureMap.Pipeline
{
    

    public class ConstructorInstance : Instance, IConfiguredInstance, IStructuredInstance
    {
        private readonly Cache<string, Instance> _dependencies = new Cache<string, Instance>();
        private readonly Plugin _plugin;

        public ConstructorInstance(Type pluggedType) : this(PluginCache.GetPlugin(pluggedType))
        {

        }

        protected override bool canBePartOfPluginFamily(PluginFamily family)
        {
            return _plugin.PluggedType.CanBeCastTo(family.PluginType);
        }

        public ConstructorInstance(Plugin plugin)
        {
            _plugin = plugin;
        
            _dependencies.OnMissing = key =>
            {
                if (_plugin.FindArgumentType(key).IsSimple())
                {
                    throw new StructureMapException(205, key, Name);
                }

                return new DefaultInstance();
            };
        }

        public ConstructorInstance Override(ExplicitArguments arguments)
        {
            var instance = new ConstructorInstance(_plugin);
            _dependencies.Each((key, i) => instance.SetChild(key, i));

            arguments.Configure(instance);

            return instance;
        }

        public ConstructorInstance(Type pluggedType, string name) : this(pluggedType)
        {
            Name = name;
        }

        protected override void addTemplatedInstanceTo(PluginFamily family, Type[] templateTypes)
        {
            throw new NotImplementedException();   
        }

        protected Plugin plugin { get { return _plugin; } }

        protected sealed override string getDescription()
        {
            return "Configured Instance of " + _plugin.PluggedType.AssemblyQualifiedName;
        }

        protected sealed override Type getConcreteType(Type pluginType)
        {
            return _plugin.PluggedType;
        }

        void IConfiguredInstance.SetChild(string name, Instance instance)
        {
            SetChild(name, instance);
        }

        public void SetValue(Type type, object value)
        {
            var name = _plugin.FindArgumentNameForType(type);
            SetValue(name, value);
        }

        void IConfiguredInstance.SetValue(string name, object value)
        {
            SetValue(name, value);
        }

        void IConfiguredInstance.SetCollection(string name, IEnumerable<Instance> children)
        {
            SetCollection(name, children);
        }

        public string GetProperty(string propertyName)
        {
            return _dependencies[propertyName].As<ObjectInstance>().Object.ToString();
        }

        internal void SetChild(string name, Instance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance", "Instance for {0} was null".ToFormat(name));
            }

            _dependencies[name] = instance;
        }

        internal void SetValue(string name, object value)
        {
            Type dependencyType = getDependencyType(name);

            var instance = buildInstanceForType(dependencyType, value);
            SetChild(name, instance);
        }

        private Type getDependencyType(string name)
        {
            var dependencyType = _plugin.FindArgumentType(name);
            if (dependencyType == null)
            {
                throw new ArgumentOutOfRangeException("name",
                                                      "Could not find a constructor parameter or property for {0} named {1}"
                                                          .ToFormat(_plugin.PluggedType.AssemblyQualifiedName, name));
            }
            return dependencyType;
        }

        internal void SetCollection(string name, IEnumerable<Instance> children)
        {
            Type dependencyType = getDependencyType(name);
            var instance = new EnumerableInstance(dependencyType, children);
            SetChild(name, instance);
        }

        protected string findPropertyName<PLUGINTYPE>()
        {
            Type dependencyType = typeof(PLUGINTYPE);

            return findPropertyName(dependencyType);
        }

        protected string findPropertyName(Type dependencyType)
        {
            string propertyName = _plugin.FindArgumentNameForType(dependencyType);

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new StructureMapException(305, dependencyType);
            }

            return propertyName;
        }

        private Instance buildInstanceForType(Type dependencyType, object value)
        {
            if (value == null) return new NullInstance();


            if (dependencyType.IsSimple() || dependencyType.IsNullable() || dependencyType == typeof(Guid) || dependencyType == typeof(DateTime))
            {
                try
                {
                    if (value.GetType() == dependencyType) return new ObjectInstance(value);

                    var converter = TypeDescriptor.GetConverter(dependencyType);
                    var convertedValue = converter.ConvertFrom(value);
                    return new ObjectInstance(convertedValue);
                }
                catch (Exception e)
                {
                    throw new StructureMapException(206, e, Name);
                }
            }


            return new ObjectInstance(value);
        }

        public object Get(string propertyName, Type pluginType, BuildSession session)
        {
            return _dependencies[propertyName].Build(pluginType, session);
        }

        public T Get<T>(string propertyName, BuildSession session)
        {
            object o = Get(propertyName, typeof (T), session);
            if (o == null) return default(T);

            return (T)o;
        }

        protected override object build(Type pluginType, BuildSession session)
        {
            IInstanceBuilder builder = PluginCache.FindBuilder(_plugin.PluggedType);
            return Build(pluginType, session, builder); 
        }

        public Type PluggedType
        {
            get { return _plugin.PluggedType; }
        }

        public bool HasProperty(string propertyName, BuildSession session)
        {
            // TODO -- richer behavior
            return _dependencies.Has(propertyName);
        }

        public object Build(Type pluginType, BuildSession session, IInstanceBuilder builder)
        {
            if (builder == null)
            {
                throw new StructureMapException(
                    201, _plugin.PluggedType.FullName, Name, pluginType);
            }


            try
            {
                var args = new Arguments(this, session);
                return builder.BuildInstance(args);
            }
            catch (StructureMapException)
            {
                throw;
            }
            catch (InvalidCastException ex)
            {
                throw new StructureMapException(206, ex, Name);
            }
            catch (Exception ex)
            {
                throw new StructureMapException(207, ex, Name, pluginType.FullName);
            }
        }

        public static ConstructorInstance For<T>()
        {
            return new ConstructorInstance(typeof(T));
        }

        Instance IStructuredInstance.GetChild(string name)
        {
            return _dependencies[name];
        }

        Instance[] IStructuredInstance.GetChildArray(string name)
        {
            return _dependencies[name].As<EnumerableInstance>().Children.ToArray();
        }

        void IStructuredInstance.RemoveKey(string name)
        {
            _dependencies.Remove(name);
        }

        public override Instance CloseType(Type[] types)
        {
            if (_plugin.PluggedType.IsOpenGeneric())
            {
                var closedType = _plugin.PluggedType.MakeGenericType(types);
                var closedInstance = new ConstructorInstance(closedType);

                _dependencies.Each((key, i) =>
                {
                    if (i.CopyAsIsWhenClosingInstance)
                    {
                        closedInstance.SetChild(key, i);
                    }
                });

                return closedInstance;
            }

            return null;
        }

        public override string ToString()
        {
            return "'{0}' -> {1}".ToFormat(Name, _plugin.PluggedType.FullName);
        }
    }
}