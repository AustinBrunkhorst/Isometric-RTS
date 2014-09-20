using MarsRTS.LIB.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class BuildingStatDisplay
    {
        Rectangle barBounds, totalBarBounds;

        Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;

                barBounds.Location = totalBarBounds.Location =
                    (value + barOffset).ToPoint();
            }
        }

        readonly Vector2 barOffset = new Vector2(0, 35);

        readonly Point barSize = new Point(467, 12);

        readonly Color barColor = new Color(64, 64, 64);
        readonly Color totalBarColor;

        readonly Color colorMin = new Color(122, 132, 78);
        readonly Color colorMax = new Color(67, 97, 67);

        readonly Color titleColor = new Color(175, 175, 175);

        const int titleSize = 17;

        readonly string title;

        const string titleFont = "Franchise";

        public BuildingStatDisplay(string title, float percentage)
        {
            this.title = title;

            barBounds = totalBarBounds = new Rectangle()
            {
                Width = barSize.X,
                Height = barSize.Y
            };

            totalBarBounds.Width = (int)(barBounds.Width * percentage);

            totalBarColor = Color.Lerp(colorMin, colorMax, percentage);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float transition)
        {
            Font.Draw(spriteBatch,
                titleFont,
                title,
                position + offset,
                titleSize,
                titleColor * transition);

            drawRectangle(spriteBatch,
                getOffsetBounds(barBounds, offset),
                barColor * transition);

            drawRectangle(spriteBatch,
                getOffsetBounds(totalBarBounds, offset),
                totalBarColor * transition);
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture, bounds, color);
        }

        Rectangle getOffsetBounds(Rectangle bounds, Vector2 offset)
        {
            bounds.Location = (bounds.Location.ToVector() + offset).ToPoint();

            return bounds;
        }
    }
}
