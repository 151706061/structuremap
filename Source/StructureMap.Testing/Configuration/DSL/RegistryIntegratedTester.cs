using System.Collections.Generic;
using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget5;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class RegistryIntegratedTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            StructureMapConfiguration.ResetAll();
        }

        #endregion

        [Test]
        public void AutomaticallyFindRegistryFromAssembly()
        {
            StructureMapConfiguration.ResetAll();
            StructureMapConfiguration.ScanAssemblies().IncludeAssemblyContainingType<RedGreenRegistry>();

            List<string> colors = new List<string>();
            foreach (IWidget widget in ObjectFactory.GetAllInstances<IWidget>())
            {
                if (!(widget is ColorWidget))
                {
                    continue;
                }

                ColorWidget color = (ColorWidget) widget;
                colors.Add(color.Color);
            }

            Assert.Contains("Red", colors);
            Assert.Contains("Green", colors);
            Assert.Contains("Yellow", colors);
            Assert.Contains("Blue", colors);
            Assert.Contains("Brown", colors);
            Assert.Contains("Black", colors);
        }


        [Test]
        public void FindRegistriesWithinPluginGraphSeal()
        {
            PluginGraph graph = new PluginGraph();
            graph.Assemblies.Add(typeof (RedGreenRegistry).Assembly);
            graph.Seal();

            List<string> colors = new List<string>();
            PluginFamily family = graph.FindFamily(typeof (IWidget));
            family.EachInstance(instance => colors.Add(instance.Name));

            Assert.Contains("Red", colors);
            Assert.Contains("Green", colors);
            Assert.Contains("Yellow", colors);
            Assert.Contains("Blue", colors);
            Assert.Contains("Brown", colors);
            Assert.Contains("Black", colors);
        }
    }
}