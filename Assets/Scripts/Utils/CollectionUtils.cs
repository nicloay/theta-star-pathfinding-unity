using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Utils
{
    public static class CollectionUtils
    {
        public static int BinarySearchValues<T>(this OrderedDictionary dictionary, T value,
            IComparer<T> comparer = null)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            comparer ??= Comparer<T>.Default;

            var lower = 0;
            var upper = dictionary.Count - 1;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = comparer.Compare(value, (T)dictionary[middle]);
                if (comparisonResult == 0)
                    return middle;
                if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~lower;
        }
    }
}