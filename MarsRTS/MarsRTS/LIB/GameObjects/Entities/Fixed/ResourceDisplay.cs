using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class ResourceDisplay
    {
        readonly Texture2D icon;

        Rectangle bounds, totalBounds;

        readonly Vector2 iconPosition, textPosition;

        readonly Point size = new Point(126, 36);

        readonly Color backgroundColor = new Color(70, 70, 70);
        readonly Color percentageBackgroundColor = new Color(84, 84, 84);
        readonly Color textColor = new Color(204, 204, 204);

        const int spacing = 10;
        const int textSize = 15;

        const string font = "Franchise";

        string text;

        int total, max;

        public ResourceDisplay(Texture2D icon, Point location, int index)
        {
            this.icon = icon;

            location.Y += (index * (size.Y + spacing));

            bounds = totalBounds = new Rectangle()
            {
                Location = location,
                Width = size.X,
                Height = size.Y
            };

            iconPosition = new Vector2()
            {
                X = location.X + 10,
                Y = location.Y + ((size.Y - icon.Height) / 2)
            };

            var bankSize = Font.Measure(font, "0", textSize);

            textPosition = new Vector2()
            {
                X = iconPosition.X + icon.Width + 10,
                Y = location.Y + ((size.Y - bankSize.Y) / 2)
            };

            text = "0";
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            drawRectangle(spriteBatch, bounds, backgroundColor);
            drawRectangle(spriteBatch, totalBounds, percentageBackgroundColor);

            spriteBatch.Draw(icon, iconPosition, Color.White);

            Font.Draw(spriteBatch,
                font,
                text,
                textPosition,
                textSize,
                textColor);
        }

        public void SetTotal(int total)
        {
            this.total = total;
            text = string.Format("{0:n0}", total);
            updatePercentage();
        }

        public void SetMax(int max)
        {
            this.max = max;
            updatePercentage();
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture, bounds, bounds, color);
        }

        void updatePercentage()
        {
            float percentage = (float)total / (float)max;

            totalBounds.Width = (int)(bounds.Width * percentage);
        }
    }
}
