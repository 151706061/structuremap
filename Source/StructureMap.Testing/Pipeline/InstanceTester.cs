using System;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Graph;
using StructureMap.Interceptors;
using StructureMap.Pipeline;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class InstanceTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        [Test]
        public void Instance_Build_Calls_into_its_Interceptor()
        {
            MockRepository mocks = new MockRepository();
            InstanceInterceptor interceptor = mocks.CreateMock<InstanceInterceptor>();
            IBuildSession buildSession = mocks.CreateMock<IBuildSession>();


            InstanceUnderTest instanceUnderTest = new InstanceUnderTest();
            instanceUnderTest.Interceptor = interceptor;

            object objectReturnedByInterceptor = new object();
            
            using (mocks.Record())
            {
                Expect.Call(interceptor.Process(instanceUnderTest.TheInstanceThatWasBuilt)).Return(objectReturnedByInterceptor);
            }

            using (mocks.Playback())
            {
                Assert.AreEqual(objectReturnedByInterceptor, instanceUnderTest.Build(typeof (object), buildSession));
            }
        }


    }

    public class InstanceUnderTest : Instance
    {
        public object TheInstanceThatWasBuilt = new object();


        protected override object build(Type pluginType, IBuildSession session)
        {
            return TheInstanceThatWasBuilt;
        }
    }
}