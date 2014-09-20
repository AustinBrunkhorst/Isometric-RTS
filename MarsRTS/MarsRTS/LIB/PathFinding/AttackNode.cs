using Microsoft.Xna.Framework;

namespace MarsRTS.LIB.PathFinding
{
    public class AttackNode
    {
        int idleFrame;

        /// <summary>
        /// Idle 
        /// </summary>
        public int IdleFrame
        {
            get { return idleFrame; }
            set { idleFrame = value; }
        }

        Point destination;

        /// <summary>
        /// Final pathfinding destination
        /// </summary>
        public Point Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public AttackNode(Point destination, int idleFrame)
        {
            this.destination = destination;
            this.idleFrame = idleFrame;
        }
    }
}
