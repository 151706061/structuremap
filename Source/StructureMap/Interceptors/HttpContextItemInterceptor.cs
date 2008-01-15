using System.Web;

namespace StructureMap.Interceptors
{
    [Pluggable("HttpContext")]
    public class HttpContextItemInterceptor : CacheInterceptor
    {
        public HttpContextItemInterceptor() : base()
        {
        }

        public static bool HasContext()
        {
            return HttpContext.Current != null;
        }

        private string getKey(string instanceKey)
        {
            return string.Format("{0}:{1}", InnerInstanceFactory.PluginType.AssemblyQualifiedName, instanceKey);
        }

        protected override void cache(string instanceKey, object instance)
        {
            string key = getKey(instanceKey);
            HttpContext.Current.Items.Add(key, instance);
        }

        protected override bool isCached(string instanceKey)
        {
            string key = getKey(instanceKey);
            return HttpContext.Current.Items.Contains(key);
        }

        protected override object getInstance(string instanceKey)
        {
            string key = getKey(instanceKey);
            return HttpContext.Current.Items[key];
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}