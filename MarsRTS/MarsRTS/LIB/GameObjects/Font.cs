using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarsRTS.LIB.GameObjects
{
    public static class Font
    {
        static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

        const float maxSize = 27.0f;

        public static void LoadContent(ContentManager content, Dictionary<string, string> fontList)
        {
            // load all fonts
            foreach (var font in fontList)
                fonts[font.Key] = content.Load<SpriteFont>(font.Value);
        }

        public static Vector2 Measure(string font, string text, float size)
        {
            return fonts[font].MeasureString(text) * getScale(size);
        }

        public static void Draw(SpriteBatch spriteBatch, string font, string text, Vector2 position, float size, Color color)
        {
            position.X = (float)Math.Floor(position.X);
            position.Y = (float)Math.Floor(position.Y);

            spriteBatch.DrawString(fonts[font],
                text,
                position,
                color,
                0,
                Vector2.Zero,
                getScale(size),
                SpriteEffects.None,
                0);
        }

        public static void Draw(SpriteBatch spriteBatch, string font, string text, Vector2 position, Vector2 size, Color color)
        {
            position.X = (float)Math.Floor(position.X);
            position.Y = (float)Math.Floor(position.Y);

            var scale = new Vector2()
            {
                X = getScale(size.X),
                Y = getScale(size.Y)
            };

            spriteBatch.DrawString(fonts[font],
                text,
                position,
                color,
                0,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0);
        }

        public static string WordWrap(string font, string text, float size, int maxLineWidth)
        {
            var spriteFont = fonts[font];

            var words = text.Split(' ');

            var stringBuilder = new StringBuilder();

            float lineWidth = 0.0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (var word in words)
            {
                var wordSize = Measure(font, word, size);

                if (lineWidth + wordSize.X < maxLineWidth)
                {
                    stringBuilder.Append(word + " ");
                    lineWidth += wordSize.X + spaceWidth;
                }
                else
                {
                    stringBuilder.Append("\n" + word + " ");
                    lineWidth = wordSize.X + spaceWidth;
                }
            }

            return stringBuilder.ToString();
        }

        static float getScale(float size)
        {
            return size / maxSize;
        }
    }
}
