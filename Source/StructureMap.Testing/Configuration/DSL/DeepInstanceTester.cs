using System;
using NUnit.Framework;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class DeepInstanceTester
    {
        private readonly Thing _prototype = new Thing(4, "Jeremy", .333, new WidgetRule(new ColorWidget("yellow")));

        private void assertThingMatches(Action<ConfigurationExpression> action)
        {
            IContainer manager = new Container(action);
            var actual = manager.GetInstance<Thing>();
            Assert.AreEqual(_prototype, actual);
        }

        [Test]
        public void DeepInstance2()
        {
            assertThingMatches(r =>
            {
                r.For<IWidget>().TheDefault.Is.OfConcreteType<ColorWidget>()
                    .WithProperty("color").EqualTo("yellow");

                r.For<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                r.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithCtorArg("average").EqualTo(.333)
                    .WithCtorArg("name").EqualTo("Jeremy")
                    .WithCtorArg("count").EqualTo(4);
            });
        }

        [Test]
        public void DeepInstance3()
        {
            assertThingMatches(r =>
            {
                r.For<IWidget>().TheDefault.IsThis(new ColorWidget("yellow"));

                r.For<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                r.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithProperty("average").EqualTo(.333)
                    .WithProperty("name").EqualTo("Jeremy")
                    .WithProperty("count").EqualTo(4);
            });
        }


        [Test]
        public void DeepInstance4()
        {
            assertThingMatches(r =>
            {
                r.For<IWidget>().TheDefault.Is.PrototypeOf(new ColorWidget("yellow"));

                r.For<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                r.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithProperty("average").EqualTo(.333)
                    .WithProperty("name").EqualTo("Jeremy")
                    .WithProperty("count").EqualTo(4);
            });
        }


        [Test]
        public void DeepInstance5()
        {
            assertThingMatches(registry =>
            {
                registry.InstanceOf<IWidget>()
                    .Is.OfConcreteType<ColorWidget>()
                    .WithName("Yellow")
                    .WithProperty("color").EqualTo("yellow");

                registry.InstanceOf<Rule>()
                    .Is.OfConcreteType<WidgetRule>()
                    .WithName("TheWidgetRule")
                    .CtorDependency<IWidget>().Is(i => i.TheInstanceNamed("Yellow"));

                registry.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithCtorArg("average").EqualTo(.333)
                    .WithCtorArg("name").EqualTo("Jeremy")
                    .WithCtorArg("count").EqualTo(4)
                    .CtorDependency<Rule>().Is(i => i.TheInstanceNamed("TheWidgetRule"));
            });
        }


        [Test]
        public void DeepInstanceTest_with_SmartInstance()
        {
            assertThingMatches(registry =>
            {
                registry.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithCtorArg("name").EqualTo("Jeremy")
                    .WithCtorArg("count").EqualTo(4)
                    .WithCtorArg("average").EqualTo(.333)
                    .SetterDependency<Rule>().Is(x =>
                    {
                        x.OfConcreteType<WidgetRule>().SetterDependency<IWidget>().Is(
                            c => c.OfConcreteType<ColorWidget>().WithCtorArg("color").EqualTo("yellow"));
                    });
            });
        }

        [Test]
        public void DeepInstanceTest1()
        {
            assertThingMatches(r =>
            {
                r.For<Thing>().TheDefault.Is.OfConcreteType<Thing>()
                    .WithProperty("name").EqualTo("Jeremy")
                    .WithProperty("count").EqualTo(4)
                    .WithProperty("average").EqualTo(.333)
                    .CtorDependency<Rule>().Is(x =>
                    {
                        x.OfConcreteType<WidgetRule>()
                            .CtorDependency<IWidget>().Is(
                            w => { w.OfConcreteType<ColorWidget>().WithProperty("color").EqualTo("yellow"); });
                    });
            });
        }
    }

    public class Thing
    {
        private readonly double _average;
        private readonly int _count;
        private readonly string _name;
        private readonly Rule _rule;


        public Thing(int count, string name, double average, Rule rule)
        {
            _count = count;
            _name = name;
            _average = average;
            _rule = rule;
        }


        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            var thing = obj as Thing;
            if (thing == null) return false;
            if (_count != thing._count) return false;
            if (!Equals(_name, thing._name)) return false;
            if (_average != thing._average) return false;
            if (!Equals(_rule, thing._rule)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = _count;
            result = 29*result + (_name != null ? _name.GetHashCode() : 0);
            result = 29*result + _average.GetHashCode();
            result = 29*result + (_rule != null ? _rule.GetHashCode() : 0);
            return result;
        }
    }
}