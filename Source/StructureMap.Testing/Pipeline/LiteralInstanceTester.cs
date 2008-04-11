using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Pipeline;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class LiteralInstanceTester
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Build_happy_path()
        {
            ATarget target = new ATarget();
            LiteralInstance instance = new LiteralInstance(target);
            Assert.AreSame(target, instance.Build(typeof(ITarget), new StubInstanceCreator()));
        }

        public interface ITarget
        {
            
        }

        public class ATarget : ITarget
        {
            public override string ToString()
            {
                return "the description of ATarget";
            }
        }
    }
}
