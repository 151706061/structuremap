using System;
using System.Collections;
using System.Collections.Generic;

namespace StructureMap
{
    public interface OpenGenericTypeSpecificationExpression
    {
        T As<T>();
    }

    public interface OpenGenericTypeListSpecificationExpression
    {
        IList<T> As<T>();
    }

    public class CloseGenericTypeExpression : OpenGenericTypeSpecificationExpression, OpenGenericTypeListSpecificationExpression
    {
        private readonly object _subject;
        private readonly IContainer _container;
        private Type _pluginType;

        public CloseGenericTypeExpression(object subject, IContainer container)
        {
            _subject = subject;
            _container = container;
        }

        /// <summary>
        /// Specify the open generic type that should have a single generic parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public OpenGenericTypeSpecificationExpression GetClosedTypeOf(Type type)
        {
            closeType(type);
            return this;
        }

        private void closeType(Type type)
        {
            if (!type.IsGeneric())
            {
                throw new StructureMapException(285);
            }

            _pluginType = type.MakeGenericType(_subject.GetType());
        }

        /// <summary>
        /// specify what type you'd like the service returned as
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T OpenGenericTypeSpecificationExpression.As<T>()
        {
            return (T) _container.With(_subject.GetType(), _subject).GetInstance(_pluginType);
        }

        public OpenGenericTypeListSpecificationExpression GetAllClosedTypesOf(Type type)
        {
            closeType(type);
            return this;
        }

        IList<T> OpenGenericTypeListSpecificationExpression.As<T>()
        {
            IList list = _container.With(_subject.GetType(), _subject).GetAllInstances(_pluginType);
            var returnValue = new List<T>();
            foreach (var o in list)
            {
                returnValue.Add((T) o);
            }

            return returnValue;
        }
    }
}