using System.Collections.Generic;

namespace NoParamlessCtor.SourceGenerator.Helpers
{
    public static class CollectionHelpers
    {
        // Unfortunately we are stuck on ancient runtime...
        public static bool TryAdd<T, F>(
            this Dictionary<T, F> dictionary,
            T key,
            F value,
            out F? existingValue)
        {
            var exists = dictionary.TryGetValue(key, out existingValue);
            
            if (!exists)
            {
                dictionary.Add(key, value);
            }

            return !exists;
        }
    }
}