using System;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.Pipeline;

namespace StructureMap.Testing.Pipeline
{
    [TestFixture]
    public class PrototypeInstanceTester
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Build_a_clone()
        {
            PrototypeTarget target = new PrototypeTarget("Jeremy");
            PrototypeInstance instance = new PrototypeInstance(target);

            object returnedValue = instance.Build(typeof(PrototypeTarget), new StubBuildSession());

            Assert.AreEqual(target, returnedValue);
            Assert.AreNotSame(target, returnedValue);
        }

        public class PrototypeTarget : ICloneable, IEquatable<PrototypeTarget>
        {
            private string _name;


            public PrototypeTarget(string name)
            {
                _name = name;
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }


            public bool Equals(PrototypeTarget prototypeTarget)
            {
                if (prototypeTarget == null) return false;
                return Equals(_name, prototypeTarget._name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                return Equals(obj as PrototypeTarget);
            }

            public override int GetHashCode()
            {
                return _name != null ? _name.GetHashCode() : 0;
            }

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }
    }
}
