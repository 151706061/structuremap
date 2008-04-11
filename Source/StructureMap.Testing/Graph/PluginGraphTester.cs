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

            graph.PluginFamilies.Add(typeof (IWidget), "Blue");
            graph.PluginFamilies.Add(typeof (WidgetMaker), "");

            graph.Seal();


            foreach (PluginFamily family in graph.PluginFamilies)
            {
                Console.WriteLine(family.PluginType.AssemblyQualifiedName);
            }

            Assert.AreEqual(5, graph.PluginFamilies.Count);
        }

        [Test]
        public void FindPlugins()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");
            graph.Assemblies.Add("StructureMap.Testing.Widget2");
            graph.PluginFamilies.Add(typeof (Rule), string.Empty);

            graph.Seal();

            PluginFamily family = graph.PluginFamilies[typeof (Rule)];
            Assert.IsNotNull(family);
            Assert.AreEqual(5, family.Plugins.Count, "There are 5 Rule classes in the two assemblies");
        }

        [Test]
        public void LocateOrCreateFamilyForTypeHappyPathExistingFamily()
        {
            PluginGraph pluginGraph = new PluginGraph();
            pluginGraph.Assemblies.Add("StructureMap.Testing.Widget");
            pluginGraph.Seal();

            Type type = typeof (WidgetMaker);
            string fullName = type.FullName;

            TypePath typePath = pluginGraph.LocateOrCreateFamilyForType(fullName);
            Assert.AreEqual(type, typePath.FindType());
        }

        [Test]
        public void LocateOrCreateFamilyForTypeHappyPathNewFamily()
        {
            PluginGraph pluginGraph = new PluginGraph();
            pluginGraph.Assemblies.Add("StructureMap.Testing.Widget");
            pluginGraph.Assemblies.Add("StructureMap.Testing.Widget2");
            pluginGraph.Assemblies.Add("StructureMap.Testing.Widget3");
            pluginGraph.Seal();

            Type type = typeof (IWidget);
            Assert.IsFalse(pluginGraph.PluginFamilies.Contains(type));

            TypePath typePath = pluginGraph.LocateOrCreateFamilyForType(type.FullName);
            Assert.AreEqual(type, typePath.FindType());

            Assert.IsTrue(pluginGraph.PluginFamilies.Contains(type));
        }

        [Test,
         ExpectedException(typeof (StructureMapException),
            ExpectedMessage = "StructureMap Exception Code:  300\nThe implied PluginType sometype cannot be found in any of the configured assemblies "
             )]
        public void LocateOrCreateFamilyForTypeSadPath()
        {
            PluginGraph pluginGraph = new PluginGraph();
            TypePath path = pluginGraph.LocateOrCreateFamilyForType("sometype");
        }

        [Test]
        public void PicksUpManuallyAddedPlugin()
        {
            PluginGraph graph = new PluginGraph();

            graph.Assemblies.Add("StructureMap.Testing.Widget");
            graph.PluginFamilies.Add(typeof (IWidget), "Blue");
            TypePath path =
                new TypePath("StructureMap.Testing.Widget", "StructureMap.Testing.Widget.NotPluggableWidget");


            PluginFamily family = graph.PluginFamilies[typeof (IWidget)];

            family.Plugins.Add(path, "NotPluggable");
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
            graph.PluginFamilies.Add(typeof (IWidget), "Blue");
            graph.Seal();

            PluginFamily family = graph.PluginFamilies[typeof (IWidget)];
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
            // TODO:  Add BigThingy.Go implementation
        }

        #endregion
    }
}