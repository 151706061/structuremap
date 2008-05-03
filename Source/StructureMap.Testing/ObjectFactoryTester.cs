using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using StructureMap.Testing.TestData;
using StructureMap.Testing.Widget;
using IList=System.Collections.IList;

namespace StructureMap.Testing
{
    [TestFixture]
    public class ObjectFactoryTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _event = new ManualResetEvent(false);
            DataMother.WriteDocument("FullTesting.XML");
        }

        #endregion

        private ManualResetEvent _event;

        private void markDone()
        {
            _event.Set();
        }

        private void modifyXml(string Color)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("StructureMap.config");
            XmlNode node =
                doc.DocumentElement.SelectSingleNode("PluginFamily[@Type='StructureMap.Testing.Widget.Rule']");
            node.Attributes["DefaultKey"].Value = Color;
            doc.Save("StructureMap.config");
        }

        private void timeout()
        {
            Thread thread = new Thread(new ThreadStart(signal));
            thread.Start();
        }

        private void signal()
        {
            Thread.Sleep(500);
            _event.Set();
        }

        [Test]
        public void SmokeTestGetAllInstances()
        {
            IList list = ObjectFactory.GetAllInstances(typeof (GrandChild));
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void SmokeTestGetAllInstancesGeneric()
        {
            IList<GrandChild> list = ObjectFactory.GetAllInstances<GrandChild>();
            Assert.IsTrue(list.Count > 0);
        }

        [Test, Ignore("Gonna redo this")]
        public void SmokeTestWhatDoIHave()
        {
            string message = ObjectFactory.WhatDoIHave();
            Debug.WriteLine(message);
        }

        [Test, Ignore("Come back to this.")]
        public void TestTheRefresh()
        {
            ColorRule rule1 = (ColorRule) ObjectFactory.GetInstance(typeof (Rule));
            Assert.AreEqual("Blue", rule1.Color, "Starts with Blue");

            ObjectFactory.Refresh += new Notify(markDone);
            modifyXml("Red");
            timeout();
            _event.WaitOne();

            ColorRule rule2 = (ColorRule) ObjectFactory.GetInstance(typeof (Rule));
            Assert.AreEqual("Red", rule2.Color, "Changed the default color to Red");


            modifyXml("Blue");
        }

        [Test]
        public void TheDefaultProfileIsSet()
        {
            ObjectFactory.ResetDefaults();

            // The default GrandChild is set within the default profile
            GrandChild defaultGrandChild = (GrandChild) ObjectFactory.GetInstance(typeof (GrandChild));
            GrandChild todd = (GrandChild) ObjectFactory.GetNamedInstance(typeof (GrandChild), "Todd");
            Assert.AreEqual(todd.BirthYear, defaultGrandChild.BirthYear);
        }

        [Test]
        public void TheDefaultProfileIsSetWhileUsingGenericGetInstance()
        {
            ObjectFactory.ResetDefaults();

            // The default GrandChild is set within the default profile
            GrandChild todd = ObjectFactory.GetNamedInstance<GrandChild>("Todd");
            GrandChild defaultGrandChild = ObjectFactory.GetInstance<GrandChild>();

            Assert.AreEqual(todd.BirthYear, defaultGrandChild.BirthYear);
        }
    }
}