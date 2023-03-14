using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Utils
{
    public static class CollectionUtils
    {
        public static int BinarySearchValues<T>(this OrderedDictionary dictionary, T value, IComparer<T> comparer = null)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            comparer ??= Comparer<T>.Default;

            int lower = 0;
            int upper = dictionary.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, (T)dictionary[middle]);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~lower;
        }
    }
}