using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public interface IPathFinder
    {
        List<Vector2Int> CalculatePath(Vector2Int start, Vector2Int end);
        bool IsPassable(Vector2Int position);
    }
}