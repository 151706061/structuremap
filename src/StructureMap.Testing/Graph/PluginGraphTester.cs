using System;
using System.Reflection;
using NUnit.Framework;
using StructureMap.Exceptions;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget;
using System.Linq;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class PluginGraphTester
    {
        [Test]
        public void add_type_adds_an_instance_for_type_once_and_only_once()
        {
            var graph = new PluginGraph();

            graph.AddType(typeof (IThingy), typeof (BigThingy));

            PluginFamily family = graph.FindFamily(typeof (IThingy));
            family.Instances
                  .Single()
                  .ShouldBeOfType<ConstructorInstance>()
                  .PluggedType.ShouldEqual(typeof (BigThingy));

            graph.AddType(typeof (IThingy), typeof (BigThingy));

            family.Instances.Count().ShouldEqual(1);
        }

        [Test, ExpectedException(typeof (StructureMapConfigurationException))]
        public void AssertErrors_throws_StructureMapConfigurationException_if_there_is_an_error()
        {
            var graph = new PluginGraph();
            graph.Log.RegisterError(400, new ApplicationException("Bad!"));

            graph.Log.AssertFailures();
        }

        [Test]
        public void find_family_for_concrete_type_with_default()
        {
            var graph = PluginGraph.Empty();
            graph.Families[typeof (BigThingy)].GetDefaultInstance()
                .ShouldBeOfType<ConstructorInstance>()
                .PluggedType.ShouldEqual(typeof (BigThingy));
        }
    }

    //[PluginFamily]
    public interface IThingy
    {
        void Go();
    }

    //[Pluggable("Big")]
    public class BigThingy : IThingy
    {
        #region IThingy Members

        public void Go()
        {
        }

        #endregion
    }
}