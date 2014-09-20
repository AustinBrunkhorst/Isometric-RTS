using MarsRTS.LIB.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.Screen.Screens.ScreenObjects.Upgrade
{
    public class ResourceBox
    {
        Vector2 resourcePosition, bankPosition,
            costPosition, resourceIconPosition, affordabilityIconPosition;

        readonly Texture2D resourceIcon, affordabilityIcon;

        readonly Rectangle bounds, topBounds;

        readonly Color validBGColor = new Color(79, 132, 78);
        readonly Color invalidBGColor = new Color(159, 83, 83);

        readonly Color validTopColor = new Color(68, 115, 68);
        readonly Color invalidTopColor = new Color(143, 74, 74);

        readonly Color resourceFontColor = new Color(204, 204, 204);
        readonly Color bankFontColor = new Color(204, 204, 204);

        readonly Color validCostColor = new Color(58, 97, 57);
        readonly Color invalidCostColor = new Color(120, 62, 62);

        const int topBoxHeight = 20;

        const int resourceFontSize = 15;
        const int bankFontSize = 14;
        const int costFontSize = 16;

        const string font = "Franchise";

        readonly string resourceText, costText, bankText;

        readonly bool affordable;

        public ResourceBox(Texture2D resourceIcon, Texture2D affordabilityIcon, Rectangle bounds, string resource, int cost, int bank)
        {
            this.resourceIcon = resourceIcon;
            this.affordabilityIcon = affordabilityIcon;
            this.bounds = bounds;
            this.resourceText = resource;
            this.costText = numberFormat(cost);
            this.bankText = numberFormat(bank);

            topBounds = new Rectangle()
            {
                X = bounds.X,
                Y = bounds.Y - topBoxHeight,
                Width = bounds.Width,
                Height = topBoxHeight
            };

            affordable = (bank >= cost);

            initPositions();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float transition)
        {
            Color costColor, bgColor, topColor;

            if (affordable)
            {
                bgColor = validBGColor;
                topColor = validTopColor;
                costColor = validCostColor;
            }
            else
            {
                bgColor = invalidBGColor;
                topColor = invalidTopColor;
                costColor = invalidCostColor;
            }

            drawRectangle(spriteBatch,
                boundsFromOffset(bounds, offset),
                bgColor * transition);

            drawRectangle(spriteBatch,
                boundsFromOffset(topBounds, offset),
                topColor * transition);

            // resource
            Font.Draw(spriteBatch,
                font,
                resourceText,
                resourcePosition + offset,
                resourceFontSize,
                resourceFontColor * transition);

            // bank
            Font.Draw(spriteBatch,
                font,
                bankText,
                bankPosition + offset,
                bankFontSize,
                bankFontColor * transition);

            // cost
            Font.Draw(spriteBatch,
                font,
                costText,
                costPosition + offset,
                costFontSize,
                costColor * transition);

            // affordability icon position
            spriteBatch.Draw(affordabilityIcon,
                affordabilityIconPosition + offset,
                Color.White * transition);

            // resource icon
            spriteBatch.Draw(resourceIcon,
                resourceIconPosition + offset,
                Color.White * transition);
        }

        void initPositions()
        {
            var resourceSize = Font.Measure(font, resourceText,
                resourceFontSize);

            var bankSize = Font.Measure(font, bankText, bankFontSize);
            var costSize = Font.Measure(font, costText, costFontSize);

            var boxCenter = bounds.Center;
            var topCenter = topBounds.Center;

            affordabilityIconPosition = new Vector2()
            {
                X = boxCenter.X - (affordabilityIcon.Width / 2),
                Y = bounds.Y + 10
            };

            resourcePosition = new Vector2()
            {
                X = boxCenter.X - (resourceSize.X / 2),
                Y = affordabilityIconPosition.Y +
                    affordabilityIcon.Height + 15,
            };

            bankPosition = new Vector2()
            {
                X = topCenter.X - (bankSize.X / 2),
                Y = topCenter.Y - (bankSize.Y / 2) + 1,
            };

            costPosition = new Vector2()
            {
                X = boxCenter.X - (costSize.X / 2),
                Y = resourcePosition.Y + resourceSize.Y + 5,
            };

            resourceIconPosition = new Vector2()
            {
                X = resourcePosition.X - resourceIcon.Width - 5,
                Y = resourcePosition.Y + (resourceSize.Y / 2) -
                    (resourceIcon.Height / 2) - 2,
            };
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture, bounds, color);
        }

        string numberFormat(int number)
        {
            return string.Format("{0:n0}", number);
        }

        Rectangle boundsFromOffset(Rectangle input, Vector2 offset)
        {
            input.X += (int)offset.X;
            input.Y += (int)offset.Y;

            return input;
        }
    }
}
