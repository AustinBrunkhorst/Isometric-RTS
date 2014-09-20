using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB.Screen.Screens.ScreenObjects
{
    public class ButtonItem
    {
        EventHandler onClick;

        public EventHandler OnClick
        {
            get { return onClick; }
            set { onClick = value; }
        }

        readonly Rectangle bounds;

        public Rectangle Bounds
        {
            get { return bounds; }
        }

        readonly Vector2 textPosition;

        readonly Color backgroundColorIdle = new Color(49, 49, 49);
        readonly Color backgroundColorActive = new Color(28, 28, 28);
        readonly Color backgroundColorHover = new Color(97, 97, 97);
        readonly Color backgroundColorDisabled = new Color(54, 54, 54);

        readonly Color fontColorIdle = new Color(175, 175, 175);
        readonly Color fontColorActive = new Color(175, 175, 175);
        readonly Color fontColorHover = new Color(175, 175, 175);
        readonly Color fontColorDisabled = new Color(175, 175, 175);

        readonly string text;

        const string font = "Franchise";

        const int fontSize = 16;
        const int padding = 15;
        const int height = 35;

        bool active, hovering, disabled;

        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }

        public ButtonItem(Vector2 position, string text, bool disabled)
        {
            this.text = text;

            this.disabled = disabled;

            var buttonSize = GetSize(text);
            var textSize = Font.Measure(font, text, fontSize);

            bounds = new Rectangle()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Width = buttonSize.X,
                Height = buttonSize.Y
            };

            var center = bounds.Center;

            textPosition = new Vector2()
            {
                X = center.X - (textSize.X / 2),
                Y = center.Y - (textSize.Y / 2)
            } + new Vector2(1);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float transition)
        {
            Color backgroundColor, fontColor;

            if (disabled)
            {
                backgroundColor = backgroundColorDisabled;
                fontColor = fontColorDisabled;
            }
            else
            {
                if (active)
                {
                    backgroundColor = backgroundColorActive;
                    fontColor = fontColorActive;
                }
                else if (hovering)
                {
                    backgroundColor = backgroundColorHover;
                    fontColor = fontColorHover;
                }
                else
                {
                    backgroundColor = backgroundColorIdle;
                    fontColor = fontColorIdle;
                }
            }

            drawRectangle(spriteBatch,
                boundsFromOffset(bounds, offset),
                backgroundColor * transition);

            Font.Draw(spriteBatch,
                font,
                text,
                textPosition + offset,
                fontSize,
                fontColor * transition);
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }

        public void SetHovering(bool hovering)
        {
            this.hovering = hovering;
        }

        public void SetState(bool disabled)
        {
            this.disabled = disabled;
        }

        public static Point GetSize(string text)
        {
            var size = Font.Measure(font, text, fontSize);

            return new Point()
            {
                X = (int)size.X + (2 * padding),
                Y = height
            };
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture,
                rectangle,
                rectangle,
                color);
        }

        Rectangle boundsFromOffset(Rectangle rectangle, Vector2 offset)
        {
            rectangle.Location =
                (rectangle.Location.ToVector() + offset).ToPoint();

            return rectangle;
        }
    }
}
