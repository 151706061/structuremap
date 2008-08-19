using NUnit.Framework;
using StructureMap.Configuration.DSL;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class InjectArrayTester : RegistryExpressions
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        #endregion

        public class Processor
        {
            private readonly IHandler[] _handlers;
            private readonly string _name;

            public Processor(IHandler[] handlers, string name)
            {
                _handlers = handlers;
                _name = name;
            }


            public IHandler[] Handlers
            {
                get { return _handlers; }
            }


            public string Name
            {
                get { return _name; }
            }
        }

        public class Processor2
        {
            private readonly IHandler[] _first;
            private readonly IHandler[] _second;


            public Processor2(IHandler[] first, IHandler[] second)
            {
                _first = first;
                _second = second;
            }


            public IHandler[] First
            {
                get { return _first; }
            }

            public IHandler[] Second
            {
                get { return _second; }
            }
        }

        public interface IHandler
        {
        }

        public class Handler1 : IHandler
        {
        }

        public class Handler2 : IHandler
        {
        }

        public class Handler3 : IHandler
        {
        }

        [Test]
        public void CanStillAddOtherPropertiesAfterTheCallToChildArray()
        {
            IContainer manager = new Container(
                registry => registry.ForRequestedType<Processor>()
                                .TheDefaultIs(
                                Instance<Processor>()
                                    .ChildArray<IHandler[]>().Contains(
                                    Instance<Handler1>(),
                                    Instance<Handler2>(),
                                    Instance<Handler3>()
                                    )
                                    .WithProperty("name").EqualTo("Jeremy")
                                ));

            var processor = manager.GetInstance<Processor>();
            Assert.AreEqual("Jeremy", processor.Name);
        }

        [Test]
        public void InjectPropertiesByName()
        {
            IContainer manager = new Container(registry => registry.ForRequestedType<Processor2>()
                                                               .TheDefaultIs(
                                                               Instance<Processor2>()
                                                                   .ChildArray<IHandler[]>("first").Contains(
                                                                   Instance<Handler1>(),
                                                                   Instance<Handler2>()
                                                                   )
                                                                   .ChildArray<IHandler[]>("second").Contains(
                                                                   Instance<Handler2>(),
                                                                   Instance<Handler3>()
                                                                   )
                                                               ));


            var processor = manager.GetInstance<Processor2>();

            Assert.IsInstanceOfType(typeof (Handler1), processor.First[0]);
            Assert.IsInstanceOfType(typeof (Handler2), processor.First[1]);
            Assert.IsInstanceOfType(typeof (Handler2), processor.Second[0]);
            Assert.IsInstanceOfType(typeof (Handler3), processor.Second[1]);
        }

        [Test,
         ExpectedException(typeof (StructureMapException),
             ExpectedMessage =
                 "StructureMap Exception Code:  307\nIn the call to ChildArray<T>(), the type T must be an array")]
        public void InjectPropertiesByNameButUseTheElementType()
        {
            var registry = new Registry();

            registry.ForRequestedType<Processor2>()
                .TheDefaultIs(
                Instance<Processor2>()
                    .ChildArray<IHandler>("first").Contains(
                    Instance<Handler1>(),
                    Instance<Handler2>()
                    )
                    .ChildArray<IHandler[]>("second").Contains(
                    Instance<Handler2>(),
                    Instance<Handler3>()
                    )
                );
        }

        [Test]
        public void PlaceMemberInArrayByReference()
        {
            IContainer manager = new Container(registry =>
            {
                registry.AddInstanceOf<IHandler>().UsingConcreteType<Handler1>().WithName("One");
                registry.AddInstanceOf<IHandler>().UsingConcreteType<Handler2>().WithName("Two");

                registry.ForRequestedType<Processor>()
                    .TheDefaultIs(
                    Instance<Processor>()
                        .WithProperty("name").EqualTo("Jeremy")
                        .ChildArray<IHandler[]>().Contains(
                        Instance("Two"),
                        Instance("One")
                        )
                    );
            });

            var processor = manager.GetInstance<Processor>();

            Assert.IsInstanceOfType(typeof (Handler2), processor.Handlers[0]);
            Assert.IsInstanceOfType(typeof (Handler1), processor.Handlers[1]);
        }


        [Test]
        public void PlaceMemberInArrayByReference_with_SmartInstance()
        {
            IContainer manager = new Container(registry =>
            {
                registry.AddInstanceOf<IHandler>().UsingConcreteType<Handler1>().WithName("One");
                registry.AddInstanceOf<IHandler>().UsingConcreteType<Handler2>().WithName("Two");

                registry.ForRequestedType<Processor>().TheDefault.Is.OfConcreteType<Processor>()
                    .WithCtorArg("name").EqualTo("Jeremy")
                    .TheArrayOf<IHandler>().Contains(x =>
                    {
                        x.References("Two");
                        x.References("One");
                    });

            });

            var processor = manager.GetInstance<Processor>();

            Assert.IsInstanceOfType(typeof(Handler2), processor.Handlers[0]);
            Assert.IsInstanceOfType(typeof(Handler1), processor.Handlers[1]);
        }

        [Test]
        public void ProgrammaticallyInjectArrayAllInline()
        {
            IContainer manager = new Container(registry => registry.ForRequestedType<Processor>()
                                                               .TheDefaultIs(
                                                               Instance<Processor>()
                                                                   .ChildArray<IHandler[]>().Contains(
                                                                   Instance<Handler1>(),
                                                                   Instance<Handler2>(),
                                                                   Instance<Handler3>()
                                                                   )
                                                                   .WithProperty("name").EqualTo("Jeremy")
                                                               ));

            var processor = manager.GetInstance<Processor>();

            Assert.IsInstanceOfType(typeof (Handler1), processor.Handlers[0]);
            Assert.IsInstanceOfType(typeof (Handler2), processor.Handlers[1]);
            Assert.IsInstanceOfType(typeof (Handler3), processor.Handlers[2]);
        }

        [Test]
        public void ProgrammaticallyInjectArrayAllInline_with_smart_instance()
        {
            IContainer container = new Container(r =>
            {
                r.ForRequestedType<Processor>().TheDefault.Is.OfConcreteType<Processor>()
                    .WithCtorArg("name").EqualTo("Jeremy")
                    .TheArrayOf<IHandler>().Contains(x =>
                    {
                        x.OfConcreteType<Handler1>();
                        x.OfConcreteType<Handler2>();
                        x.OfConcreteType<Handler3>();
                    });

                int number = 0;
            });

            var processor = container.GetInstance<Processor>();

            Assert.IsInstanceOfType(typeof(Handler1), processor.Handlers[0]);
            Assert.IsInstanceOfType(typeof(Handler2), processor.Handlers[1]);
            Assert.IsInstanceOfType(typeof(Handler3), processor.Handlers[2]);
        }

        [Test,
         ExpectedException(typeof (StructureMapException),
             ExpectedMessage =
                 "StructureMap Exception Code:  307\nIn the call to ChildArray<T>(), the type T must be an array")]
        public void TryToInjectByTheElementTypeInsteadOfTheArrayType()
        {
            var registry = new Registry();

            registry.ForRequestedType<Processor>()
                .TheDefaultIs(
                Instance<Processor>()
                    .WithProperty("name").EqualTo("Jeremy")
                    .ChildArray<IHandler>().Contains(
                    Instance<Handler1>())
                );
        }
    }
}