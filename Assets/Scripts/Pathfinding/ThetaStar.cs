using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Pathfinding
{
    public class ThetaStar
    {
        private static readonly Vector2Int UNKNOWN = new(-1, -1);
        private readonly HashSet<Vector2Int> _closedQueue;
        private readonly float[,] _gScore; // [x,y]
        private readonly int _height;


        private readonly bool[,] _map;

        private readonly PriorityQueue _openQueue; 
        private readonly Vector2Int[,] _parent; // [x,y]
        private readonly int _width;
        private Vector2Int _end;

        public ThetaStar(bool[,] map)
        {
            _map = map;
            _width = map.GetLength(0);
            _height = map.GetLength(1);
            _gScore = new float[_width, _height];
            _parent = new Vector2Int [_width, _height];
            _openQueue = new PriorityQueue();
            _closedQueue = new HashSet<Vector2Int>();
        }

        public bool IsPassable(Vector2Int position) => _map[position.x, position.y];

        private void ResetCache()
        {
            Array.Clear(_gScore, 0, _gScore.Length);
            Array.Clear(_parent, 0, _parent.Length);
            _openQueue.Clear();
            _closedQueue.Clear();
        }

        /// <summary>
        ///     Theta star pathfinding implementation,
        ///     <see>
        ///         <cref>https://en.wikipedia.org/wiki/Theta*</cref>
        ///     </see>
        /// </summary>
        public List<Vector2Int> CalculatePath(Vector2Int start, Vector2Int end)
        {
            _end = end;
            ResetCache();

            _gScore[start.x, start.y] = 0;
            _parent[start.x, start.y] = start;

            _openQueue.Enqueue(start, _gScore[start.x, start.y] + (start - end).magnitude);

            while (_openQueue.Count > 0)
            {
                var s = _openQueue.Dequeue();
                if (s == end) return ReconstructPath(s);

                _closedQueue.Add(s);
                foreach (var neighbour in GetNeighbours(s))
                {
                    if (neighbour.x < 0 || neighbour.y < 0 || neighbour.x >= _width || neighbour.y >= _height ||
                        !_map[neighbour.x, neighbour.y] ||
                        _closedQueue.Contains(neighbour)) continue;

                    if (!_openQueue.Contains(neighbour))
                    {
                        _gScore[neighbour.x, neighbour.y] = float.PositiveInfinity;
                        _parent[neighbour.x, neighbour.y] = UNKNOWN;
                    }

                    UpdateVertex(s, neighbour);
                }
            }

            return null;
        }

        private void UpdateVertex(Vector2Int s, Vector2Int neighbour)
        {
            if (HasLineOfSight(_parent[s.x, s.y], neighbour))
            {
                var parentPosition = _parent[s.x, s.y];
                var parentScore = _gScore[parentPosition.x, parentPosition.y] +
                                  (parentPosition - neighbour).magnitude;
                if (!(parentScore < _gScore[neighbour.x, neighbour.y])) return;
                _gScore[neighbour.x, neighbour.y] = parentScore;
                _parent[neighbour.x, neighbour.y] = parentPosition;

                if (_openQueue.Contains(neighbour)) _openQueue.Remove(neighbour);
                
                _openQueue.Enqueue(neighbour,
                    _gScore[neighbour.x, neighbour.y] + (neighbour - _end).magnitude);
            }
            else
            {
                var score = _gScore[s.x, s.y] + (s - neighbour).magnitude;
                if (!(score < _gScore[neighbour.x, neighbour.y])) return;
                _gScore[neighbour.x, neighbour.y] = score;
                _parent[neighbour.x, neighbour.y] = s;
                if (_openQueue.Contains(neighbour)) _openQueue.Remove(neighbour);
                _openQueue.Enqueue(neighbour,
                    _gScore[neighbour.x, neighbour.y] + (neighbour - _end).magnitude);
            }
        }

        // If has Line of Sight - return distance, otherwise -1
        // copy-pasta from last section of https://news.movel.ai/theta-star
        private bool HasLineOfSight(Vector2Int s, Vector2Int s2) 
        {
            var x0 = s.x;
            var y0 = s.y;
            var x1 = s2.x;
            var y1 = s2.y;

            var dy = y1 - y0;
            var dx = x1 - x0;

            var f = 0;

            if (dy < 0)
            {
                dy = -dy;
                s.y = -1;
            }
            else
            {
                s.y = 1;
            }

            if (dx < 0)
            {
                dx = -dx;
                s.x = -1;
            }
            else
            {
                s.x = 1;
            }


            if (dx >= dy)
                while (x0 != x1)
                {
                    f += dy;
                    if (f >= dx)
                    {
                        if (!_map[x0 + (s.x - 1) / 2, y0 + (s.y - 1) / 2]) return false;
                        y0 += s.y;
                        f -= dx;
                    }

                    if (f != 0 && !_map[x0 + (s.x - 1) / 2, y0 + (s.y - 1) / 2]) return false;


                    if (dy == 0 && !_map[x0 + (s.x - 1) / 2, y0] && !_map[x0 + (s.x - 1) / 2, y0 - 1]) return false;
                    x0 += s.x;
                }
            else
                while (y0 != y1)
                {
                    f += dx;
                    if (f >= dy)
                    {
                        if (!_map[x0 + (s.x - 1) / 2, y0 + (s.y - 1) / 2]) return false;
                        x0 += s.x;
                        f -= dy;
                    }

                    if (f != 0 && !_map[x0 + (s.x - 1) / 2, y0 + (s.y - 1) / 2]) return false;
                    if (dx == 0 && !_map[x0, y0 + (s.y - 1) / 2] && !_map[x0 - 1, y0 + (s.y - 1) / 2]) return false;
                    y0 += s.y;
                }

            return true;
        }

        private static Vector2Int[] GetNeighbours(Vector2Int src)
        {
            return new[]
            {
                src + Vector2Int.left,
                src + Vector2Int.right,
                src + Vector2Int.down,
                src + Vector2Int.up
            };
        }

        private List<Vector2Int> ReconstructPath(Vector2Int s)
        {
            var result = new List<Vector2Int>();
            while (_parent[s.x, s.y] != s)
            {
                result.Add(s);
                s = _parent[s.x, s.y];
            }
            result.Add(_parent[s.x, s.y]);
            return result;
        }
    }
}