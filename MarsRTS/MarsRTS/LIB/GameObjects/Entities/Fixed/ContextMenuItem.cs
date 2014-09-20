using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class ContextMenuItem
    {
        string text;

        /// <summary>
        /// Context menu item text
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        Vector2 buttonPosition, textPosition;

        public Vector2 ButtonPosition
        {
            get { return buttonPosition; }
            set { buttonPosition = value; }
        }

        public Vector2 TextPosition
        {
            get { return textPosition; }
            set { textPosition = value; }
        }

        Point size;

        public Point Size
        {
            get { return size; }
            set { size = value; }
        }

        ButtonState state;

        public ButtonState State
        {
            get { return state; }
            set { state = value; }
        }

        EventHandler onClick;

        /// <summary>
        /// Callback for when this item is clicked
        /// </summary>
        public EventHandler OnClick
        {
            get { return onClick; }
            set { onClick = value; }
        }

        object tag;

        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public ContextMenuItem(string text)
        {
            this.text = text;
        }

        public void SetState(ButtonState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Gets the rectangle bounds of this button
        /// </summary>
        /// <param name="parentLocation"> Location of the parent menu </param>
        public Rectangle GetBounds(Point parentLocation)
        {
            return new Rectangle()
            {
                X = parentLocation.X + (int)buttonPosition.X,
                Y = parentLocation.Y + (int)buttonPosition.Y,
                Width = size.X,
                Height = size.Y
            };
        }
    }
}
