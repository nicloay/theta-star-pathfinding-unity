using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Priority_Queue;
using UnityEngine;

namespace Pathfinding
{
    public class ThetaStarOptimised : IPathFinder
    {
        private static readonly (int, int) UNKNOWN = new(-1, -1);
        private readonly HashSet<(int, int)> _closedQueue; // OPTIMISATION4 - see below
        private readonly float[,] _gScore; // [x,y]
        private readonly int _height;


        private readonly bool[,] _map;

        //private readonly Dictionary<Vector2Int, Vector2IntNode> _nodeByVector = new(); 
        private readonly Dictionary<(int, int), Vector2IntNode>
            _nodeByVector =
                new(new TupleComparer()); // OPTIMISATION4: use int instead of vector to reduce hash calculation at dict

        private readonly FastPriorityQueue<Vector2IntNode>
            _openQueue; // OPTIMISATION1 - replace priorityqueue with fast one

        private readonly (int, int)[,] _parent; // [x,y]
        private readonly int _width;
        private (int, int) _end;

        public ThetaStarOptimised(bool[,] map)
        {
            _map = map;
            _width = map.GetLength(0);
            _height = map.GetLength(1);
            _gScore = new float[_width, _height];
            _parent = new (int, int) [_width, _height];
            _openQueue = new FastPriorityQueue<Vector2IntNode>(10000);
            _closedQueue = new HashSet<(int, int)>(new TupleComparer());
        }

        /// <summary>
        ///     Theta star pathfinding implementation,
        ///     <see>
        ///         <cref>https://en.wikipedia.org/wiki/Theta*</cref>
        ///     </see>
        /// </summary>
        public List<Vector2Int> CalculatePath(Vector2Int start, Vector2Int end)
        {
            return CalculatePath((start.x, start.y), (end.x, end.y));
        }

        private void ResetCache()
        {
            Array.Clear(_gScore, 0, _gScore.Length);
            Array.Clear(_parent, 0, _parent.Length);
            _openQueue.Clear();
            _closedQueue.Clear();
            _nodeByVector.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // OPTIMISATION2 - use inlining in most of places
        private void Enqueue((int, int) vector, float priority)
        {
            var node = new Vector2IntNode(vector);
            _openQueue.Enqueue(node, priority);
            _nodeByVector.Add(vector, node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveIfExist((int, int) vector)
        {
            if (_nodeByVector.TryGetValue(vector, out var node))
            {
                _openQueue.Remove(node);
                _nodeByVector.Remove(vector);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetMagnitude((int, int) from, (int, int) to)
        {
            var dsx = from.Item1 - to.Item1;
            var dsy = from.Item2 - to.Item2;
            return Mathf.Sqrt(dsx * dsx + dsy * dsy);
        }

        public List<Vector2Int> CalculatePath((int, int) start, (int, int) end)
        {
            _end = end;
            ResetCache();

            _gScore[start.Item1, start.Item2] = 0;
            _parent[start.Item1, start.Item2] = start;

            _openQueue.Enqueue(new Vector2IntNode(start), _gScore[start.Item1, start.Item2] + GetMagnitude(start, end));

            while (_openQueue.Count > 0)
            {
                var s = _openQueue.Dequeue().Vector;
                _nodeByVector.Remove(s);
                if (s == end) return ReconstructPath(s);

                _closedQueue.Add(s);
                foreach (var (x, y) in GetNeighbours(s))
                {
                    if (x < 0 || y < 0 || x >= _width || y >= _height ||
                        !_map[x, y] ||
                        _closedQueue.Contains((x, y))) continue;


                    if (!_nodeByVector.ContainsKey((x, y)))
                    {
                        _gScore[x, y] = float.PositiveInfinity;
                        _parent[x, y] = UNKNOWN;
                    }

                    UpdateVertex(s, (x, y));
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateVertex((int, int) s, (int, int) neighbour)
        {
            if (HasLineOfSight(_parent[s.Item1, s.Item2], neighbour))
            {
                var parentPosition = _parent[s.Item1, s.Item2];
                var parentScore = _gScore[parentPosition.Item1, parentPosition.Item2] +
                                  GetMagnitude(parentPosition, neighbour);
                if (!(parentScore < _gScore[neighbour.Item1, neighbour.Item2])) return;
                _gScore[neighbour.Item1, neighbour.Item2] = parentScore;
                _parent[neighbour.Item1, neighbour.Item2] = parentPosition;

                RemoveIfExist(neighbour);

                Enqueue(neighbour,
                    _gScore[neighbour.Item1, neighbour.Item2] + GetMagnitude(neighbour, _end));
            }
            else
            {
                var score = _gScore[s.Item1, s.Item2] + GetMagnitude(s, neighbour);
                if (!(score < _gScore[neighbour.Item1, neighbour.Item2])) return;
                _gScore[neighbour.Item1, neighbour.Item2] = score;
                _parent[neighbour.Item1, neighbour.Item2] = s;
                RemoveIfExist(neighbour);
                Enqueue(neighbour, _gScore[neighbour.Item1, neighbour.Item2] + GetMagnitude(neighbour, _end));
            }
        }

        // If has Line of Sight - return distance, otherwise -1
        // copy-pasta from last section of https://news.movel.ai/theta-star
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasLineOfSight((int, int) s, (int, int) s2)
        {
            var sx = s.Item1; // OPTIMISATION3: another optimisation to reduce v2Int.x expensive call
            var sy = s.Item2;

            var x0 = sx;
            var y0 = sy;
            var x1 = s2.Item1;
            var y1 = s2.Item2;

            var dy = y1 - y0;
            var dx = x1 - x0;

            var f = 0;

            if (dy < 0)
            {
                dy = -dy;
                sy = -1;
            }
            else
            {
                sy = 1;
            }

            if (dx < 0)
            {
                dx = -dx;
                sx = -1;
            }
            else
            {
                sx = 1;
            }


            if (dx >= dy)
                while (x0 != x1)
                {
                    f += dy;
                    if (f >= dx)
                    {
                        if (!_map[x0 + (sx - 1) / 2, y0 + (sy - 1) / 2]) return false;
                        y0 += sy;
                        f -= dx;
                    }

                    if (f != 0 && !_map[x0 + (sx - 1) / 2, y0 + (sy - 1) / 2]) return false;


                    if (dy == 0 && !_map[x0 + (sx - 1) / 2, y0] && !_map[x0 + (sx - 1) / 2, y0 - 1]) return false;
                    x0 += sx;
                }
            else
                while (y0 != y1)
                {
                    f += dx;
                    if (f >= dy)
                    {
                        if (!_map[x0 + (sx - 1) / 2, y0 + (sy - 1) / 2]) return false;
                        x0 += sx;
                        f -= dy;
                    }

                    if (f != 0 && !_map[x0 + (sx - 1) / 2, y0 + (sy - 1) / 2]) return false;
                    if (dx == 0 && !_map[x0, y0 + (sy - 1) / 2] && !_map[x0 - 1, y0 + (sy - 1) / 2]) return false;
                    y0 += sy;
                }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int, int)[] GetNeighbours((int, int) position) // OPTIMISATION5 - use tuples instead of Vectors
        {
            return new[]
            {
                (position.Item1 + 1, position.Item2),
                (position.Item1 - 1, position.Item2),
                (position.Item1, position.Item2 + 1),
                (position.Item1, position.Item2 - 1)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Vector2Int> ReconstructPath((int, int) s)
        {
            var result = new List<Vector2Int>();
            while (_parent[s.Item1, s.Item2] != s)
            {
                result.Add(new Vector2Int(s.Item1, s.Item2));
                s = _parent[s.Item1, s.Item2];
            }

            var p = _parent[s.Item1, s.Item2];
            result.Add(new Vector2Int(p.Item1, p.Item2));
            return result;
        }


        private class
            TupleComparer : IEqualityComparer<(int, int)> // OPTIMISATION 6 - replace hash calculation at comparer functions 16 bit per axis
        {
            public bool Equals((int, int) x, (int, int) y)
            {
                return x.Item1 == y.Item1 && x.Item2 == y.Item2;
            }

            public int GetHashCode((int, int) obj)
            {
                return (obj.Item1 << 16) | obj.Item2;
            }
        }
    }
}