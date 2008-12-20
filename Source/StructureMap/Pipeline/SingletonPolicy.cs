namespace StructureMap.Pipeline
{
    [Pluggable("Singleton")]
    public class SingletonPolicy : CacheInterceptor
    {
        private readonly object _locker = new object();
        private InstanceCache _cache;

        protected override InstanceCache findCache()
        {
            if (_cache == null)
            {
                lock (_locker)
                {
                    if (_cache == null)
                    {
                        _cache = buildNewCache();
                    }
                }
            }

            return _cache;
        }

        protected override CacheInterceptor clone()
        {
            return new SingletonPolicy();
        }
    }
}