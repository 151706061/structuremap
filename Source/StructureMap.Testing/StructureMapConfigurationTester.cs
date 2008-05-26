using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using StructureMap.Configuration;
using StructureMap.Graph;
using StructureMap.Testing.GenericWidgets;

namespace StructureMap.Testing
{
    [TestFixture]
    public class StructureMapConfigurationTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            ObjectFactory.ReInitialize();
            StructureMapConfiguration.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            StructureMapConfiguration.ResetAll();
        }

        #endregion

        private static XmlNode createNodeFromText(string outerXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(outerXml);
            return document.DocumentElement;
        }

        [Test]
        public void BuildPluginGraph()
        {
            PluginGraph graph = StructureMapConfiguration.GetPluginGraph();
            Assert.IsNotNull(graph);
        }


        [Test]
        public void PullConfigurationFromTheAppConfig()
        {
            StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            StructureMapConfiguration.PullConfigurationFromAppConfig = true;

            ColorThing<string, bool> thing =
                (ColorThing<string, bool>) ObjectFactory.GetInstance<IThing<string, bool>>();
            Assert.AreEqual("Cornflower", thing.Color, "Cornflower is the color from the App.config file");
        }

        

        [Test]
        public void SettingsFromAllParentConfigFilesShouldBeIncluded()
        {
            StructureMapConfigurationSection configurationSection = new StructureMapConfigurationSection();

            XmlNode fromMachineConfig =
                createNodeFromText(@"<StructureMap><Assembly Name=""SomeAssembly""/></StructureMap>");
            XmlNode fromWebConfig =
                createNodeFromText(@"<StructureMap><Assembly Name=""AnotherAssembly""/></StructureMap>");

            IList<XmlNode> parentNodes = new List<XmlNode>();
            parentNodes.Add(fromMachineConfig);

            IList<XmlNode> effectiveConfig =
                configurationSection.Create(parentNodes, null, fromWebConfig) as IList<XmlNode>;

            Assert.IsNotNull(effectiveConfig, "A list of configuration nodes should have been returned.");
            Assert.AreEqual(2, effectiveConfig.Count, "Both configurations should have been returned.");
            Assert.AreEqual(fromMachineConfig, effectiveConfig[0]);
            Assert.AreEqual(fromWebConfig, effectiveConfig[1]);
        }

        [Test]
        public void TheDefaultNameIs_should_set_the_default_profile_name()
        {
            StructureMapConfiguration.IgnoreStructureMapConfig = true;

            string theDefaultProfileName = "the default profile";
            StructureMapConfiguration.TheDefaultProfileIs(theDefaultProfileName);

            PluginGraph graph = StructureMapConfiguration.GetPluginGraph();
            Assert.AreEqual(theDefaultProfileName, graph.ProfileManager.DefaultProfileName);
        }
    }

    public interface ISomething
    {
    }

    public class Something : ISomething
    {
        public Something()
        {
            throw new ApplicationException("You can't make me!");
        }
    }
}