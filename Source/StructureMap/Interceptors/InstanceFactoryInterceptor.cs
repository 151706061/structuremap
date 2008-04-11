using System;
using System.Collections;
using StructureMap.Pipeline;

namespace StructureMap.Interceptors
{
    /// <summary>
    /// Base "Decorator" class around IInstanceFactory to alter the object creation process
    /// for a PluginType.  The SingletonInterceptor is an example subclass that ensures that 
    /// only one instance is created for a given InstanceKey as a more testable alternative to 
    /// the GoF Singleton pattern. 
    /// </summary>
    [PluginFamily]
    public abstract class InstanceFactoryInterceptor : IInstanceFactory, ICloneable
    {
        private IInstanceFactory _innerInstanceFactory;


        public virtual IInstanceFactory InnerInstanceFactory
        {
            get { return _innerInstanceFactory; }
            set { _innerInstanceFactory = value; }
        }

        /// <summary>
        /// Declares whether or not the interceptor creates a stubbed or mocked version of the PluginType 
        /// </summary>
        public virtual bool IsMockedOrStubbed
        {
            get { return false; }
        }

        #region ICloneable Members

        public abstract object Clone();

        #endregion

        #region IInstanceFactory Members

        /// <summary>
        /// Establishes a reference to the parent InstanceManager
        /// </summary>
        /// <param name="instanceManager"></param>
        public void SetInstanceManager(InstanceManager instanceManager)
        {
            InnerInstanceFactory.SetInstanceManager(instanceManager);
        }

        /// <summary>
        /// The CLR System.Type that the IInstanceManager builds instances  
        /// </summary>
        public Type PluginType
        {
            get { return InnerInstanceFactory.PluginType; }
        }

        /// <summary>
        /// Creates an object instance for the InstanceKey
        /// </summary>
        /// <param name="instanceKey">The named instance</param>
        /// <returns></returns>
        public virtual object GetInstance(string instanceKey)
        {
            return InnerInstanceFactory.GetInstance(instanceKey);
        }

        /// <summary>
        /// Creates an object instance directly from the Memento
        /// </summary>
        /// <param name="instance">A representation of an object instance</param>
        /// <returns></returns>
        public virtual object GetInstance(IConfiguredInstance instance, IInstanceCreator instanceCreator)
        {
            return InnerInstanceFactory.GetInstance(instance, instanceCreator);
        }

        /// <summary>
        /// Creates a new object instance of the default instance memento
        /// </summary>
        /// <returns></returns>
        public virtual object GetInstance()
        {
            return InnerInstanceFactory.GetInstance();
        }

        /// <summary>
        /// Sets the default instance 
        /// </summary>
        /// <param name="instanceKey"></param>
        public void SetDefault(string instanceKey)
        {
            InnerInstanceFactory.SetDefault(instanceKey);
        }

        /// <summary>
        /// Makes the InstanceMemento the basis of the default instance
        /// </summary>
        /// <param name="instance"></param>
        public void SetDefault(Instance instance)
        {
            InnerInstanceFactory.SetDefault(instance);
        }

        /// <summary>
        /// The InstanceKey of the default instance built by this IInstanceFactory
        /// </summary>
        public string DefaultInstanceKey
        {
            get { return InnerInstanceFactory.DefaultInstanceKey; }
        }

        public IList GetAllInstances()
        {
            return InnerInstanceFactory.GetAllInstances();
        }

        public void AddInstance(Instance instance)
        {
            InnerInstanceFactory.AddInstance(instance);
        }

        public Instance AddType<T>()
        {
            return InnerInstanceFactory.AddType<T>();
        }

        public Instance GetDefault()
        {
            return InnerInstanceFactory.GetDefault();
        }


        public virtual object ApplyInterception(object rawValue)
        {
            return InnerInstanceFactory.ApplyInterception(rawValue);
        }

        #endregion
    }
}