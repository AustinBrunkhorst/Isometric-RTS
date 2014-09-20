using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class BuildingIcon
    {
        EventHandler onClick;

        public EventHandler OnClick
        {
            get { return onClick; }
            set { onClick = value; }
        }

        Texture2D texture;

        Rectangle bounds, titleBounds;

        Vector2 titlePosition, infoPosition;

        public static readonly Point Size = new Point(133, 133);

        readonly Color backgroundColor = new Color(235, 235, 235);
        readonly Color backgroundColorHover = new Color(250, 250, 250);
        readonly Color titleBackgroundColor = new Color(49, 49, 49) * 0.70f;
        readonly Color titleFontColor = new Color(225, 225, 225);

        readonly Color infoFontColor;
        readonly Color infoColorValid = new Color(58, 117, 49);
        readonly Color infoColorFull = new Color(168, 168, 168);

        readonly string title, infoText;

        const string titleFont = "Franchise";
        const string infoFont = "Franchise";

        const int titleFontSize = 12;
        const int infoFontSize = 12;

        bool hovering;

        public BuildingIcon(Texture2D texture, string title, int total, int max)
        {
            this.texture = texture;
            this.title = title;

            var titleHeight = 26;

            this.bounds = new Rectangle(0, 0, Size.X, Size.Y);
            this.titleBounds = new Rectangle(0, 0, Size.X, titleHeight);

            var titleSize = Font.Measure(titleFont, title, titleFontSize);

            titlePosition = (new Vector2(titleBounds.Width,
                titleBounds.Height) - titleSize) / 2;

            infoText = string.Format("{0}/{1}", total, max);

            infoFontColor = infoColorValid;

            if (total >= max)
                infoFontColor = infoColorFull;

            infoPosition = new Vector2()
            {
                X = 5,
                Y = Size.Y -
                    Font.Measure(infoFont, infoText, infoFontSize).Y - 5
            };
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float transition)
        {
            Color background = backgroundColor;

            if (hovering)
                background = backgroundColorHover;

            drawRectangle(spriteBatch,
                offsetRectangle(bounds, offset),
                background * transition);

            spriteBatch.Draw(texture,
                offset,
                Color.White * transition);

            drawRectangle(spriteBatch,
                offsetRectangle(titleBounds, offset),
                titleBackgroundColor * transition);

            Font.Draw(spriteBatch,
                titleFont,
                title,
                offset + titlePosition,
                titleFontSize,
                titleFontColor * transition);

            Font.Draw(spriteBatch,
                infoFont,
                infoText,
                offset + infoPosition,
                infoFontSize,
                infoFontColor * transition);
        }

        public void SetHovering(bool hovering)
        {
            this.hovering = hovering;
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture,
                bounds,
                bounds,
                color);
        }

        Rectangle offsetRectangle(Rectangle rectangle, Vector2 offset)
        {
            rectangle.X += (int)offset.X;
            rectangle.Y += (int)offset.Y;

            return rectangle;
        }
    }
}
