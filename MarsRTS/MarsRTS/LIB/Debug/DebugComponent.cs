using MarsRTS.LIB.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB
{
    public class DebugComponent : DrawableGameComponent
    {
        int frameRate, frameCounter;

        TimeSpan elapsedTime = TimeSpan.Zero;
        SpriteBatch spriteBatch;

        string fpsText = "0";

        public DebugComponent(Game game) : base(game) { }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime = TimeSpan.Zero;
                frameRate = frameCounter;
                frameCounter = 0;
            }

            fpsText = String.Format("FPS: {0}", frameRate);
        }

        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            spriteBatch.Begin();

            Font.Draw(spriteBatch,
                "Myriad",
                fpsText,
                Vector2.Zero,
                18,
                Color.White);

            spriteBatch.End();
        }
    }
}