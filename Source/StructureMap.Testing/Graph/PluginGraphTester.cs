using System;
using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Source;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class PluginGraphTester
    {
        #region Setup/Teardown

        [TearDown]
        public void TearDown()
        {
            ObjectMother.Reset();
        }

        #endregion

        [Test]
        public void FindPluginFamilies()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");

            graph.FindFamily(typeof (IWidget)).DefaultInstanceKey = "Blue";
            graph.FindFamily(typeof (WidgetMaker));

            graph.Seal();


            foreach (PluginFamily family in graph.PluginFamilies)
            {
                Console.WriteLine(family.PluginType.AssemblyQualifiedName);
            }

            Assert.AreEqual(5, graph.FamilyCount);
        }

        [Test]
        public void FindPlugins()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");
            graph.Assemblies.Add("StructureMap.Testing.Widget2");
            graph.FindFamily(typeof (Rule));

            graph.Seal();

            PluginFamily family = graph.FindFamily(typeof (Rule));
            Assert.IsNotNull(family);
            Assert.AreEqual(5, family.Plugins.Count, "There are 5 Rule classes in the two assemblies");
        }


        [Test]
        public void PicksUpManuallyAddedPlugin()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");
            graph.FindFamily(typeof (IWidget)).DefaultInstanceKey = "Blue";
            

            PluginFamily family = graph.FindFamily(typeof (IWidget));
            family.AddPlugin(typeof (NotPluggableWidget), "NotPluggable");

            graph.Seal();


            Assert.IsNotNull(family);

            Assert.AreEqual(
                5,
                family.Plugins.Count,
                "5 different IWidget classes are marked as Pluggable, + the manual add");
        }

        [Test]
        public void PutsRightNumberOfPluginsIntoAFamily()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");
            graph.FindFamily(typeof (IWidget)).DefaultInstanceKey = "Blue";
            graph.Seal();

            PluginFamily family = graph.FindFamily(typeof (IWidget));
            Assert.IsNotNull(family);

            Assert.AreEqual("Blue", family.DefaultInstanceKey);

            Assert.AreEqual(4, family.Plugins.Count, "3 different IWidget classes are marked as Pluggable");
        }


    }

    [PluginFamily]
    public interface IThingy
    {
        void Go();
    }

    [Pluggable("Big")]
    public class BigThingy : IThingy
    {
        #region IThingy Members

        public void Go()
        {
        }

        #endregion
    }
}