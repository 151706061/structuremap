using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Configuration.Mementos;
using StructureMap.Graph;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class LiteralExpressionTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        [Test]
        public void BuildFromInstanceManager()
        {
            ColorWidget theWidget = new ColorWidget("Red");
            LiteralExpression<IWidget> expression = new LiteralExpression<IWidget>(theWidget);
            PluginGraph graph = new PluginGraph();
            ((IExpression) expression).Configure(graph);

            InstanceManager manager = new InstanceManager(graph);

            IWidget actualWidget = manager.CreateInstance<IWidget>(expression.InstanceKey);
            Assert.AreSame(theWidget, actualWidget);
        }

        [Test]
        public void ConfiguresALiteral()
        {
            ColorWidget theWidget = new ColorWidget("Red");
            LiteralExpression<IWidget> expression = new LiteralExpression<IWidget>(theWidget);
            PluginGraph graph = new PluginGraph();
            ((IExpression) expression).Configure(graph);

            PluginFamily family = graph.PluginFamilies[typeof (IWidget)];
            Assert.IsNotNull(family);

            LiteralMemento memento = (LiteralMemento) family.Source.GetMemento(expression.InstanceKey);
            Assert.AreSame(theWidget, memento.Build(null));
        }

        [Test]
        public void OverrideTheInstanceKey()
        {
            ColorWidget theWidget = new ColorWidget("Red");
            LiteralExpression<IWidget> expression = new LiteralExpression<IWidget>(theWidget);
            expression.WithName("Blue");
            PluginGraph graph = new PluginGraph();
            ((IExpression) expression).Configure(graph);

            PluginFamily family = graph.PluginFamilies[typeof (IWidget)];
            Assert.IsNotNull(family);

            LiteralMemento memento = (LiteralMemento) family.Source.GetMemento(expression.InstanceKey);
            Assert.AreSame(theWidget, memento.Build(null));
        }
    }
}