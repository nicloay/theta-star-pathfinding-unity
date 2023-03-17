using Priority_Queue;

namespace Pathfinding
{
    public class Vector2IntNode : FastPriorityQueueNode
    {
        public readonly (int, int) Vector;

        public Vector2IntNode((int, int) vector)
        {
            Vector = vector;
        }
    }
}