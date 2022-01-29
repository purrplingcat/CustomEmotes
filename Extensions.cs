using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomEmotes
{
    static class Extensions
    {
        public static IEnumerable<TSource> Deduplicate<TSource>(this IEnumerable<TSource> source, Func<TSource, object> keySelector)
        {
            var dict = new Dictionary<object, TSource>();

            foreach (var item in source)
            {
                dict[keySelector(item)] = item;
            }

            return dict.Values;
        }
    }
}
