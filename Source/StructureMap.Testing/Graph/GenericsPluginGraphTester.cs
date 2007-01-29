using System;
using NUnit.Framework;
using StructureMap.Graph;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class GenericsPluginGraphTester
    {
        [SetUp]
        public void SetUp()
        {
        }

        private void assertCanBeCast(Type pluginType, Type pluggedType)
        {
            Assert.IsTrue(GenericsPluginGraph.CanBeCast(pluginType, pluggedType));
        }

        private void assertCanNotBeCast(Type pluginType, Type pluggedType)
        {
            Assert.IsFalse(GenericsPluginGraph.CanBeCast(pluginType, pluggedType));
        }


        [Test]
        public void DirectImplementationOfInterfaceCanBeCast()
        {
            assertCanBeCast(typeof (IGenericService<>), typeof (GenericService<>));
            assertCanNotBeCast(typeof (IGenericService<>), typeof (SpecificService<>));
        }

        [Test]
        public void DirectInheritanceOfAbstractClassCanBeCast()
        {
            assertCanBeCast(typeof (BaseSpecificService<>), typeof (SpecificService<>));
        }

        [Test]
        public void ImplementationOfInterfaceFromBaseType()
        {
            assertCanBeCast(typeof (ISomething<>), typeof (SpecificService<>));
        }

        [Test]
        public void RecursiveImplementation()
        {
            assertCanBeCast(typeof (ISomething<>), typeof (SpecificService<>));
            assertCanBeCast(typeof (ISomething<>), typeof (GrandChildSpecificService<>));
        }

        [Test]
        public void RecursiveInheritance()
        {
            assertCanBeCast(typeof (BaseSpecificService<>), typeof (ChildSpecificService<>));
            assertCanBeCast(typeof (BaseSpecificService<>), typeof (GrandChildSpecificService<>));
        }


        [Test]
        public void BuildTemplatedFamilyWithOnlyOneTemplateParameter()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService<>));
            family.Plugins.Add(typeof (GenericService<>), "Default");
            family.Plugins.Add(typeof (SecondGenericService<>), "Second");
            family.Plugins.Add(typeof (ThirdGenericService<>), "Third");

            PluginFamily templatedFamily = family.CreateTemplatedClone(typeof (int));

            Assert.IsNotNull(templatedFamily);
            Assert.AreEqual(typeof (IGenericService<int>), templatedFamily.PluginType);

            Assert.AreEqual(3, templatedFamily.Plugins.Count);
            Assert.AreEqual(typeof (GenericService<int>), templatedFamily.Plugins["Default"].PluggedType);
            Assert.AreEqual(typeof (SecondGenericService<int>), templatedFamily.Plugins["Second"].PluggedType);
            Assert.AreEqual(typeof (ThirdGenericService<int>), templatedFamily.Plugins["Third"].PluggedType);
        }

        [Test]
        public void GetTemplatedFamily()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService<>));
            family.Plugins.Add(typeof (GenericService<>), "Default");
            family.Plugins.Add(typeof (SecondGenericService<>), "Second");
            family.Plugins.Add(typeof (ThirdGenericService<>), "Third");

            GenericsPluginGraph genericsGraph = new GenericsPluginGraph();
            genericsGraph.AddFamily(family);

            PluginFamily templatedFamily = genericsGraph.CreateTemplatedFamily(typeof (IGenericService<int>));

            Assert.IsNotNull(templatedFamily);
            Assert.AreEqual(typeof (IGenericService<int>), templatedFamily.PluginType);
        }

        [Test]
        public void BuildAnInstanceManagerFromTemplatedPluginFamily()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService<>));
            family.DefaultInstanceKey = "Default";
            family.Plugins.Add(typeof (GenericService<>), "Default");
            family.Plugins.Add(typeof (SecondGenericService<>), "Second");
            family.Plugins.Add(typeof (ThirdGenericService<>), "Third");

            PluginFamily intFamily = family.CreateTemplatedClone(typeof (int));
            PluginFamily stringFamily = family.CreateTemplatedClone(typeof (string));

            InstanceFactory intFactory = new InstanceFactory(intFamily, true);
            InstanceFactory stringFactory = new InstanceFactory(stringFamily, true);

            GenericService<int> intService = (GenericService<int>) intFactory.GetInstance();
            Assert.AreEqual(typeof (int), intService.GetT());

            Assert.IsInstanceOfType(typeof (SecondGenericService<int>), intFactory.GetInstance("Second"));

            GenericService<string> stringService = (GenericService<string>) stringFactory.GetInstance();
            Assert.AreEqual(typeof (string), stringService.GetT());
        }


        [Test]
        public void BuildTemplatedFamilyWithThreeTemplateParameters()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService3<,,>));
            family.Plugins.Add(typeof (GenericService3<,,>), "Default");
            family.Plugins.Add(typeof (SecondGenericService3<,,>), "Second");
            family.Plugins.Add(typeof (ThirdGenericService3<,,>), "Third");

            PluginFamily templatedFamily = family.CreateTemplatedClone(typeof (int), typeof (bool), typeof (string));

            Assert.IsNotNull(templatedFamily);
            Assert.AreEqual(typeof (IGenericService3<int, bool, string>), templatedFamily.PluginType);

            Assert.AreEqual(3, templatedFamily.Plugins.Count);
            Assert.AreEqual(typeof (GenericService3<int, bool, string>), templatedFamily.Plugins["Default"].PluggedType);
            Assert.AreEqual(typeof (SecondGenericService3<int, bool, string>),
                            templatedFamily.Plugins["Second"].PluggedType);
            Assert.AreEqual(typeof (ThirdGenericService3<int, bool, string>),
                            templatedFamily.Plugins["Third"].PluggedType);
        }
    }

    public interface IGenericService<T>
    {
    }

    public class GenericService<T> : IGenericService<T>
    {
        public Type GetT()
        {
            return typeof (T);
        }
    }

    public class SecondGenericService<T> : IGenericService<T>
    {
    }

    public class ThirdGenericService<T> : IGenericService<T>
    {
    }

    public interface ISomething<T>
    {
    }

    public abstract class BaseSpecificService<T> : ISomething<T>
    {
    }

    public class SpecificService<T> : BaseSpecificService<T>
    {
    }

    public class ChildSpecificService<T> : SpecificService<T>
    {
    }

    public class GrandChildSpecificService<T> : ChildSpecificService<T>
    {
    }


    public interface IGenericService3<T, U, V>
    {
    }

    public class GenericService3<T, U, V> : IGenericService3<T, U, V>
    {
        public Type GetT()
        {
            return typeof (T);
        }
    }

    public class SecondGenericService3<T, U, V> : IGenericService3<T, U, V>
    {
    }

    public class ThirdGenericService3<T, U, V> : IGenericService3<T, U, V>
    {
    }
}