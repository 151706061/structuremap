using System;
using System.Collections.Generic;
using NUnit.Framework;
using StructureMap.Attributes;
using StructureMap.Configuration.DSL;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class CreatePluginFamilyTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        public interface Something
        {
        }

        public class RedSomething : Something
        {
        }

        public class GreenSomething : Something
        {
        }

        [Test]
        public void Add_an_instance_by_lambda()
        {
            StructureMap.Container manager =
                new StructureMap.Container(
                    delegate(Registry registry) { registry.ForRequestedType<IWidget>().AddInstance(delegate { return new AWidget(); }); });

            Assert.IsInstanceOfType(typeof (AWidget), manager.GetAllInstances<IWidget>()[0]);
        }

        [Test]
        public void Add_an_instance_by_literal()
        {
            AWidget aWidget = new AWidget();

            StructureMap.Container manager =
                new StructureMap.Container(
                    delegate(Registry registry) { registry.ForRequestedType<IWidget>().AddInstance(aWidget); });

            Assert.IsInstanceOfType(typeof (AWidget), manager.GetAllInstances<IWidget>()[0]);
        }

        [Test]
        public void AddInstanceByNameOnlyAddsOneInstanceToStructureMap()
        {
            IContainer manager = new StructureMap.Container(delegate(Registry registry)
            {
                registry.ForRequestedType<Something>().AddInstance(
    RegistryExpressions.Instance<Something>().UsingConcreteType<RedSomething>().WithName("Red")
    );
            });
            IList<Something> instances = manager.GetAllInstances<Something>();
            Assert.AreEqual(1, instances.Count);
        }

        [Test]
        public void AddInstanceWithNameOnlyAddsOneInstanceToStructureMap()
        {
            IContainer manager = new StructureMap.Container(delegate(Registry registry)
            {
                registry.AddInstanceOf<Something>().UsingConcreteType<RedSomething>().WithName("Red");
            });
            IList<Something> instances = manager.GetAllInstances<Something>();
            Assert.AreEqual(1, instances.Count);
        }

        [Test]
        public void AsAnotherScope()
        {
            Registry registry = new Registry();
            CreatePluginFamilyExpression<IGateway> expression =
                registry.BuildInstancesOf<IGateway>().CacheBy(InstanceScope.ThreadLocal);
            Assert.IsNotNull(expression);

            PluginGraph pluginGraph = registry.Build();

            PluginFamily family = pluginGraph.FindFamily(typeof (IGateway));
            Assert.IsInstanceOfType(typeof (ThreadLocalStoragePolicy), family.Policy);
        }

        [Test]
        public void BuildInstancesOfType()
        {
            Registry registry = new Registry();
            registry.BuildInstancesOf<IGateway>();
            PluginGraph pluginGraph = registry.Build();

            Assert.IsTrue(pluginGraph.ContainsFamily(typeof (IGateway)));
        }


        [Test]
        public void BuildPluginFamilyAsPerRequest()
        {
            Registry registry = new Registry();
            CreatePluginFamilyExpression<IGateway> expression =
                registry.BuildInstancesOf<IGateway>();
            Assert.IsNotNull(expression);

            PluginGraph pluginGraph = registry.Build();

            PluginFamily family = pluginGraph.FindFamily(typeof (IGateway));
            Assert.IsInstanceOfType(typeof (BuildPolicy), family.Policy);
        }

        [Test]
        public void BuildPluginFamilyAsSingleton()
        {
            Registry registry = new Registry();
            CreatePluginFamilyExpression<IGateway> expression =
                registry.BuildInstancesOf<IGateway>().AsSingletons();
            Assert.IsNotNull(expression);

            PluginGraph pluginGraph = registry.Build();
            PluginFamily family = pluginGraph.FindFamily(typeof (IGateway));
            Assert.IsInstanceOfType(typeof (SingletonPolicy), family.Policy);
        }

        [Test]
        public void CanOverrideTheDefaultInstance1()
        {
            Registry registry = new Registry();
            // Specify the default implementation for an interface
            registry.BuildInstancesOf<IGateway>().TheDefaultIsConcreteType<StubbedGateway>();

            PluginGraph pluginGraph = registry.Build();
            Assert.IsTrue(pluginGraph.ContainsFamily(typeof (IGateway)));

            StructureMap.Container manager = new StructureMap.Container(pluginGraph);
            IGateway gateway = (IGateway) manager.GetInstance(typeof (IGateway));

            Assert.IsInstanceOfType(typeof (StubbedGateway), gateway);
        }

        [Test]
        public void CanOverrideTheDefaultInstanceAndCreateAnAllNewPluginOnTheFly()
        {
            Registry registry = new Registry();
            registry.BuildInstancesOf<IGateway>().TheDefaultIsConcreteType<FakeGateway>();
            PluginGraph pluginGraph = registry.Build();

            Assert.IsTrue(pluginGraph.ContainsFamily(typeof (IGateway)));

            StructureMap.Container manager = new StructureMap.Container(pluginGraph);
            IGateway gateway = (IGateway) manager.GetInstance(typeof (IGateway));

            Assert.IsInstanceOfType(typeof (FakeGateway), gateway);
        }

        [Test]
        public void CreatePluginFamilyWithADefault()
        {
            IContainer manager = new StructureMap.Container(delegate(Registry registry)
            {
                registry.BuildInstancesOf<IWidget>().TheDefaultIs(
                    RegistryExpressions.Instance<IWidget>().UsingConcreteType<ColorWidget>().WithProperty("Color").EqualTo(
                        "Red")
                    );
            });

            ColorWidget widget = (ColorWidget) manager.GetInstance<IWidget>();
            Assert.AreEqual("Red", widget.Color);
        }

        [Test]
        public void PutAnInterceptorIntoTheInterceptionChainOfAPluginFamilyInTheDSL()
        {
            StubbedInstanceFactoryInterceptor factoryInterceptor = new StubbedInstanceFactoryInterceptor();

            Registry registry = new Registry();
            registry.BuildInstancesOf<IGateway>().InterceptConstructionWith(factoryInterceptor);

            PluginGraph pluginGraph = registry.Build();

            Assert.AreSame(pluginGraph.FindFamily(typeof (IGateway)).Policy, factoryInterceptor);
        }

        [Test]
        public void Set_the_default_by_a_lambda()
        {
            StructureMap.Container manager =
                new StructureMap.Container(
                    delegate(Registry registry) { registry.ForRequestedType<IWidget>().TheDefaultIs(delegate { return new AWidget(); }); });

            Assert.IsInstanceOfType(typeof (AWidget), manager.GetInstance<IWidget>());
        }

        [Test]
        public void Set_the_default_to_a_built_object()
        {
            AWidget aWidget = new AWidget();

            StructureMap.Container manager =
                new StructureMap.Container(
                    delegate(Registry registry) { registry.ForRequestedType<IWidget>().TheDefaultIs(aWidget); });

            Assert.AreSame(aWidget, manager.GetInstance<IWidget>());
        }

        [Test]
        public void TheDefaultInstanceIsConcreteType()
        {
            IContainer manager = new StructureMap.Container(delegate(Registry registry)
            {
                // Needs to blow up if the concrete type can't be used
                registry.BuildInstancesOf<Rule>().TheDefaultIsConcreteType<ARule>();
            });

            Assert.IsInstanceOfType(typeof (ARule), manager.GetInstance<Rule>());
        }

        [Test]
        public void TheDefaultInstanceIsPickedUpFromTheAttribute()
        {
            Registry registry = new Registry();
            registry.BuildInstancesOf<IGateway>();
            registry.ScanAssemblies().IncludeAssemblyContainingType<IGateway>();

            PluginGraph pluginGraph = registry.Build();

            Assert.IsTrue(pluginGraph.ContainsFamily(typeof (IGateway)));

            StructureMap.Container manager = new StructureMap.Container(pluginGraph);
            IGateway gateway = (IGateway) manager.GetInstance(typeof (IGateway));

            Assert.IsInstanceOfType(typeof (DefaultGateway), gateway);
        }
    }

    public class StubbedInstanceFactoryInterceptor : IBuildInterceptor
    {
        #region IBuildInterceptor Members

        public IBuildPolicy InnerPolicy
        {
            get { throw new NotImplementedException(); }
            set { }
        }

        public object Build(IBuildSession buildSession, Type pluginType, Instance instance)
        {
            throw new NotImplementedException();
        }

        public IBuildPolicy Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}