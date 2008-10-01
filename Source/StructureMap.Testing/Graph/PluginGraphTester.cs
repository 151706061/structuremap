using System;
using NUnit.Framework;
using StructureMap.Exceptions;
using StructureMap.Graph;
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

        [Test, ExpectedException(typeof (StructureMapConfigurationException))]
        public void AssertErrors_throws_StructureMapConfigurationException_if_there_is_an_error()
        {
            PluginGraph graph = new PluginGraph();
            graph.Log.RegisterError(400, new ApplicationException("Bad!"));

            graph.Log.AssertFailures();
        }

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
            Assert.AreEqual(5, family.PluginCount, "There are 5 Rule classes in the two assemblies");
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
                family.PluginCount,
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

            Assert.AreEqual(4, family.PluginCount, "3 different IWidget classes are marked as Pluggable");
        }

        [Test]
        public void Seal_does_not_throw_an_exception_if_there_are_no_errors()
        {
            PluginGraph graph = new PluginGraph();
            Assert.AreEqual(0, graph.Log.ErrorCount);

            graph.Seal();
        }

        [Test]
        public void add_type_adds_a_plugin_for_type_once_and_only_once()
        {
            var graph = new PluginGraph();

            graph.AddType(typeof (IThingy), typeof (BigThingy));

            var family = graph.FindFamily(typeof (IThingy));
            family.PluginCount.ShouldEqual(1);
            family.FindPlugin(typeof(BigThingy)).ShouldNotBeNull();

            graph.AddType(typeof(IThingy), typeof(BigThingy));

            family.PluginCount.ShouldEqual(1);
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