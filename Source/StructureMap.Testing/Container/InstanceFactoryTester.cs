using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Pipeline;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget2;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Container
{
    [TestFixture]
    public class InstanceFactoryTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            PluginGraph graph = new PluginGraph();
            Registry registry = new Registry(graph);
            registry.BuildInstancesOf<Rule>();
            registry.ScanAssemblies()
                .IncludeAssembly("StructureMap.Testing.Widget")
                .IncludeAssembly("StructureMap.Testing.Widget2");

            registry.Build();

            PipelineGraph pipelineGraph = new PipelineGraph(graph);
            _session = new BuildSession(pipelineGraph, graph.InterceptorLibrary);

            _manager = registry.BuildInstanceManager();

            
        }

        #endregion

        private IInstanceManager _manager;
        private BuildSession _session;






        [Test, ExpectedException(typeof (StructureMapException))]
        public void GetInstanceWithInvalidInstanceKey()
        {
            _manager.CreateInstance<Rule>("NonExistentRule");
        }

        [Test]
        public void CanMakeAClassWithNoConstructorParametersWithoutADefinedMemento()
        {
            Registry registry = new Registry();
            registry.ScanAssemblies().IncludeAssembly("StructureMap.Testing.Widget3");
            registry.BuildInstancesOf<IGateway>();

            PluginGraph graph = registry.Build();
            PipelineGraph pipelineGraph = new PipelineGraph(graph);

            BuildSession session = new BuildSession(graph);


            DefaultGateway gateway = 
                (DefaultGateway) session.CreateInstance(typeof (IGateway), "Default");

            Assert.IsNotNull(gateway);
        }





    }
}