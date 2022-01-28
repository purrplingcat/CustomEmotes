using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomEmotes
{
    static class Extensions
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
        {
            foreach (var pair in source)
            {
                target[pair.Key] = pair.Value;
            }
        }
    }
}
