using System.Collections.Generic;
using System.Linq;

namespace Granite.Infrastructure.Common.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<int> To(this int start, int end)
        {
            return end >= start
                ? Enumerable.Range(start, end - start + 1)
                : Enumerable.Empty<int>();
        }
        
        public static string Join<T>(this IEnumerable<T> items,
            string delimeter = "")
        {
            if (items == null) return string.Empty;
            var itemsArray = items as T[] ?? items.ToArray();
            return !itemsArray.Any()
                ? string.Empty
                : itemsArray.Select(x => x.ToString()).Aggregate((a, i) =>
                    "{0}{1}{2}".ToFormat(a, delimeter, i));
        }
    }
}