namespace Cedar.NEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class DictionaryExtensions
    {
        internal static IDictionary<string, string> Merge(
            this IDictionary<string, string> target,
            params IDictionary<string, string>[] others)
        {
            return target.Merge(others.AsEnumerable());
        }

        private static IDictionary<string, string> Merge(
            this IDictionary<string, string> target,
            IEnumerable<IDictionary<string, string>> others)
        {
            foreach(var pair in others.SelectMany(other => other))
            {
                target[pair.Key] = pair.Value;
            }

            return target;
        }
    }
}