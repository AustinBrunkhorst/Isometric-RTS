using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.GameObjects
{
    public class HealthBar
    {
        public static Texture2D Texture;

        Vector2 size, percentageSize;

        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;

                percentageSize.Y = value.Y;
            }
        }

        public HealthBar(Point size)
        {
            this.size = percentageSize = new Vector2(size.X, size.Y);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            spriteBatch.Draw(Texture,
                position,
                Texture.Bounds,
                Color.Black,
                0,
                Vector2.Zero,
                size,
                SpriteEffects.None,
                1.0f);

            spriteBatch.Draw(Texture,
                position,
                Texture.Bounds,
                color,
                0,
                Vector2.Zero,
                percentageSize,
                SpriteEffects.None,
                1.0f);
        }

        public void SetPercentage(float percentage)
        {
            percentageSize.X = size.X * percentage;
        }
    }
}
