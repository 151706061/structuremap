using System;
using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;
using StructureMap.Testing.Widget;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class DeepInstanceTester : RegistryExpressions
    {
        private readonly Thing _prototype = new Thing(4, "Jeremy", .333, new WidgetRule(new ColorWidget("yellow")));

        private void assertThingMatches(Action<ConfigurationExpression> action)
        {
            IContainer manager = new Container(action);
            Thing actual = manager.GetInstance<Thing>();
            Assert.AreEqual(_prototype, actual);
        }

        [Test]
        public void DeepInstance2()
        {
            assertThingMatches(registry =>
            {
                registry.BuildInstancesOf<IWidget>().TheDefaultIs(
                    Instance<ColorWidget>()
                        .WithProperty("color").EqualTo("yellow")
                    );

                registry.BuildInstancesOf<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                registry.BuildInstancesOf<Thing>().TheDefaultIs(
                    Instance<Thing>()
                        .WithProperty("average").EqualTo(.333)
                        .WithProperty("name").EqualTo("Jeremy")
                        .WithProperty("count").EqualTo(4)
                    );
            });
        }

        [Test]
        public void DeepInstance3()
        {
            assertThingMatches(registry =>
            {
                registry.BuildInstancesOf<IWidget>().TheDefaultIs(
                    Object<IWidget>(new ColorWidget("yellow"))
                    );

                registry.BuildInstancesOf<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                registry.BuildInstancesOf<Thing>().TheDefaultIs(
                    Instance<Thing>()
                        .WithProperty("average").EqualTo(.333)
                        .WithProperty("name").EqualTo("Jeremy")
                        .WithProperty("count").EqualTo(4)
                    );
            });
        }


        [Test]
        public void DeepInstance4()
        {
            assertThingMatches(registry =>
            {
                registry.BuildInstancesOf<IWidget>().TheDefaultIs(
                    Prototype<IWidget>(new ColorWidget("yellow"))
                    );

                registry.BuildInstancesOf<Rule>().TheDefaultIsConcreteType<WidgetRule>();

                registry.BuildInstancesOf<Thing>().TheDefaultIs(
                    Instance<Thing>()
                        .WithProperty("average").EqualTo(.333)
                        .WithProperty("name").EqualTo("Jeremy")
                        .WithProperty("count").EqualTo(4)
                    );
            });
        }


        [Test]
        public void DeepInstance5()
        {
            assertThingMatches(registry =>
            {
                registry.AddInstanceOf<IWidget>()
                    .UsingConcreteType<ColorWidget>()
                    .WithName("Yellow")
                    .WithProperty("color").EqualTo("yellow");

                registry.AddInstanceOf<Rule>()
                    .UsingConcreteType<WidgetRule>()
                    .WithName("TheWidgetRule")
                    .Child<IWidget>().IsNamedInstance("Yellow");

                registry.BuildInstancesOf<Thing>().TheDefaultIs(
                    Instance<Thing>()
                        .WithProperty("average").EqualTo(.333)
                        .WithProperty("name").EqualTo("Jeremy")
                        .WithProperty("count").EqualTo(4)
                        .Child<Rule>().IsNamedInstance("TheWidgetRule")
                    );
            });
        }

        [Test]
        public void DeepInstanceTest1()
        {
            ConfiguredInstance widgetExpression = Instance<ColorWidget>()
                .WithProperty("color").EqualTo("yellow");

            ConfiguredInstance ruleExpression = Instance<WidgetRule>()
                .Child<IWidget>().Is(widgetExpression);


            assertThingMatches(registry => registry.BuildInstancesOf<Thing>().TheDefaultIs(
                                               Instance<Thing>()
                                                   .WithProperty("name").EqualTo("Jeremy")
                                                   .WithProperty("count").EqualTo(4)
                                                   .WithProperty("average").EqualTo(.333)
                                                   .Child<Rule>().Is(
                                                   ruleExpression
                                                   )
                                               ));
        }


        [Test]
        public void DeepInstanceTest_with_SmartInstance()
        {
            assertThingMatches(registry =>
            {
                registry.ForRequestedType<Thing>().TheDefault.Is.OfConcreteType<Thing>()
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
            Thing thing = obj as Thing;
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