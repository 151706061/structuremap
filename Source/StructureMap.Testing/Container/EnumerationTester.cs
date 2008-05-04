using NUnit.Framework;
using StructureMap.Configuration.Mementos;
using StructureMap.Graph;
using StructureMap.Source;
using StructureMap.Testing.Widget2;

namespace StructureMap.Testing.Container
{
    [TestFixture]
    public class EnumerationTester
    {
        public EnumerationTester()
        {
        }


        [Test]
        public void BuildClassWithEnumeration()
        {
            PluginGraph graph = new PluginGraph();


            PluginFamily family = graph.FindFamily(typeof (Cow));
            family.Plugins.Add(typeof (Cow), "Default");


            InstanceFactory cowFactory = new InstanceFactory(family, true);
            cowFactory.SetInstanceManager(new InstanceManager());

            MemoryInstanceMemento memento = new MemoryInstanceMemento("Default", "Angus");

            memento.SetProperty("Name", "Bessie");
            memento.SetProperty("Breed", "Angus");
            memento.SetProperty("Weight", "1200");

            cowFactory.AddInstance(memento.ReadInstance(graph, typeof(Cow)));

            Cow angus = cowFactory.GetInstance("Angus") as Cow;

            Assert.IsNotNull(angus);
            Assert.AreEqual("Bessie", angus.Name, "Name");
            Assert.AreEqual(BreedEnum.Angus, angus.Breed, "Breed");
            Assert.AreEqual(1200, angus.Weight, "Weight");
        }
    }
}