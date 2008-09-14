using System.Xml;
using NUnit.Framework;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.TestData;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing
{
    [TestFixture]
    public class AlternativeConfigurationTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            DataMother.BackupStructureMapConfig();

            StructureMapConfiguration.ResetAll();
            DataMother.WriteDocument("Config1.xml");
            DataMother.WriteDocument("Config2.xml");
            DataMother.WriteDocument("FullTesting.XML");
        }

        [TearDown]
        public void TearDown()
        {
            StructureMapConfiguration.ResetAll();
            DataMother.RestoreStructureMapConfig();
        }

        #endregion

        public void assertTheDefault(string color)
        {
            ColorWidget widget = (ColorWidget) ObjectFactory.GetInstance<IWidget>();
            Assert.AreEqual(color, widget.Color);
        }

        [Test]
        public void AddNodeDirectly()
        {
            string xml = "<StructureMap><Assembly Name=\"StructureMap.Testing.GenericWidgets\"/></StructureMap>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);


            StructureMapConfiguration.UseDefaultStructureMapConfigFile = true;
            StructureMapConfiguration.IncludeConfigurationFromNode(doc.DocumentElement, string.Empty);

            IPlug<string> service = ObjectFactory.GetInstance<IPlug<string>>();
            Assert.IsNotNull(service);
        }

        [Test]
        public void NotTheDefault()
        {
            StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            StructureMapConfiguration.IgnoreStructureMapConfig = true;
            StructureMapConfiguration.IncludeConfigurationFromFile("Config1.xml");

            assertTheDefault("Orange");
        }

        [Test]
        public void WithTheDefault()
        {
            assertTheDefault("Red");
        }
    }
}