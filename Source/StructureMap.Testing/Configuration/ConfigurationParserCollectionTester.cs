using System;
using System.Xml;
using NUnit.Framework;
using StructureMap.Configuration;
using StructureMap.Testing.TestData;

namespace StructureMap.Testing.Configuration
{
    [TestFixture]
    public class ConfigurationParserCollectionTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _collection = new ConfigurationParserCollection();
            DataMother.BackupStructureMapConfig();
        }

        [TearDown]
        public void TearDown()
        {
            DataMother.RestoreStructureMapConfig();
        }

        #endregion

        private ConfigurationParserCollection _collection;


        public void assertParserIdList(params string[] expected)
        {
            Array.Sort(expected);
            ConfigurationParser[] parsers = _collection.GetParsers();
            Converter<ConfigurationParser, string> converter =
                delegate(ConfigurationParser parser) { return parser.Id; };

            string[] actuals = Array.ConvertAll<ConfigurationParser, string>(parsers, converter);
            Array.Sort(actuals);

            Assert.AreEqual(expected, actuals);
        }

        [Test]
        public void DoNotUseDefaultAndUseADifferentFile()
        {
            DataMother.RemoveStructureMapConfig();

            _collection.UseAndEnforceExistenceOfDefaultFile = false;
            _collection.IgnoreDefaultFile = true;

            DataMother.WriteDocument("GenericsTesting.xml");

            _collection.IncludeFile("GenericsTesting.xml");
            assertParserIdList("Generics");
        }

        [Test,
         ExpectedException(typeof (StructureMapException),
            ExpectedMessage = "StructureMap Exception Code:  100\nExpected file \"StructureMap.config\" cannot be opened at DoesNotExist.xml"
             )]
        public void FileDoesNotExist()
        {
            _collection.UseAndEnforceExistenceOfDefaultFile = false;
            _collection.IgnoreDefaultFile = true;
            _collection.IncludeFile("DoesNotExist.xml");
            _collection.GetParsers();
        }

        [Test]
        public void GetIncludes()
        {
            DataMother.RemoveStructureMapConfig();

            DataMother.WriteDocument("Include1.xml");
            DataMother.WriteDocument("Include2.xml");
            DataMother.WriteDocument("Master.xml");

            _collection.UseAndEnforceExistenceOfDefaultFile = false;
            _collection.IgnoreDefaultFile = true;
            _collection.IncludeFile("Master.xml");

            assertParserIdList("Include1", "Include2", "Master");
        }

        [Test]
        public void GetMultiples()
        {
            DataMother.WriteDocument("Include1.xml");
            DataMother.WriteDocument("Include2.xml");
            DataMother.WriteDocument("Master.xml");

            DataMother.WriteDocument("GenericsTesting.xml");

            _collection.IncludeFile("GenericsTesting.xml");
            _collection.UseAndEnforceExistenceOfDefaultFile = true;
            _collection.IncludeFile("Master.xml");

            assertParserIdList("Generics", "Include1", "Include2", "Main", "Master");
        }

        [Test]
        public void GetXmlFromSomewhereElse()
        {
            DataMother.RemoveStructureMapConfig();

            string xml = "<StructureMap Id=\"Somewhere\"/>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            FetchNodeDelegate fetcher = delegate { return doc.DocumentElement; };

            _collection.IncludeNode(fetcher);
            _collection.UseAndEnforceExistenceOfDefaultFile = false;
            _collection.IgnoreDefaultFile = true;

            assertParserIdList("Somewhere");
        }

        [Test]
        public void SimpleDefaultConfigurationParser()
        {
            _collection.UseAndEnforceExistenceOfDefaultFile = true;
            assertParserIdList("Main");
        }

        [Test]
        public void UseDefaultIsTrueUponConstruction()
        {
            Assert.IsFalse(_collection.UseAndEnforceExistenceOfDefaultFile);
        }
    }
}