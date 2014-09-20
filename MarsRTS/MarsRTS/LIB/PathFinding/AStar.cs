using MarsRTS.LIB.GameObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarsRTS.LIB.PathFinding
{
    public class AStar
    {
        TileGrid<PathNode> grid;

        Rectangle bounds;

        public AStar(TileGrid<PathNode> grid)
        {
            this.grid = grid;

            bounds = new Rectangle(0, 0, grid.Width, grid.Height);
        }

        public List<Point> Search(Point start, Point end, bool diagonals = false)
        {
            var openHeap = new BinaryHeap<PathNode>();

            // find the node at the starting point, close it, and add it
            // to the heap
            var startNode = grid[start.X, start.Y];

            startNode.Closed = true;

            openHeap.Add(startNode);

            // iterate until there are no more items in the heap
            while (openHeap.Count > 0)
            {
                var currentNode = openHeap.Pop();

                // reached our destination
                if (currentNode.Location == end)
                {
                    var current = currentNode;

                    var path = new List<Point>();

                    path.Add(start);

                    while (current.Parent != null)
                    {
                        path.Add(current.Location);

                        current = current.Parent;
                    }

                    path.Reverse();

                    return path;
                }

                currentNode.Closed = true;

                var neighbors = getNeighbors(currentNode, diagonals);

                foreach (var neighbor in neighbors)
                {
                    // don't need to check it
                    if (neighbor.Closed || !neighbor.IsPassible)
                        continue;

                    // the g score is the shortest distance from start 
                    // to the current node. We need to check if the current
                    // neightbor to this node is the lowest cost so far
                    int g = currentNode.G + neighbor.Cost;

                    bool hasVisited = neighbor.Visited;

                    if (!hasVisited || g < neighbor.G)
                    {
                        // found the best node so far, so esitmate it's cost
                        // and deal with it in the heap accordingly
                        neighbor.Visited = true;
                        neighbor.Parent = currentNode;
                        neighbor.H = manhattanDistance(neighbor.Location, end);
                        neighbor.G = g;
                        neighbor.F = neighbor.G + neighbor.H;

                        // add the neighbor to the heap if we haven't visited
                        // it yet, otherwise rescore it in the heap
                        if (!hasVisited)
                            openHeap.Add(neighbor);
                        else
                            openHeap.RescoreItem(neighbor);
                    }
                }
            }

            return new List<Point>();
        }

        /// <summary>
        /// Gets all neighbors at the given point
        /// </summary>
        List<PathNode> getNeighbors(PathNode node, bool diagonal)
        {
            var neighbors = new List<PathNode>();

            int x = node.Location.X,
                y = node.Location.Y;

            var points = new List<Point>()
            {
                new Point(x - 1, y), // west
                new Point(x + 1, y), // east
                new Point(x, y - 1), // north
                new Point(x, y + 1), // south
            };

            if (diagonal)
            {
                points.Add(new Point(x - 1, y - 1)); // north west
                points.Add(new Point(x + 1, y - 1)); // north east
                points.Add(new Point(x - 1, y + 1)); // south west
                points.Add(new Point(x + 1, y + 1)); // south east
            }

            foreach (var point in points)
            {
                if (bounds.Contains(point))
                    neighbors.Add(grid[point.X, point.Y]);
            }

            return neighbors;
        }

        /// <summary>
        /// Manhattan distance between two points. Used for estimating the 
        /// score of a path node
        /// </summary>
        int manhattanDistance(Point p1, Point p2)
        {
            return Math.Max(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        }
    }
}
