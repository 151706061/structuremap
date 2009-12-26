using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using StructureMap.Configuration;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.TestData;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing
{
    [TestFixture]
    public class StructureMapConfigurationTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            DataMother.RestoreStructureMapConfig();
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
            var document = new XmlDocument();
            document.LoadXml(outerXml);
            return document.DocumentElement;
        }


        public static void Bootstrap()
        {
            StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            StructureMapConfiguration.PullConfigurationFromAppConfig = true;

            StructureMapConfiguration.AddRegistry(new CoreRegistry());
            StructureMapConfiguration.AddRegistry(new WebRegistry());

            StructureMapConfiguration.ForRequestedType<IGateway>().TheDefaultIsConcreteType<DefaultGateway>();

            var gateway = ObjectFactory.GetInstance<IGateway>();
        }

        public class WebRegistry : Registry
        {
        }

        public class CoreRegistry : Registry
        {
        }

        [Test]
        public void BuildPluginGraph()
        {
            PluginGraph graph = StructureMapConfiguration.GetPluginGraph();
            Assert.IsNotNull(graph);
        }

        [Test]
        public void Ignore_the_StructureMap_config_file_even_if_it_exists()
        {
            StructureMapConfiguration.IgnoreStructureMapConfig = true;
            StructureMapConfiguration.GetPluginGraph().FamilyCount.ShouldEqual(0);
        }

        [Test]
        public void PullConfigurationFromTheAppConfig()
        {
            
            ObjectFactory.Initialize(x =>
            {
                x.UseDefaultStructureMapConfigFile = false;
                x.PullConfigurationFromAppConfig = true;
            });

            ObjectFactory.GetInstance<IThing<string, bool>>()
                .IsType<ColorThing<string, bool>>().Color.ShouldEqual("Cornflower");
        }


        [Test]
        public void SettingsFromAllParentConfigFilesShouldBeIncluded()
        {
            var configurationSection = new StructureMapConfigurationSection();

            XmlNode fromMachineConfig =
                createNodeFromText(@"<StructureMap><Assembly Name=""SomeAssembly""/></StructureMap>");
            XmlNode fromWebConfig =
                createNodeFromText(@"<StructureMap><Assembly Name=""AnotherAssembly""/></StructureMap>");

            IList<XmlNode> parentNodes = new List<XmlNode>();
            parentNodes.Add(fromMachineConfig);

            var effectiveConfig =
                configurationSection.Create(parentNodes, null, fromWebConfig) as IList<XmlNode>;

            Assert.IsNotNull(effectiveConfig, "A list of configuration nodes should have been returned.");
            Assert.AreEqual(2, effectiveConfig.Count, "Both configurations should have been returned.");
            Assert.AreEqual(fromMachineConfig, effectiveConfig[0]);
            Assert.AreEqual(fromWebConfig, effectiveConfig[1]);
        }

        [Test]
        public void StructureMap_functions_without_StructureMapconfig_file_in_the_default_mode()
        {
            DataMother.RemoveStructureMapConfig();

            StructureMapConfiguration.GetPluginGraph().ShouldNotBeNull();
        }

        [Test(
            Description =
                "Guid test based on problems encountered by Paul Segaro. See http://groups.google.com/group/structuremap-users/browse_thread/thread/34ddaf549ebb14f7?hl=en"
            )]
        public void TheDefaultInstance_has_a_dependency_upon_a_Guid_NewGuid_lambda_generated_instance()
        {
            StructureMapConfiguration.IgnoreStructureMapConfig = true;

            StructureMapConfiguration.ForRequestedType<Guid>().TheDefault.Is.ConstructedBy(() => Guid.NewGuid());
            StructureMapConfiguration.ForRequestedType<IFoo>().TheDefaultIsConcreteType<Foo>();

            Assert.That(ObjectFactory.GetInstance<IFoo>().SomeGuid != Guid.Empty);
        }

        [Test(
            Description =
                "Guid test based on problems encountered by Paul Segaro. See http://groups.google.com/group/structuremap-users/browse_thread/thread/34ddaf549ebb14f7?hl=en"
            )]
        public void TheDefaultInstanceIsALambdaForGuidNewGuid()
        {
            StructureMapConfiguration.IgnoreStructureMapConfig = true;

            StructureMapConfiguration.ForRequestedType<Guid>().TheDefault.Is.ConstructedBy(() => Guid.NewGuid());

            Assert.That(ObjectFactory.GetInstance<Guid>() != Guid.Empty);
        }

        [Test]
        public void TheDefaultNameIs_should_set_the_default_profile_name()
        {
            StructureMapConfiguration.IgnoreStructureMapConfig = true;

            string theDefaultProfileName = "the default profile";
            StructureMapConfiguration.TheDefaultProfileIs(theDefaultProfileName);

            PluginGraph graph = StructureMapConfiguration.GetPluginGraph();
            graph.ProfileManager.DefaultProfileName.ShouldEqual(theDefaultProfileName);
        }

        [Test]
        public void Use_the_StructureMap_config_file_if_it_exists()
        {
            DataMother.RestoreStructureMapConfig();
            StructureMapConfiguration.GetPluginGraph().FamilyCount.ShouldBeGreaterThan(0);
        }
    }

    public interface IFoo
    {
        Guid SomeGuid { get; set; }
    }

    public class Foo : IFoo
    {
        public Foo(Guid someGuid)
        {
            SomeGuid = someGuid;
        }

        #region IFoo Members

        public Guid SomeGuid { get; set; }

        #endregion
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