using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.GameObjects
{
    public struct EntityBorder
    {
        Rectangle bounds;

        float angle;

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public EntityBorder(Vector2 p1, Vector2 p2, int weight)
        {
            this.angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            this.bounds = new Rectangle()
             {
                 Location = new Point((int)p1.X, (int)p1.Y),
                 Width = (int)Vector2.Distance(p1, p2),
                 Height = weight
             };
        }

        public Rectangle GetBounds(Vector2 camera)
        {
            return new Rectangle()
            {
                X = (int)(bounds.X + camera.X),
                Y = (int)(bounds.Y + camera.Y),
                Width = bounds.Width,
                Height = bounds.Height
            };
        }
    }
}
