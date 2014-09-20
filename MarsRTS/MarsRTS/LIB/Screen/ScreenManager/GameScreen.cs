using MarsRTS.LIB.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB.ScreenManager
{
    public abstract class GameScreen
    {
        ScreenState state = ScreenState.TransitionIn;

        /// <summary>
        /// Bound to when the screen is finished transitioning in
        /// </summary>
        public event EventHandler TransitionedIn;

        /// <summary>
        /// Bound to when the screen is finished transitioning out
        /// </summary>
        public event EventHandler TransitionedOut;

        /// <summary>
        /// The screens current state
        /// </summary>
        public ScreenState State
        {
            get { return state; }
            set { state = value; }
        }

        bool isOverlay;

        /// <summary>
        /// Determines if the screen should be displayed
        /// without removing screens behind it
        /// </summary>
        public bool IsOverlay
        {
            get { return isOverlay; }
            protected set { isOverlay = value; }
        }

        TimeSpan transitionInDuration = TimeSpan.Zero;

        /// <summary>
        /// Duration for transitioning in
        /// </summary>
        public TimeSpan TransitionInDuration
        {
            get { return transitionInDuration; }
            protected set { transitionInDuration = value; }
        }

        TimeSpan transitionOutDuration = TimeSpan.Zero;

        /// <summary>
        /// Duration for transitioning out
        /// </summary>
        public TimeSpan TransitionOutDuration
        {
            get { return transitionOutDuration; }
            protected set { transitionOutDuration = value; }
        }

        float transitionOffset = 0.0f;

        /// <summary>
        /// Float representing the offset of the current transition
        /// range [0 - 1]
        /// </summary>
        public float TransitionOffset
        {
            get { return transitionOffset; }
            set { transitionOffset = value; }
        }

        bool isExiting = false;

        /// <summary>
        /// Determines if after transitioning out
        /// the screen will be removed from the manager
        /// </summary>
        public bool IsExiting
        {
            get { return isExiting; }
            protected internal set { isExiting = value; }
        }

        /// <summary>
        /// Determines if the screen is currently
        /// in a transition
        /// </summary>
        public bool IsTransitioning
        {
            get
            {
                return (state == ScreenState.TransitionIn
                    || state == ScreenState.TransitionOut);
            }
        }

        bool otherScreenFocused;

        /// <summary>
        /// Determines if the screen is active and 
        /// will recive user input
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !otherScreenFocused &&
                       (state == ScreenState.TransitionIn ||
                        state == ScreenState.Active);
            }
        }

        ScreenManager screenManager;

        /// <summary>
        /// Screen's parent screen manager
        /// </summary>
        public ScreenManager ScreenManager
        {
            get { return screenManager; }
            internal set { screenManager = value; }
        }

        /// <summary>
        /// Screen manager's sprite batch
        /// </summary>
        protected SpriteBatch SpriteBatch
        {
            get { return screenManager.SpriteBatch; }
        }

        /// <summary>
        /// Game window viewport
        /// </summary>
        protected Viewport Viewport
        {
            get { return screenManager.GraphicsDevice.Viewport; }
            set { screenManager.GraphicsDevice.Viewport = value; }
        }

        /// <summary>
        /// Use when loading textures and other assets
        /// </summary>
        /// <param name="content"> Content manager to use when loading assets </param>
        public virtual void LoadContent(ContentManager content) { }

        /// <summary>
        /// Use when disposing textures and other assets
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Use when managing screen input. This will
        /// only be called on a screens with focus
        /// </summary>
        public virtual void HandleInput() { }

        /// <summary>
        /// Update screen logic
        /// </summary>
        /// <param name="otherScreenFocused"> Determines if other screens are focused </param>
        /// <param name="covered"> Determines if the screen is covered by other screens </param>
        public virtual void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            this.otherScreenFocused = otherScreenFocused;

            if (isExiting)
            {
                // transition out to die
                state = ScreenState.TransitionOut;

                if (!updateTransition(gameTime, transitionOutDuration, -1))
                {
                    // remove the screen
                    screenManager.RemoveScreen(this);
                }
            }
            else if (covered)
            {
                // transition out
                if (updateTransition(gameTime, transitionOutDuration, -1))
                {
                    // still transitioning
                    state = ScreenState.TransitionOut;
                }
                else
                {
                    // finished
                    state = ScreenState.Hidden;
                }
            }
            else
            {
                // transition in because it's active
                if (updateTransition(gameTime, transitionInDuration, 1))
                {
                    // still transitioning
                    state = ScreenState.TransitionIn;
                }
                else
                {
                    // transition finished
                    state = ScreenState.Active;
                }
            }
        }

        /// <summary>
        /// Render screen content. Only called if 
        /// screen is visible
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Transition out and remove the screen from the manager
        /// </summary>
        public void ExitScreen()
        {
            IsExiting = true;

            // no transition, remove it right away
            if (TransitionOutDuration == TimeSpan.Zero)
            {
                ScreenManager.RemoveScreen(this);
            }

            MarsRTS.SetCursor(Cursors.Default);
        }

        /// <summary>
        /// Helper method for updating the screen transition position.
        /// </summary>
        /// <returns> True if still transitioning, false if finished </returns>
        bool updateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta =
                    (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                          time.TotalMilliseconds);

            transitionOffset += transitionDelta * direction;

            // finished transitioning?
            if (transitionOffset <= 0 || transitionOffset >= 1)
            {
                // callback only if it's actually transitioning
                if (state == ScreenState.TransitionIn ||
                    state == ScreenState.TransitionOut)
                {
                    // transition callbacks
                    if (transitionOffset <= 0)
                    {
                        if (TransitionedIn != null)
                            TransitionedIn(this, EventArgs.Empty);
                    }
                    else
                    {
                        if (TransitionedOut != null)
                            TransitionedOut(this, EventArgs.Empty);
                    }
                }

                transitionOffset = MathHelper.Clamp(transitionOffset, 0, 1);

                return false;
            }

            // still transitioning
            return true;
        }
    }
}
