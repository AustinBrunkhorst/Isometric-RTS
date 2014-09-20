using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.Extensions
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Calculates the intersection depth between two rectangles
        /// </summary>
        public static Vector2 GetIntersectionDepth(this Rectangle a, Rectangle b)
        {
            // half sizes of both rectangles
            float halfWidthA = a.Width / 2.0f,
                  halfHeightA = a.Height / 2.0f,
                  halfWidthB = b.Width / 2.0f,
                  halfHeightB = b.Height / 2.0f;

            // center points
            Vector2 centerA = new Vector2(a.Left + halfWidthA,
                a.Top + halfHeightA);
            Vector2 centerB = new Vector2(b.Left + halfWidthB,
                b.Top + halfHeightB);

            // current and minimum-non-intersecting distances between centers
            float dX = centerA.X - centerB.X,
                  dY = centerA.Y - centerB.Y,
                  minDX = halfWidthA + halfWidthB,
                  minDY = halfHeightA + halfHeightB;

            // no intersection? return empty
            if (Math.Abs(dX) >= minDX || Math.Abs(dY) >= minDY)
                return Vector2.Zero;

            return new Vector2()
            {
                X = (dX > 0 ? 1.0f : -1.0f) * minDX - dX,
                Y = (dY > 0 ? 1.0f : -1.0f) * minDY - dY
            };
        }
    }
}
