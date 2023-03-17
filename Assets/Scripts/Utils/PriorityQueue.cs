using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("PathFinder.Tests")]

namespace Utils
{
    public class PriorityQueue
    {
        internal OrderedDictionary OrderedDictionary { get; } = new();

        public int Count => OrderedDictionary.Count;

        public bool Contains(Vector2Int key)
        {
            return OrderedDictionary.Contains(key);
        }

        public void Clear()
        {
            OrderedDictionary.Clear();
        }

        /// <summary>
        ///     binary search return closed value as well
        ///     <see>
        ///         <cref>
        ///             https://learn.microsoft.com/en-us/dotnet/api/system.array.binarysearch?source=recommendations&amp;
        ///             view=net-7.0#system-array-binarysearch(system-array-system-object)
        ///         </cref>
        ///     </see>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="priority"></param>
        public void Enqueue(Vector2Int vector, float priority)
        {
            var insertPosition = OrderedDictionary.BinarySearchValues(priority);
            if (insertPosition < 0) insertPosition = ~insertPosition; // ~- invert (*-1)
            OrderedDictionary.Insert(insertPosition, vector, priority);
        }

        public Vector2Int Dequeue()
        {
            var key = OrderedDictionary.Keys.OfType<Vector2Int>().FirstOrDefault();
            OrderedDictionary.RemoveAt(0);
            return key;
        }

        public void Remove(Vector2Int neighbour)
        {
            OrderedDictionary.Remove(neighbour);
        }
    }
}