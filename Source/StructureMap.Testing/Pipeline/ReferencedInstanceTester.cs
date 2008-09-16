using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Testing.Graph;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class ReferencedInstanceTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        public interface IReferenced
        {
        }

        public class ConcreteReferenced : IReferenced
        {
        }

        [Test]
        public void Create_referenced_instance_happy_path()
        {
            MockRepository mocks = new MockRepository();
            BuildSession buildSession = mocks.StrictMock<BuildSession>();

            ConcreteReferenced returnedValue = new ConcreteReferenced();
            string theReferenceKey = "theReferenceKey";
            ReferencedInstance instance = new ReferencedInstance(theReferenceKey);

            using (mocks.Record())
            {
                Expect.Call(buildSession.CreateInstance(typeof (IReferenced), theReferenceKey)).Return(returnedValue);
            }

            using (mocks.Playback())
            {
                Assert.AreSame(returnedValue, instance.Build(typeof (IReferenced), buildSession));
            }
        }

        [Test]
        public void FindMaster_Instance_happy_path()
        {
            PluginFamily family = new PluginFamily(typeof (ISomething));
            LiteralInstance redInstance = new LiteralInstance(new SomethingOne()).WithName("Red");
            family.AddInstance(redInstance);
            family.AddInstance(new LiteralInstance(new SomethingOne()).WithName("Blue"));

            ReferencedInstance instance = new ReferencedInstance("Red");
            Assert.AreSame(redInstance, ((IDiagnosticInstance) instance).FindInstanceForProfile(family, null, null));
        }

        [Test]
        public void GetDescription()
        {
            string theReferenceKey = "theReferenceKey";
            ReferencedInstance instance = new ReferencedInstance(theReferenceKey);

            TestUtility.AssertDescriptionIs(instance, "\"theReferenceKey\"");
        }
    }
}