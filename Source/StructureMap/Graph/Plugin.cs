using System;
using System.Reflection;
using StructureMap.Configuration.Mementos;

namespace StructureMap.Graph
{
    /// <summary>
    /// Represents a concrete class that can be built by StructureMap as an instance of the parent 
    /// PluginFamily�s PluginType. The properties of a Plugin are the CLR Type of the concrete class, 
    /// and the human-friendly concrete key that StructureMap will use to identify the Type.
    /// </summary>
    public class Plugin
    {
        #region static

        public static Plugin CreateAutofilledPlugin(Type concreteType)
        {
            string pluginKey = Guid.NewGuid().ToString();
            Plugin plugin = CreateExplicitPlugin(concreteType, pluginKey, string.Empty);
            if (!plugin.CanBeAutoFilled)
            {
                throw new StructureMapException(231);
            }

            return plugin;
        }


        /// <summary>
        /// Determines if the PluggedType is a valid Plugin into the
        /// PluginType
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="pluggedType"></param>
        /// <returns></returns>
        public static bool IsAnExplicitPlugin(Type pluginType, Type pluggedType)
        {
            bool returnValue = false;

            bool markedAsPlugin = PluggableAttribute.MarkedAsPluggable(pluggedType);
            if (markedAsPlugin)
            {
                returnValue = CanBeCast(pluginType, pluggedType);
            }

            return returnValue;
        }


        /// <summary>
        /// Determines if the pluggedType can be upcast to the pluginType
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="pluggedType"></param>
        /// <returns></returns>
        public static bool CanBeCast(Type pluginType, Type pluggedType)
        {
            if (pluggedType.IsInterface || pluggedType.IsAbstract)
            {
                return false;
            }

            if (GenericsPluginGraph.CanBeCast(pluginType, pluggedType))
            {
                return true;
            }

            ConstructorInfo constructor = GetGreediestConstructor(pluggedType);
            if (constructor == null)
            {
                return false;
            }

            return pluginType.IsAssignableFrom(pluggedType);
        }


        /// <summary>
        /// Creates an Implicit Plugin that discovers its ConcreteKey from a [Pluggable]
        /// attribute on the PluggedType 
        /// </summary>
        /// <param name="pluggedType"></param>
        /// <returns></returns>
        public static Plugin CreateImplicitPlugin(Type pluggedType)
        {
            PluggableAttribute att = PluggableAttribute.InstanceOf(pluggedType);
            if (att == null)
            {
                return
                    new Plugin(pluggedType, TypePath.GetAssemblyQualifiedName(pluggedType), DefinitionSource.Implicit);
            }
            else
            {
                return new Plugin(pluggedType, att.ConcreteKey, DefinitionSource.Implicit);
            }
        }

        /// <summary>
        /// Creates an Explicit Plugin for the pluggedType with the entered
        /// concreteKey
        /// </summary>
        /// <param name="pluggedType"></param>
        /// <param name="concreteKey"></param>
        /// <param name="description"></param>
        public static Plugin CreateExplicitPlugin(Type pluggedType, string concreteKey, string description)
        {
            return new Plugin(pluggedType, concreteKey, DefinitionSource.Explicit);
        }

        public static ConstructorInfo GetGreediestConstructor(Type pluggedType)
        {
            ConstructorInfo returnValue = null;

            foreach (ConstructorInfo constructor in pluggedType.GetConstructors())
            {
                if (returnValue == null)
                {
                    returnValue = constructor;
                }
                else if (constructor.GetParameters().Length > returnValue.GetParameters().Length)
                {
                    returnValue = constructor;
                }
            }

            return returnValue;
        }

        #endregion

        private string _concreteKey;
        private DefinitionSource _definitionSource;
        private Type _pluggedType;
        private SetterPropertyCollection _setters;


        /// <summary>
        /// Creates an Explicit Plugin for the pluggedType with the entered
        /// concreteKey
        /// </summary>
        /// <param name="pluggedType"></param>
        /// <param name="concreteKey"></param>
        private Plugin(Type pluggedType, string concreteKey, DefinitionSource definitionSource) : base()
        {
            if (concreteKey == string.Empty)
            {
                throw new StructureMapException(112, pluggedType.FullName);
            }

            _pluggedType = pluggedType;
            _concreteKey = concreteKey;
            _definitionSource = definitionSource;
            _setters = new SetterPropertyCollection(this);
        }

        /// <summary>
        /// Troubleshooting constructor used by PluginGraphBuilder to find possible problems
        /// with the configured Plugin
        /// </summary>
        /// <param name="path"></param>
        /// <param name="concreteKey"></param>
        public Plugin(TypePath path, string concreteKey) : base()
        {
            if (concreteKey == string.Empty)
            {
                throw new StructureMapException(112, path.ClassName);
            }

            setPluggedType(path, concreteKey);
            _setters = new SetterPropertyCollection(this);

            _concreteKey = concreteKey;
            _definitionSource = DefinitionSource.Explicit;
        }


        /// <summary>
        /// The ConcreteKey that identifies the Plugin within a PluginFamily
        /// </summary>
        public string ConcreteKey
        {
            get { return _concreteKey; }
            set { _concreteKey = value; }
        }


        /// <summary>
        /// The concrete CLR Type represented by the Plugin
        /// </summary>
        public Type PluggedType
        {
            get { return _pluggedType; }
        }

        /// <summary>
        /// Finds any methods on the PluggedType marked with the [ValidationMethod]
        /// attributes
        /// </summary>
        public MethodInfo[] ValidationMethods
        {
            get { return ValidationMethodAttribute.GetValidationMethods(_pluggedType); }
        }

        /// <summary>
        /// Property's that will be filled by setter injection
        /// </summary>
        public SetterPropertyCollection Setters
        {
            get
            {
                if (_setters == null)
                {
                    _setters = new SetterPropertyCollection(this);
                }

                return _setters;
            }
        }

        /// <summary>
        /// Denotes the source or the definition for this Plugin.  Implicit means the
        /// Plugin is defined by a [Pluggable] attribute on the PluggedType.  Explicit
        /// means the Plugin was defined in the StructureMap.config file.
        /// </summary>
        public DefinitionSource DefinitionSource
        {
            get { return _definitionSource; }
            set { _definitionSource = value; }
        }

        /// <summary>
        /// Determines if the concrete class can be autofilled.
        /// </summary>
        public bool CanBeAutoFilled
        {
            get
            {
                bool returnValue = true;

                ConstructorInfo ctor = GetConstructor();
                foreach (ParameterInfo parameter in ctor.GetParameters())
                {
                    returnValue = returnValue && canTypeBeAutoFilled(parameter.ParameterType);
                }

                foreach (SetterProperty setter in Setters)
                {
                    Type propertyType = setter.Property.PropertyType;
                    returnValue = returnValue && canTypeBeAutoFilled(propertyType);
                }

                return returnValue;
            }
        }

        private void setPluggedType(TypePath path, string concreteKey)
        {
            try
            {
                _pluggedType = path.FindType();
            }
            catch (Exception ex)
            {
                throw new StructureMapException(111, ex, path.ClassName, concreteKey);
            }
        }


        public Plugin CreateTemplatedClone(params Type[] types)
        {
            Type templatedType;
            if (_pluggedType.IsGenericType)
            {
                templatedType = _pluggedType.MakeGenericType(types);
            }
            else
            {
                templatedType = _pluggedType;
            }
            Plugin templatedPlugin = new Plugin(templatedType, _concreteKey, _definitionSource);
            templatedPlugin._setters = _setters;

            return templatedPlugin;
        }


        /// <summary>
        /// Returns the System.Reflection.ConstructorInfo for the PluggedType.  Uses either
        /// the "greediest" constructor with the most arguments or the constructor function
        /// marked with the [DefaultConstructor]
        /// </summary>
        /// <returns></returns>
        public ConstructorInfo GetConstructor()
        {
            ConstructorInfo returnValue = DefaultConstructorAttribute.GetConstructor(_pluggedType);

            // if no constructor is marked as the "ContainerConstructor", find the greediest constructor
            if (returnValue == null)
            {
                returnValue = GetGreediestConstructor(_pluggedType);
            }

            if (returnValue == null)
            {
                throw new StructureMapException(180, _pluggedType.Name);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets a class name for the InstanceBuilder that will be emitted for this Plugin
        /// </summary>
        /// <returns></returns>
        public string GetInstanceBuilderClassName()
        {
            string className = "";

            if (_pluggedType.IsGenericType)
            {
                className += escapeClassName(_pluggedType);

                Type[] args = _pluggedType.GetGenericArguments();
                foreach (Type arg in args)
                {
                    className += escapeClassName(arg);
                }
            }
            else
            {
                className = escapeClassName(_pluggedType);
            }

            return className + "InstanceBuilder";
        }

        private string escapeClassName(Type type)
        {
            string typeName = type.Namespace + type.Name;
            string returnValue = typeName.Replace(".", string.Empty);
            return returnValue.Replace("`", string.Empty);
        }


        /// <summary>
        /// Boolean flag denoting the presence of any constructor arguments
        /// </summary>
        /// <returns></returns>
        public bool HasConstructorArguments()
        {
            return (GetConstructor().GetParameters().Length > 0);
        }

        public override string ToString()
        {
            return ("Plugin:  " + _concreteKey).PadRight(40) + PluggedType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Creates an InstanceMemento for a PluggedType that requires no
        /// configuration.  I.e. a CLR Type that has no constructor functions or 
        /// is marked as "[AutoFilled]"
        /// </summary>
        /// <returns></returns>
        public InstanceMemento CreateImplicitMemento()
        {
            InstanceMemento returnValue = null;

            if (CanBeAutoFilled)
            {
                MemoryInstanceMemento memento = new MemoryInstanceMemento(ConcreteKey, ConcreteKey);
                memento.DefinitionSource = DefinitionSource.Implicit;

                returnValue = memento;
            }

            return returnValue;
        }

        private bool canTypeBeAutoFilled(Type parameterType)
        {
            bool cannotBeFilled = false;

            cannotBeFilled = cannotBeFilled || parameterType.IsValueType;
            cannotBeFilled = cannotBeFilled || parameterType.IsArray;
            cannotBeFilled = cannotBeFilled || parameterType.Equals(typeof (string));

            return !cannotBeFilled;
        }


        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            Plugin plugin = obj as Plugin;
            if (plugin == null) return false;
            return Equals(_pluggedType, plugin._pluggedType) && Equals(_concreteKey, plugin._concreteKey);
        }

        public override int GetHashCode()
        {
            return
                (_pluggedType != null ? _pluggedType.GetHashCode() : 0) +
                29*(_concreteKey != null ? _concreteKey.GetHashCode() : 0);
        }

        public string FindFirstConstructorArgumentOfType<T>()
        {
            ConstructorInfo ctor = GetConstructor();
            foreach (ParameterInfo info in ctor.GetParameters())
            {
                if (info.ParameterType.Equals(typeof (T)))
                {
                    return info.Name;
                }
            }

            throw new StructureMapException(302, typeof (T).FullName, _pluggedType.FullName);
        }

        public void AddToSource(MementoSource source)
        {
            InstanceMemento memento = CreateImplicitMemento();
            if (memento != null)
            {
                source.AddExternalMemento(memento);
            }
        }
    }
}