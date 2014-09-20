using MarsRTS.LIB;
using MarsRTS.LIB.GameObjects;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.Screen.Screens;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MarsRTS
{
    public class MarsRTS : Game
    {
        GraphicsDeviceManager graphics;

        ScreenManager screenManager;

        static CursorManager cursorManager;

        static Texture2D blankTexture;

        public static Texture2D BlankTexture
        {
            get { return MarsRTS.blankTexture; }
            set { MarsRTS.blankTexture = value; }
        }

        public static Cursor Cursor
        {
            get { return cursorManager.Cursor; }
        }

        public MarsRTS()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100,
                PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100,
                SynchronizeWithVerticalRetrace = false,
                PreferMultiSampling = true,
                //IsFullScreen = true
            };

            IsFixedTimeStep = false;

            Window.Title = "Mars Defense";

            Content.RootDirectory = "Content";

            screenManager = new ScreenManager(this)
            {
                BackgroundColor = new Color(253, 198, 137)
            };

            cursorManager = new CursorManager(this);
        }

        public static void SetCursor(Cursor cursor)
        {
            cursorManager.Cursor = cursor;
        }

        protected override void Initialize()
        {
            screenManager.AddScreen(new Play());

            Components.Add(screenManager);
            Components.Add(new DebugComponent(this));
            Components.Add(cursorManager);

            string fontDir = "Fonts/";

            var fonts = new Dictionary<string, string>()
            {
                {"Franchise", fontDir + "Franchise"},
                {"Myriad", fontDir + "Myriad"}
            };

            Font.LoadContent(Content, fonts);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData<Color>(new Color[] { Color.White });

            Cursors.LoadContent(this, Content);

            // default
            cursorManager.Cursor = Cursors.Default;

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(screenManager.BackgroundColor);

            base.Draw(gameTime);
        }
    }
}
