using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.Extensions
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Converts the given vector to a point
        /// </summary>
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        /// <summary>
        /// Creates a vector from an angle
        /// </summary>
        public static Vector2 FromAngle(float angle)
        {
            return new Vector2()
            {
                X = (float)Math.Cos(angle),
                Y = (float)Math.Sin(angle)
            };
        }
    }
}
