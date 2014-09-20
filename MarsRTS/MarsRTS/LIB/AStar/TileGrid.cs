using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace PathFinding
{
    public class TileGrid<T> where T : PathNode, new()
    {
        /// <summary>
        /// Size of the grid
        /// </summary>
        readonly int width, height;

        /// <summary>
        /// Grid width
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Grid height
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Rectangle representing the bounds used for determining if a node 
        /// is inside the grid while path finding
        /// </summary>
        readonly Rectangle bounds;

        T[][] grid;

        /// <summary>
        /// Grid item at the given position
        /// </summary>
        /// <param name="x"> X position </param>
        /// <param name="y"> Y position </param>
        public T this[int x, int y]
        {
            get { return grid[x][y]; }
            set { grid[x][y] = value; }
        }

        /// <summary>
        /// Grid item at the given position
        /// </summary>
        /// <param name="p"> Node location </param>
        public T this[Point p]
        {
            get { return grid[p.X][p.Y]; }
            set { grid[p.X][p.Y] = value; }
        }

        public TileGrid(int width, int height)
        {
            this.width = width;
            this.height = height;

            bounds = new Rectangle(0, 0, width, height);

            grid = new T[width][];

            // initialize our grid with empty grid spaces
            for (int x = 0; x < width; x++)
            {
                grid[x] = new T[height];

                for (int y = 0; y < height; y++)
                {
                    grid[x][y] = new T()
                    {
                        Index = new Point(x, y)
                    };
                }
            }
        }

        /// <summary>
        /// Calculate the shortest path between two points
        /// </summary>
        /// <param name="start"> Starting point </param>
        /// <param name="end"> Ending point </param>
        /// <param name="diagonal"> Allow diagonal movements </param>
        /// <returns> List of points included in the path (in order) </returns>
        public List<Point> FindPath(Point start, Point end, bool diagonal)
        {
            reset();

            var openHeap = new BinaryHeap<PathNode>();

            // find the node at the starting point, close it, and add it
            // to the heap
            var startNode = grid[start.X][start.Y];

            startNode.Closed = true;

            openHeap.Add(startNode);

            // iterate until there are no more items in the heap
            while (openHeap.Count > 0)
            {
                var currentNode = openHeap.Pop();

                // reached our destination
                if (currentNode.Index == end)
                {
                    var current = currentNode;

                    var path = new List<Point>();

                    path.Add(start);

                    while (current.Parent != null)
                    {
                        path.Add(current.Index);

                        current = current.Parent;
                    }

                    path.Reverse();

                    return path;
                }

                currentNode.Closed = true;

                var neighbors = getNeighbors(currentNode, diagonal);

                foreach (var neighbor in neighbors)
                {
                    // don't need to check it
                    if (neighbor.Closed || !neighbor.IsPassible)
                        continue;

                    // the g score is the shortest distance from start 
                    // to the current node. We need to check if the current
                    // neighbor to this node is the lowest cost so far
                    int g = currentNode.G + neighbor.Cost;

                    bool hasVisited = neighbor.Visited;

                    if (!hasVisited || g < neighbor.G)
                    {

                        // found the best node so far, so estimate it's cost
                        // and deal with it in the heap accordingly
                        neighbor.Visited = true;
                        neighbor.Parent = currentNode;
                        neighbor.H = manhattanDistance(neighbor.Index, end);
                        neighbor.G = g;
                        neighbor.F = neighbor.G + neighbor.H;

                        // add the neighbor to the heap if we haven't visited
                        // it yet, otherwise re score it in the heap
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

            int x = node.Index.X,
                y = node.Index.Y;

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
                    neighbors.Add(grid[point.X][point.Y]);
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

        /// <summary>
        /// Reset all spaces for path finding
        /// </summary>
        void reset()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var space = grid[x][y];

                    space.Closed = false;
                    space.Visited = false;
                    space.Parent = null;
                    space.F = space.G = space.H = 0;
                }
            }
        }
    }
}
