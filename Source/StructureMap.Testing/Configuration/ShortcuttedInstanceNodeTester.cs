using System.Collections.Generic;
using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Testing.TestData;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Configuration
{
    [TestFixture]
    public class ShortcuttedInstanceNodeTester
    {
        private InstanceManager _manager;
        private PluginGraph _graph;

        [SetUp]
        public void SetUp()
        {
            _graph = DataMother.GetPluginGraph("ShortInstance.xml");
            _manager = new InstanceManager(_graph);
        }

        [Test]
        public void CreateTheInferredPluginCorrectly()
        {
            // Who needs the Law of Demeter?
            InstanceMemento[] mementoArray = _graph.PluginFamilies[typeof(IWidget)].Source.GetAllMementos();
            Assert.AreEqual(4, mementoArray.Length);
        }

        [Test]
        public void GetUnKeyedInstancesToo()
        {
            IList<IWidget> list = _manager.GetAllInstances<IWidget>();
            Assert.AreEqual(4, list.Count);
        }

        [Test]
        public void GetTheWidget()
        {
            ColorWidget widget = (ColorWidget)_manager.CreateInstance<IWidget>("Red");
            Assert.AreEqual("Red", widget.Color);

            ColorWidget widget2 = (ColorWidget)_manager.CreateInstance<IWidget>("Red");
            Assert.AreNotSame(widget, widget2);
        }

        [Test]
        public void GetTheRule()
        {
            ColorRule rule = (ColorRule)_manager.CreateInstance<Rule>("Blue");
            Assert.AreEqual("Blue", rule.Color);

            ColorRule rule2 = (ColorRule)_manager.CreateInstance<Rule>("Blue");
            Assert.AreSame(rule, rule2);
        }

        [Test]
        public void GetAllRules()
        {
            IList<Rule> list = _manager.GetAllInstances<Rule>();
            Assert.AreEqual(1, list.Count);
        }
    }
}
