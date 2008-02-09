using System;
using System.Collections.Generic;
using System.Reflection;
using Rhino.Mocks;
using StructureMap.Configuration.Mementos;
using StructureMap.Graph;

namespace StructureMap.AutoMocking
{
    public delegate void GenericVoidMethod<TARGETCLASS>(TARGETCLASS target);

    public delegate void VoidMethod();

    // Note that it subclasses the RhinoMocks.MockRepository class
    public class RhinoAutoMocker<TARGETCLASS> : MockRepository where TARGETCLASS : class
    {
        private readonly AutoMockedInstanceManager _manager;
        private TARGETCLASS _classUnderTest;

        public RhinoAutoMocker()
        {
            RhinoMocksServiceLocator locator = new RhinoMocksServiceLocator(this);
            _manager = new AutoMockedInstanceManager(locator);
        }

        // Replaces the inner InstanceManager in ObjectFactory with the mocked
        // InstanceManager from the auto mocking container.  This will make ObjectFactory
        // return mocks for everything.  Use cautiously!!!!!!!!!!!!!!!
        public void MockObjectFactory()
        {
            ObjectFactory.ReplaceManager(_manager);
        }

        // Gets the ClassUnderTest with mock objects (or stubs) pushed in
        // for all of its dependencies
        public TARGETCLASS ClassUnderTest
        {
            get
            {
                if (_classUnderTest == null)
                {
                    _classUnderTest = _manager.FillDependencies<TARGETCLASS>();
                }

                return _classUnderTest;
            }
        }

        // I find it useful from time to time to use partial mocks for the ClassUnderTest
        // Especially in Presenter testing
        public void PartialMockTheClassUnderTest()
        {
            _classUnderTest = PartialMock<TARGETCLASS>(getConstructorArgs());
        }

        private object[] getConstructorArgs()
        {
            ConstructorInfo ctor = Plugin.GetGreediestConstructor(typeof (TARGETCLASS));
            List<object> list = new List<object>();
            foreach (ParameterInfo parameterInfo in ctor.GetParameters())
            {
                Type dependencyType = parameterInfo.ParameterType;
                object dependency = _manager.CreateInstance(dependencyType);
                list.Add(dependency);
            }

            return list.ToArray();
        }

        // Get one of the mock objects that are injected into the constructor function
        // of the ClassUnderTest
        public T Get<T>()
        {
            return _manager.CreateInstance<T>();
        }

        // Set the auto mocking container to use a Stub for Type T
        public void InjectStub<T>(T stub)
        {
            _manager.InjectStub<T>(stub);
        }
    }



}