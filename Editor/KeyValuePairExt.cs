using System.Collections.Generic;

namespace ParallaxEditor
{
    internal static class KeyValuePairExt
    {
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
