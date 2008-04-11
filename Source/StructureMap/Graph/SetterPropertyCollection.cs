using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using StructureMap.Attributes;

namespace StructureMap.Graph
{
    /// <summary>
    /// Custom collection class for SetterProperty objects
    /// </summary>
    public class SetterPropertyCollection : IEnumerable<SetterProperty>
    {
        private readonly Plugin _plugin;
        private Dictionary<string, SetterProperty> _properties;

        public SetterPropertyCollection(Plugin plugin)
        {
            _properties = new Dictionary<string, SetterProperty>();
            _plugin = plugin;


            PropertyInfo[] properties = SetterPropertyAttribute.FindMarkedProperties(plugin.PluggedType);
            foreach (PropertyInfo property in properties)
            {
                addSetterProperty(property, property.Name);
            }
        }

        public SetterProperty[] Setters
        {
            get
            {
                SetterProperty[] returnValue = new SetterProperty[_properties.Count];
                _properties.Values.CopyTo(returnValue, 0);
                
                return returnValue;
            }
        }

        public int Count
        {
            get { return _properties.Count; }
        }

        public SetterProperty Add(string propertyName)
        {
            PropertyInfo property = _plugin.PluggedType.GetProperty(propertyName);
            addSetterProperty(property, propertyName);

            return _properties[propertyName];
        }

        private void addSetterProperty(PropertyInfo property, string propertyName)
        {
            if (property == null)
            {
                throw new StructureMapException(240, propertyName, _plugin.PluggedType);
            }

            if (property.GetSetMethod() == null)
            {
                throw new StructureMapException(241, propertyName, _plugin.PluggedType);
            }

            SetterProperty setterProperty = new SetterProperty(property);
            _properties.Add(propertyName, setterProperty);
        }

        public bool Contains(string propertyName)
        {
            return _properties.ContainsKey(propertyName);
        }

        IEnumerator<SetterProperty> IEnumerable<SetterProperty>.GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<SetterProperty>) this).GetEnumerator();
        }
    }
}