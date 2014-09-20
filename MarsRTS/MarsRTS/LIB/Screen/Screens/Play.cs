using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSAssetData.Enemies;
using System;

namespace MarsRTS.LIB.Screen.Screens
{
    public class Play : GameScreen
    {
        /// <summary>
        /// Maximum distance from mouse down to mouse up to consider invoking
        /// a click
        /// </summary>
        const int clickDistanceThreshold = 5;

        MarsWorld world;

        Point lastMouseDown;

        Random random = new Random();

        public Play()
        {
            Camera camera = new Camera()
            {
                ShakeSpeed = new Vector2(32, 16),
            };

            world = new MarsWorld(camera, this);

            TransitionInDuration = TimeSpan.FromMilliseconds(400);
            TransitionOutDuration = TimeSpan.FromMilliseconds(400);
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            HealthBar.Texture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
            HealthBar.Texture.SetData<Color>(new Color[] { Color.White });

            world.LoadContent(content);
        }

        public override void HandleInput()
        {
            if (InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                if (InputManager.IsMouseButtonTriggered(MouseButton.Left))
                {
                    lastMouseDown = InputManager.MousePosition;
                }

                if (world.CanDrag)
                {
                    Vector2 delta = InputManager.LastMousePosition.ToVector() -
                        InputManager.MousePosition.ToVector();

                    world.Camera.Move(-delta);
                }
            }
            else if (InputManager.IsMouseButtonClicked(MouseButton.Left))
            {
                var current = InputManager.MousePosition.ToVector();

                float distance = Vector2.Distance(current,
                    lastMouseDown.ToVector());

                if (distance <= clickDistanceThreshold)
                {
                    world.OnClick(current - world.Camera.RawPosition);
                }
            }

            if (InputManager.IsKeyTriggered(Keys.B))
            {
                ScreenManager.AddScreen(new BuildingListScreen(world));
            }

            var mouse = InputManager.MousePosition.ToVector() - world.Camera.RawPosition;

            var selection = world.GetNodePosition(mouse);

            if (InputManager.IsMouseButtonTriggered(MouseButton.Right))
            {
                if (new Rectangle(0, 0, MarsWorld.Size.X, MarsWorld.Size.Y).Contains(selection))
                {
                    var type = InputManager.IsKeyPressed(Keys.LeftShift) ?
                        Enemy.Tank : Enemy.Basic;

                    for (int i = 0; i < 25; i++)
                    {
                        var offset = new Vector2(random.Next(250), random.Next(250));
                        world.AddEnemy(type, mouse - new Vector2(10) + offset);
                    }
                }
            }

            if (InputManager.IsKeyTriggered(Keys.OemTilde))
            {
                MarsWorld.Debug = !MarsWorld.Debug;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            base.Update(gameTime, otherScreenFocused, covered);

            world.Update(gameTime);
        }

        public override void Draw()
        {
            SpriteBatch.Begin();

            world.Draw(SpriteBatch);

            SpriteBatch.End();
        }

        float ease(float t)
        {
            return t < .5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }
    }
}
