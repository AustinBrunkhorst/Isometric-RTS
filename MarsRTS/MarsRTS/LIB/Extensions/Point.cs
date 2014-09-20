using Microsoft.Xna.Framework;

namespace MarsRTS.LIB.Extensions
{
    public static class PointExtensions
    {
        /// <summary>
        /// Converts a point to a vector
        /// </summary>
        public static Vector2 ToVector(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }
    }
}
