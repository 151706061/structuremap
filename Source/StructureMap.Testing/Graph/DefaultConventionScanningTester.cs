using System;
using System.Diagnostics;
using NUnit.Framework;
using StructureMap.Graph;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class DefaultConventionScanningTester
    {
        [Test]
        public void FindPluginType()
        {
            Assert.AreEqual(typeof (IConvention), new DefaultConventionScanner().FindPluginType(typeof (Convention)));
            Assert.IsNull(new DefaultConventionScanner().FindPluginType(GetType()));
        }


        [Test]
        public void Process_to_Container()
        {
            var container = new Container(registry =>
            {
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.With<DefaultConventionScanner>();
                });
            });

            container.GetInstance<IConvention>().ShouldBeOfType<Convention>();
        }


        [Test]
        public void Process_to_Container_2()
        {
            var container = new Container(registry =>
            {
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.With(new DefaultConventionScanner());
                });
            });

            Assert.IsInstanceOfType(typeof (Convention), container.GetInstance<IConvention>());
        }

        [Test]
        public void Process_to_PluginGraph()
        {
            var graph = new PluginGraph();
            var scanner = new DefaultConventionScanner();
            scanner.Process(typeof (Convention), graph);

            Assert.IsFalse(graph.PluginFamilies.Contains(typeof (IServer)));
            Assert.IsTrue(graph.PluginFamilies.Contains(typeof (IConvention)));

            PluginFamily family = graph.FindFamily(typeof (IConvention));
            family.Seal();
            Assert.AreEqual(1, family.InstanceCount);
        }
    }

    public interface IConvention
    {
    }

    public interface IServer
    {
    }

    public class Convention : IConvention, IServer
    {
        public void Go()
        {
            throw new NotImplementedException();
        }
    }

    public class Something : IConvention
    {
    }
}