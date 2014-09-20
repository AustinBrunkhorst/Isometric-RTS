using MarsRTS.LIB.GameObjects.Entities.Fixed;
using MarsRTS.LIB.PathFinding;
using Microsoft.Xna.Framework;

namespace MarsRTS.LIB.GameObjects
{
    public class GridSpace : PathNode
    {
        BuildingEntity occupant;

        /// <summary>
        /// Building occupying this grid space
        /// </summary>
        public BuildingEntity Occupant
        {
            get { return occupant; }
            set { occupant = value; }
        }

        Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        int tileIndex;

        public int TileIndex
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }

        public GridSpace()
        {
            IsPassible = true;
        }
    }
}
