using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class ReferenceMementoBuilderTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        public interface Abstraction
        {
        }

        public class Concretion1 : Abstraction
        {
        }

        public class Concretion2 : Abstraction
        {
        }

        [Test]
        public void CreateMemento()
        {
            Registry registry = new Registry();
            registry.AddInstanceOf<Abstraction>().UsingConcreteType<Concretion1>().WithName("One");
            registry.AddInstanceOf<Abstraction>().UsingConcreteType<Concretion2>().WithName("Two");

            IInstanceManager manager = registry.BuildInstanceManager();
            ReferenceMementoBuilder builder1 = new ReferenceMementoBuilder("One");
            ReferenceMementoBuilder builder2 = new ReferenceMementoBuilder("Two");

            InstanceMemento memento1 = ((IMementoBuilder) builder1).BuildMemento(new PluginGraph());
            InstanceMemento memento2 = ((IMementoBuilder) builder2).BuildMemento(new PluginGraph());


            Assert.IsInstanceOfType(typeof (Concretion1), manager.CreateInstance<Abstraction>(memento1));
            Assert.IsInstanceOfType(typeof (Concretion2), manager.CreateInstance<Abstraction>(memento2));
        }
    }
}