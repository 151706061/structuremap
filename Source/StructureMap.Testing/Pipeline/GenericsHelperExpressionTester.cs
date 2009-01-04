using NUnit.Framework;

namespace StructureMap.Testing.Pipeline
{
    public class Address
    {
        
    }

    public class AddressDTO
    {
        
    }

    public class Continuation{}

    public class AddressFlattener : IFlattener<Address>
    {
        public object ToDto(object input)
        {
            var dto = createDTO((Address) input);
            return dto;
        }

        private object createDTO(Address input)
        {
            // creates the AddressDTO object from the 
            // Address object passed in
            throw new System.NotImplementedException();
        }
    }

    public interface IFlattener
    {
        object ToDto(object input);
    }

    public interface IFlattener<T> : IFlattener
    {
        
    }

    public class PassthroughFlattener<T> : IFlattener<T>
    {
        public object ToDto(object input)
        {
            return input;
        }
    }

    [TestFixture]
    public class when_accessing_a_type_registered_as_an_open_generics_type
    {
        private Container container;

        [SetUp]
        public void SetUp()
        {
            container = new Container(x =>
            {
                // Define the basic open type for IFlattener<>
                x.ForRequestedType(typeof (IFlattener<>)).TheDefaultIsConcreteType(typeof (PassthroughFlattener<>));
                
                // Explicitly Register a specific closed type for Address
                x.ForRequestedType<IFlattener<Address>>().TheDefaultIsConcreteType<AddressFlattener>();
            });
        }

        [Test]
        public void asking_for_a_closed_type_that_is_not_explicitly_registered_will_close_the_open_type_template()
        {
            container.GetInstance<IFlattener<Continuation>>()
                .ShouldBeOfType<PassthroughFlattener<Continuation>>();
        }

        [Test]
        public void asking_for_a_closed_type_that_is_explicitly_registered_returns_the_explicitly_defined_type()
        {
            container.GetInstance<IFlattener<Address>>()
                .ShouldBeOfType<AddressFlattener>();
        }

        [Test]
        public void using_the_generics_helper_expression()
        {
            IFlattener flattener1 = container.ForGenericType(typeof (IFlattener<>))
                .WithParameters(typeof (Address)).GetInstanceAs<IFlattener>();
            flattener1.ShouldBeOfType<AddressFlattener>();

            IFlattener flattener2 = container.ForGenericType(typeof(IFlattener<>))
                .WithParameters(typeof(Continuation)).GetInstanceAs<IFlattener>();
            flattener2.ShouldBeOfType<PassthroughFlattener<Continuation>>();
        }

        [Test]
        public void throws_exception_if_passed_a_type_that_is_not_an_open_generic_type()
        {
            try
            {
                container.ForGenericType(typeof (string)).WithParameters().GetInstanceAs<IFlattener>();
                Assert.Fail("Should have thrown exception");
            }
            catch (StructureMapException ex)
            {
                ex.ErrorCode.ShouldEqual(285);
            }
        }
    }

    public class ObjectFlattener
    {
        private readonly IContainer _container;

        // You can inject the IContainer itself into an object by the way...
        public ObjectFlattener(IContainer container)
        {
            _container = container;
        }

        // This method can "flatten" any object
        public object Flatten(object input)
        {
            var flattener = _container.ForGenericType(typeof (IFlattener<>))
                .WithParameters(input.GetType())
                .GetInstanceAs<IFlattener>();

            return flattener.ToDto(input);
        }
    }

    public class FindAddressController
    {
        public Address FindAddress(long id)
        {
            return null;
        }

        public Continuation WhatShouldTheUserDoNext()
        {
            return null;
        }
    }
}