using System;
using NMock;
using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Source;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Container
{
    [TestFixture]
    public class MockingTester
    {
        private InstanceManager _manager;
        private Type gatewayType = typeof (IGateway);

        [SetUp]
        public void SetUp()
        {
            PluginGraph graph = new PluginGraph();
            graph.Assemblies.Add("StructureMap.Testing.Widget3");
            graph.PluginFamilies.Add(gatewayType, string.Empty, new MemoryMementoSource());
            graph.Seal();
            _manager = new InstanceManager(graph);
        }

    }
}