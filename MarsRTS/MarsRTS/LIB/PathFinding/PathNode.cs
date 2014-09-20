using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.PathFinding
{
    /// <summary>
    /// Represents a node that is used to find the shortest path in A*
    /// </summary>
    public abstract class PathNode : IComparable<PathNode>
    {
        int f, g, h, cost;

        /// <summary>
        /// Total cost of this node from estimations
        /// </summary>
        public int F
        {
            get { return f; }
            set { f = value; }
        }

        /// <summary>
        /// Total cost of this node from the starting point
        /// </summary>
        public int G
        {
            get { return g; }
            set { g = value; }
        }

        /// <summary>
        /// Estimated Heuristic cost of this node from the destination
        /// </summary>
        public int H
        {
            get { return h; }
            set { h = value; }
        }

        /// <summary>
        /// Initial cost of this node
        /// </summary>
        public int Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        bool visited;

        /// <summary>
        /// Whether or not we've searched this node
        /// </summary>
        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }

        bool closed;

        /// <summary>
        /// Whether or not this node is able to be searched again
        /// </summary>
        public bool Closed
        {
            get { return closed; }
            set { closed = value; }
        }

        bool isPassible;

        /// <summary>
        /// Whether or not this node can be used as a path
        /// </summary>
        public bool IsPassible
        {
            get { return isPassible; }
            set { isPassible = value; }
        }

        Point index;

        /// <summary>
        /// Grid location
        /// </summary>
        public Point Index
        {
            get { return index; }
            set { index = value; }
        }

        PathNode parent;

        /// <summary>
        /// Parent node this was created from as an adjacent cell
        /// </summary>
        public PathNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Compares the f cost variable
        /// </summary>
        int IComparable<PathNode>.CompareTo(PathNode other)
        {
            return f - other.F;
        }
    }
}
