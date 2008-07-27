using NUnit.Framework;
using StructureMap.Graph;
using StructureMap.Source;
using StructureMap.Testing.TestData;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget5;

namespace StructureMap.Testing.Graph
{
    [TestFixture]
    public class SetterInjectionEmittingTester
    {
        private MementoSource _source;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DataMother.WriteDocument("GridColumnInstances.xml");
            DataMother.WriteDocument("FullTesting.XML");
            _source = new XmlFileMementoSource("GridColumnInstances.xml", "//GridColumns", "GridColumn");
        }


        private Container buildInstanceManager()
        {
            PluginGraph pluginGraph = DataMother.BuildPluginGraphFromXml(@"
<StructureMap>
	<Assembly Name='StructureMap.Testing.Widget'/>
	<Assembly Name='StructureMap.Testing.Widget5'/>
	
	<PluginFamily Type='StructureMap.Testing.Widget.IWidget' Assembly='StructureMap.Testing.Widget' DefaultKey='Money'>
		<Source Type='XmlFile' FilePath='FullTesting.XML' XPath='Widgets' NodeName='Widget'/>
		<Plugin Assembly='StructureMap.Testing.Widget' Type='StructureMap.Testing.Widget.NotPluggableWidget' ConcreteKey='NotPluggable'/>
	</PluginFamily>

	<PluginFamily Type='StructureMap.Testing.Widget.Rule' Assembly='StructureMap.Testing.Widget' DefaultKey='Blue'>
		<Instance Key='Bigger' Type='GreaterThan'>
			<Property Name='Attribute' Value='MyDad' />
			<Property Name='Value' Value='10' />
		</Instance>
		<Instance Key='Blue' Type='Color'>
			<Property Name='Color' Value='Blue' />
		</Instance>
		<Instance Key='Red' Type='Color'>
			<Property Name='Color' Value='Red' />
		</Instance>
	</PluginFamily>

	<PluginFamily Type='StructureMap.Testing.Widget5.IGridColumn' Assembly='StructureMap.Testing.Widget5' DefaultKey=''>
		<Source Type='XmlFile' FilePath='GridColumnInstances.XML' XPath='//GridColumns' NodeName='GridColumn'/>
		<Plugin Assembly='StructureMap.Testing.Widget5' Type='StructureMap.Testing.Widget5.OtherGridColumn' ConcreteKey='Other'>
			<Setter Name='ColumnName' />
			<Setter Name='FontStyle' />
			<Setter Name='Rules' />
			<Setter Name='Widget' />
			<Setter Name='WrapLines' />
		</Plugin>
	</PluginFamily>
	
	<Instances/>
</StructureMap>


");
                
                

            return new Container(pluginGraph);
        }

        [Test]
        public void ChildArraySetter()
        {
            Container manager = buildInstanceManager();

            WidgetArrayGridColumn column =
                (WidgetArrayGridColumn) manager.GetInstance(typeof (IGridColumn), "WidgetArray");

            Assert.AreEqual(3, column.Widgets.Length);
        }

        [Test]
        public void ChildObjectSetter()
        {
            Container manager = buildInstanceManager();


            WidgetGridColumn column = (WidgetGridColumn) manager.GetInstance(typeof (IGridColumn), "BlueWidget");
            Assert.IsTrue(column.Widget is ColorWidget);
        }

        [Test]
        public void EnumSetter()
        {
            PluginGraph graph = new PluginGraph();
            PluginFamily family = graph.FindFamily(typeof (IGridColumn));
            Plugin plugin = new Plugin(typeof (EnumGridColumn));
            family.AddPlugin(plugin);

            family.AddInstance(_source.GetMemento("Enum"));

            Container manager = new Container(graph);

            EnumGridColumn column = (EnumGridColumn) manager.GetInstance<IGridColumn>("Enum");

            Assert.AreEqual(FontStyleEnum.BodyText, column.FontStyle);
        }

        [Test]
        public void PrimitiveNonStringSetter()
        {
            PluginGraph graph = new PluginGraph();
            PluginFamily family = graph.FindFamily(typeof (IGridColumn));
            Plugin plugin = new Plugin(typeof (LongGridColumn));
            family.AddPlugin(plugin);

            InstanceMemento memento = _source.GetMemento("Long");
            long count = long.Parse(memento.GetProperty("Count"));
            family.AddInstance(memento);

            Container manager = new Container(graph);


            LongGridColumn column = (LongGridColumn) manager.GetInstance<IGridColumn>("Long");
            Assert.AreEqual(count, column.Count);
        }

        [Test]
        public void StringSetter()
        {
            PluginGraph graph = new PluginGraph();
            PluginFamily family = graph.FindFamily(typeof (IGridColumn));
            Plugin plugin = new Plugin(typeof (StringGridColumn));
            family.AddPlugin(plugin);

            InstanceMemento memento = _source.GetMemento("String");
            family.AddInstance(memento);

            Container manager = new Container(graph);
            StringGridColumn column = (StringGridColumn) manager.GetInstance<IGridColumn>("String");


            Assert.AreEqual(memento.GetProperty("Name"), column.Name);
        }
    }
}