using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MarsRTS.LIB.ScreenManager
{
    public class ScreenManager : DrawableGameComponent
    {
        /// <summary>
        /// Determines if the manager has been initialized yet
        /// for use with loading content
        /// </summary>
        bool initialized;

        SpriteBatch spriteBatch;

        /// <summary>
        /// Spritebatch used by all screens
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        Color backgroundColor = Color.Black;

        /// <summary>
        /// Background color to clear screen with
        /// </summary>
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        /// <summary>
        /// Game screens being managed
        /// </summary>
        List<GameScreen> screens;

        /// <summary>
        /// Queue of screens to be updated based on visibility
        /// </summary>
        List<GameScreen> screenUpdateQueue;

        public ScreenManager(Game game)
            : base(game)
        {
            screens = new List<GameScreen>();
            screenUpdateQueue = new List<GameScreen>();
        }

        public override void Initialize()
        {
            base.Initialize();

            initialized = true;
        }

        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (GameScreen screen in screens)
                screen.LoadContent(Game.Content);
        }

        protected override void UnloadContent()
        {
            foreach (GameScreen screen in screens)
                screen.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screenUpdateQueue.Clear();

            foreach (GameScreen screen in screens)
                screenUpdateQueue.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // iterate until there are no more in the queue
            while (screenUpdateQueue.Count > 0)
            {
                // pop a screen from the end of the list
                GameScreen screen = screenUpdateQueue[screenUpdateQueue.Count - 1];

                screenUpdateQueue.RemoveAt(screenUpdateQueue.Count - 1);

                // update the screen
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.State == ScreenState.TransitionIn ||
                    screen.State == ScreenState.Active)
                {
                    // give this screen input
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput();

                        otherScreenHasFocus = true;
                    }

                    // If this isn't an overlay, set proceeding
                    // screens covered
                    if (!screen.IsOverlay)
                        coveredByOtherScreen = true;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // iterates through all screens in this manager
            // and draws it if it's state is not hidden
            foreach (GameScreen screen in screens.ToArray())
            {
                if (screen.State != ScreenState.Hidden)
                    screen.Draw();
            }
        }

        /// <summary>
        /// Adds a screen to the manager
        /// </summary>
        /// <param name="screen"> Screen to add </param>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.IsExiting = false;

            if (initialized)
                screen.LoadContent(Game.Content);

            screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the manager
        /// </summary>
        /// <param name="screen"> Screen to remove </param>
        public void RemoveScreen(GameScreen screen)
        {
            if (initialized)
                screen.UnloadContent();

            screens.Remove(screen);
            screenUpdateQueue.Remove(screen);
        }

        /// <summary>
        /// Returns the number of screens being managed currently
        /// </summary>
        /// <returns></returns>
        public int GetScreenCount()
        {
            return screens.Count;
        }

        /// <summary>
        /// Traces all screens being managed
        /// </summary>
        public void Debug()
        {
            List<string> screenNames = new List<string>();

            System.Diagnostics.Debug.WriteLine("-- SCREEN DEBUG --");

            foreach (var screen in screens)
                System.Diagnostics.Debug.WriteLine(screen.GetType().Name);

            System.Diagnostics.Debug.WriteLine("------------------");
        }
    }
}
