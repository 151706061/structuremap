using System;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget3;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class BuildStrategiesTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        [Test]
        public void BuildPolicy_should_apply_interception()
        {
            
            MockRepository mocks = new MockRepository();
            IBuildSession buildSession = mocks.CreateMock<IBuildSession>();

            object firstValue = "first";
            object secondValue = "second";
            StubInstance instance = new StubInstance(firstValue);
            Type thePluginType = typeof (IGateway);

            using (mocks.Record())
            {
                Expect.Call(buildSession.ApplyInterception(thePluginType, firstValue)).Return(secondValue);
            }

            using (mocks.Playback())
            {
                BuildPolicy policy = new BuildPolicy();
                object returnedObject = policy.Build(buildSession, thePluginType, instance);

                Assert.AreSame(secondValue, returnedObject);
            }
        }

        [Test]
        public void BuildPolicy_should_throw_270_if_interception_fails()
        {
            try
            {
                LiteralInstance instance = new LiteralInstance("something")
                    .OnCreation<object>(delegate(object o){throw new NotImplementedException();});

                BuildPolicy policy = new BuildPolicy();
                policy.Build(new StubBuildSession(), typeof (string), instance);
            }
            catch (StructureMapException e)
            {
                Assert.AreEqual(270, e.ErrorCode);
            }
        }


        [Test]
        public void Singleton_build_policy()
        {
            SingletonPolicy policy = new SingletonPolicy();
            ConstructorInstance instance1 = new ConstructorInstance(delegate { return new ColorService("Red"); }).WithName("Red");
            ConstructorInstance instance2 = new ConstructorInstance(delegate { return new ColorService("Green"); }).WithName("Green");

            ColorService red1 = (ColorService) policy.Build(new StubBuildSession(), null, instance1);
            ColorService green1 = (ColorService)policy.Build(new StubBuildSession(), null, instance2);
            ColorService red2 = (ColorService)policy.Build(new StubBuildSession(), null, instance1);
            ColorService green2 = (ColorService)policy.Build(new StubBuildSession(), null, instance2);
            ColorService red3 = (ColorService)policy.Build(new StubBuildSession(), null, instance1);
            ColorService green3 = (ColorService)policy.Build(new StubBuildSession(), null, instance2);
        
            Assert.AreSame(red1, red2);
            Assert.AreSame(red1, red3);
            Assert.AreSame(green1, green2);
            Assert.AreSame(green1, green3);
        }

        [Test]
        public void CloneSingleton()
        {
            SingletonPolicy policy = new SingletonPolicy();

            SingletonPolicy clone = (SingletonPolicy) policy.Clone();
            Assert.AreNotSame(policy, clone);
            Assert.IsInstanceOfType(typeof(BuildPolicy), clone.InnerPolicy);
        }

        [Test]
        public void CloneHybrid()
        {
            HybridBuildPolicy policy = new HybridBuildPolicy();

            HybridBuildPolicy clone = (HybridBuildPolicy)policy.Clone();
            Assert.AreNotSame(policy, clone);
            Assert.IsInstanceOfType(typeof(BuildPolicy), clone.InnerPolicy);
        }


        public class StubInstance : Instance
        {
            private object _constructedObject;


            public StubInstance(object constructedObject)
            {
                _constructedObject = constructedObject;
            }

            protected override object build(Type pluginType, IBuildSession session)
            {
                return _constructedObject;
            }

            protected override string getDescription()
            {
                return "Stubbed";
            }
        }
    }
}