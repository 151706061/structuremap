using System;
using System.Collections.Generic;

namespace StructureMap.Interceptors
{
    public class InterceptorLibrary
    {
        public static readonly InterceptorLibrary Empty = new InterceptorLibrary();

        private readonly Dictionary<Type, CompoundInterceptor> _analyzedInterceptors
            = new Dictionary<Type, CompoundInterceptor>();

        private readonly List<TypeInterceptor> _interceptors = new List<TypeInterceptor>();
        private readonly object _locker = new object();

        public void AddInterceptor(TypeInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
        }

        public void ImportFrom(InterceptorLibrary source)
        {
            lock (_locker)
            {
                _analyzedInterceptors.Clear();
                _interceptors.AddRange(source._interceptors);
            }
        }

        public CompoundInterceptor FindInterceptor(Type type)
        {
            if (_analyzedInterceptors.ContainsKey(type))
            {
                return _analyzedInterceptors[type];
            }

            lock (_locker)
            {
                if (!_analyzedInterceptors.ContainsKey(type))
                {
                    TypeInterceptor[] interceptorArray =
                        _interceptors.FindAll(i => i.MatchesType(type)).ToArray();
                    _analyzedInterceptors.Add(type, new CompoundInterceptor(interceptorArray));
                }
            }

            return _analyzedInterceptors[type];
        }

        public InstanceInterceptor[] FindInterceptors(Type type)
        {
            return FindInterceptor(type).Interceptors;
        }
    }
}