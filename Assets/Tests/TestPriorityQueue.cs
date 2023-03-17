using NUnit.Framework;
using UnityEngine;
using Utils;

namespace Tests
{
    public class TestPriorityQueue
    {
        private static readonly Vector2Int V1 = Vector2Int.down;
        private static readonly Vector2Int V2 = Vector2Int.up;
        private static readonly Vector2Int V3 = Vector2Int.right;
        private static readonly Vector2Int V4 = Vector2Int.left;

        [Test]
        public void AllInOne()
        {
            var q = new PriorityQueue();
            q.Enqueue(V1, 0.5f);
            q.Enqueue(V2, 1.5f);
            q.Enqueue(V3, 5.5f);
            q.Enqueue(V4, 3.5f);
            Assert.That(q.OrderedDictionary.Keys, Is.EquivalentTo(new[] { V1, V2, V4, V3 }));
            Assert.That(q.OrderedDictionary.Values, Is.EquivalentTo(new[] { 0.5, 1.5, 3.5, 5.5 }));

            q.Dequeue();
            Assert.That(q.OrderedDictionary.Keys, Is.EquivalentTo(new[] { V2, V4, V3 }));
            Assert.That(q.OrderedDictionary.Values, Is.EquivalentTo(new[] { 1.5, 3.5, 5.5 }));
            Assert.That(q.Contains(V4), Is.True);
            Assert.That(q.Contains(V1), Is.False);

            q.Clear();
            q.Enqueue(V4, 0.2f);
            q.Enqueue(V3, 0.1f);
            Assert.That(q.OrderedDictionary.Keys, Is.EquivalentTo(new[] { V3, V4 }));
            Assert.That(q.OrderedDictionary.Values, Is.EquivalentTo(new[] { 0.1f, 0.2f }));

            q.Remove(V3);
            Assert.That(q.OrderedDictionary.Keys, Is.EquivalentTo(new[] { V4 }));
            Assert.That(q.OrderedDictionary.Values, Is.EquivalentTo(new[] { 0.2f }));

            q.Clear();

            q.Enqueue(V1, 0.5f);
            q.Enqueue(V2, 1.5f);
            q.Enqueue(V3, 5.5f);
            q.Enqueue(V4, 3.5f);

            q.Remove(V3);
            q.Remove(V1);
            Assert.That(q.OrderedDictionary.Keys, Is.EquivalentTo(new[] { V2, V4 }));
            Assert.That(q.OrderedDictionary.Values, Is.EquivalentTo(new[] { 1.5f, 3.5f }));
        }
    }
}