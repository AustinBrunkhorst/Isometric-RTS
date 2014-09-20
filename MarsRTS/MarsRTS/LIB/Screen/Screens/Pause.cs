using MarsRTS.LIB.Input;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MarsRTS.LIB.Screen.Screens
{
    class Pause : GameScreen
    {
        Texture2D texture, background;

        Vector2 texturePosition;

        float offset;

        Color tint;

        public Pause()
        {
            TransitionInDuration = TimeSpan.FromMilliseconds(400);
            TransitionOutDuration = TimeSpan.FromMilliseconds(400);

            IsOverlay = true;
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            texture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { new Color(192, 197, 207) });

            background = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
            background.SetData<Color>(new Color[] { Color.Black * 0.75f });
        }

        public override void HandleInput()
        {
            if (InputManager.IsKeyTriggered(Keys.Enter))
                ExitScreen();
        }

        public override void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            base.Update(gameTime, otherScreenFocused, covered);

            float t = 1 - TransitionOffset;
            int width = Viewport.Width / 2;
            int height = Viewport.Height / 2;

            if (IsTransitioning)
            {
                texturePosition = Vector2.Lerp(new Vector2(width, height), new Vector2(width - 250, height - 250), ease(t));
            }
            else
            {
                texturePosition = new Vector2(width - 250, height - 250);
            }
        }

        public override void Draw()
        {
            SpriteBatch.Begin();

            float t = ease((1 - TransitionOffset));

            SpriteBatch.Draw(background, Viewport.Bounds, Viewport.Bounds, Color.White * t);
            SpriteBatch.Draw(texture, texturePosition, texture.Bounds, Color.White, 0, Vector2.Zero, 500 * t, SpriteEffects.None, 0);

            SpriteBatch.End();
        }

        float ease(float t)
        {
            return t < .5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }
    }
}
