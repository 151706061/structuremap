using System;
using System.Reflection;
using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.GenericWidgets;

namespace StructureMap.Testing
{
    [TestFixture]
    public class GenericsAcceptanceTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        [Test]
        public void BuildFamilyAndPluginThenSealAndCreateInstanceManagerWithGenericTypeWithOpenGenericParameters()
        {
            PluginGraph graph = new PluginGraph();
            graph.Assemblies.Add(Assembly.GetExecutingAssembly());
            PluginFamily family = graph.FindFamily(typeof (IGenericService<>));
            family.DefaultInstanceKey = "Default";
            family.AddPlugin(typeof (GenericService<>), "Default");

            graph.Seal();

            Container manager = new Container(graph);
        }

        [Test]
        public void CanBuildAGenericObjectThatHasAnotherGenericObjectAsAChild()
        {
            Type serviceType = typeof (IService<double>);
            PluginGraph pluginGraph = PluginGraph.BuildGraphFromAssembly(serviceType.Assembly);
            Container manager = new Container(pluginGraph);

            Type doubleServiceType = typeof (IService<double>);

            ServiceWithPlug<double> service =
                (ServiceWithPlug<double>) manager.GetInstance(doubleServiceType, "Plugged");
            Assert.AreEqual(typeof (double), service.Plug.PlugType);
        }

        [Test]
        public void CanCreatePluginFamilyForGenericTypeWithGenericParameter()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService<int>));
        }

        [Test]
        public void CanCreatePluginFamilyForGenericTypeWithoutGenericParameter()
        {
            PluginFamily family = new PluginFamily(typeof (IGenericService<>));
        }

        [Test]
        public void CanCreatePluginForGenericTypeWithGenericParameter()
        {
            Plugin plugin = new Plugin(typeof (GenericService<int>), "key");
        }

        [Test]
        public void CanCreatePluginForGenericTypeWithoutGenericParameter()
        {
            Plugin plugin = new Plugin(typeof (GenericService<>), "key");
        }


        [Test]
        public void CanEmitForATemplateWithTwoTemplates()
        {
            PluginFamily family = new PluginFamily(typeof (ITarget<int, string>));


            family.AddPlugin(typeof (SpecificTarget<int, string>), "specific");

            InstanceFactory factory = new InstanceFactory(family);
        }

        [Test]
        public void CanEmitInstanceBuilderForATypeWithConstructorArguments()
        {
            PluginGraph graph = new PluginGraph();
            PluginFamily family = graph.FindFamily(typeof (ComplexType<int>));
            family.AddPlugin(new Plugin(typeof (ComplexType<int>), "complex"));

            Container manager = new Container(graph);

            ConfiguredInstance instance = new ConfiguredInstance().WithConcreteKey("complex")
                .WithProperty("name").EqualTo("Jeremy")
                .WithProperty("age").EqualTo(32);

            ComplexType<int> com = manager.GetInstance<ComplexType<int>>(instance);
            Assert.AreEqual("Jeremy", com.Name);
            Assert.AreEqual(32, com.Age);
        }

        [Test]
        public void CanGetPluginFamilyFromPluginGraphWithNoParameters()
        {
            PluginGraph graph = new PluginGraph();
            graph.Assemblies.Add(Assembly.GetExecutingAssembly());
            PluginFamily family1 = graph.FindFamily(typeof (IGenericService<int>));
            PluginFamily family2 = graph.FindFamily(typeof (IGenericService<string>));
            PluginFamily family3 = graph.FindFamily(typeof (IGenericService<>));

            Assert.AreSame(graph.FindFamily(typeof (IGenericService<int>)), family1);
            Assert.AreSame(graph.FindFamily(typeof (IGenericService<string>)), family2);
            Assert.AreSame(graph.FindFamily(typeof (IGenericService<>)), family3);
        }

        [Test]
        public void CanGetPluginFamilyFromPluginGraphWithParameters()
        {
            PluginGraph graph = new PluginGraph();
            graph.Assemblies.Add(Assembly.GetExecutingAssembly());
            PluginFamily family1 = graph.FindFamily(typeof (IGenericService<int>));
            PluginFamily family2 = graph.FindFamily(typeof (IGenericService<string>));

            Assert.AreSame(graph.FindFamily(typeof (IGenericService<int>)), family1);
            Assert.AreSame(graph.FindFamily(typeof (IGenericService<string>)), family2);
        }


        [Test]
        public void CanPlugGenericConcreteClassIntoGenericInterfaceWithNoGenericParametersSpecified()
        {
            bool canPlug = TypeRules.CanBeCast(typeof (IGenericService<>), typeof (GenericService<>));
            Assert.IsTrue(canPlug);
        }


        [Test]
        public void Define_profile_with_generics_and_concrete_type()
        {
            IContainer manager = new Container(registry =>
            {
                registry.CreateProfile("1").For(typeof (IService<>)).UseConcreteType(typeof (Service<>));
                registry.CreateProfile("2").For(typeof (IService<>)).UseConcreteType(typeof (Service2<>));
            });

            manager.SetDefaultsToProfile("1");

            Assert.IsInstanceOfType(typeof (Service<string>), manager.GetInstance<IService<string>>());

            manager.SetDefaultsToProfile("2");
            Assert.IsInstanceOfType(typeof (Service2<int>), manager.GetInstance<IService<int>>());
        }

        [Test]
        public void Define_profile_with_generics_with_named_instance()
        {
            IContainer manager = new Container(registry =>
            {
                registry.AddInstanceOf(typeof (IService<>),
                                       new ConfiguredInstance(typeof (Service<>)).WithName("Service1"));
                registry.AddInstanceOf(typeof (IService<>),
                                       new ConfiguredInstance(typeof (Service2<>)).WithName("Service2"));
                registry.CreateProfile("1").For(typeof (IService<>)).UseNamedInstance("Service1");
                registry.CreateProfile("2").For(typeof (IService<>)).UseNamedInstance("Service2");
            });

            manager.SetDefaultsToProfile("1");

            Assert.IsInstanceOfType(typeof (Service<string>), manager.GetInstance<IService<string>>());

            manager.SetDefaultsToProfile("2");
            Assert.IsInstanceOfType(typeof (Service2<int>), manager.GetInstance<IService<int>>());
        }

        [Test]
        public void GenericsTypeAndProfileOrMachine()
        {
            PluginGraph pluginGraph = PluginGraph.BuildGraphFromAssembly(typeof (IService<>).Assembly);
            pluginGraph.SetDefault("1", typeof (IService<>), new ReferencedInstance("Default"));
            pluginGraph.SetDefault("2", typeof (IService<>), new ReferencedInstance("Plugged"));


            Container manager = new Container(pluginGraph);

            IPlug<string> plug = manager.GetInstance<IPlug<string>>();

            manager.SetDefaultsToProfile("1");
            Assert.IsInstanceOfType(typeof (Service<string>), manager.GetInstance(typeof (IService<string>)));

            manager.SetDefaultsToProfile("2");
            Assert.IsInstanceOfType(typeof (ServiceWithPlug<string>), manager.GetInstance(typeof (IService<string>)));

            manager.SetDefaultsToProfile("1");
            Assert.IsInstanceOfType(typeof (Service<string>), manager.GetInstance(typeof (IService<string>)));
        }


        [Test]
        public void GetGenericTypeByString()
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Type type = assem.GetType("StructureMap.Testing.ITarget`2");

            Type genericType = type.GetGenericTypeDefinition();
            Assert.AreEqual(typeof (ITarget<,>), genericType);
        }


        [Test]
        public void SmokeTestCanBeCaseWithImplementationOfANonGenericInterface()
        {
            Assert.IsTrue(GenericsPluginGraph.CanBeCast(typeof (ITarget<,>), typeof (DisposableTarget<,>)));
        }
    }


    public class ComplexType<T>
    {
        private readonly int _age;
        private readonly string _name;

        public ComplexType(string name, int age)
        {
            _name = name;
            _age = age;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Age
        {
            get { return _age; }
        }

        [ValidationMethod]
        public void Validate()
        {
            throw new ApplicationException("Break!");
        }
    }

    public interface ITarget<T, U>
    {
    }

    public class SpecificTarget<T, U> : ITarget<T, U>
    {
    }

    public class DisposableTarget<T, U> : ITarget<T, U>, IDisposable
    {
        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

    public interface ITarget2<T, U, V>
    {
    }

    public class SpecificTarget2<T, U, V> : ITarget2<T, U, V>
    {
    }

    public interface IGenericService<T>
    {
        void DoSomething(T thing);
    }

    public class GenericService<T> : IGenericService<T>
    {
        #region IGenericService<T> Members

        public void DoSomething(T thing)
        {
            throw new NotImplementedException();
        }

        #endregion

        public Type GetGenericType()
        {
            return typeof (T);
        }
    }
}