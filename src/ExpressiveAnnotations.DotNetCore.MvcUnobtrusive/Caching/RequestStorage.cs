using System.Collections.Generic;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Caching
{
    public class RequestStorage
    {
        private readonly Dictionary<string, object> Items = new Dictionary<string, object>();

        internal T Get<T>(string key) => Items.ContainsKey(key) ? (T)Items[key] : default(T);

        internal void Set<T>(string key, T value) => Items[key] = value;

        internal void Remove(string key) => Items.Remove(key);
    }
}
