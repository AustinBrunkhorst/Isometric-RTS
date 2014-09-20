using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MarsRTS.LIB.Input
{
    public static class InputManager
    {
        /// <summary>
        /// Minimum threshold for states pertaining to float values
        /// </summary>
        const float pressThreshold = 0.5f;

        static KeyboardState keyState, lastKeyState;

        /// <summary>
        /// Current keyboard state
        /// </summary>
        public static KeyboardState KeyboardState
        {
            get { return keyState; }
        }

        static GamePadState padState, lastPadState;

        /// <summary>
        /// Current gamepad state
        /// </summary>
        public static GamePadState GamePadState
        {
            get { return padState; }
        }

        static MouseState mouseState, lastMouseState;

        /// <summary>
        /// Current mouse state
        /// </summary>
        public static MouseState MouseState
        {
            get { return mouseState; }
        }

        /// <summary>
        /// Current mouse position
        /// </summary>
        public static Point MousePosition
        {
            get { return new Point(mouseState.X, mouseState.Y); }
        }

        /// <summary>
        /// Mouse position from last update
        /// </summary>
        public static Point LastMousePosition
        {
            get { return new Point(lastMouseState.X, lastMouseState.Y); }
        }

        /// <summary>
        /// Determines if the key is pressed
        /// </summary>
        /// <param name="key"> Key to check </param>
        public static bool IsKeyPressed(Keys key)
        {
            return keyState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines if the key is triggered
        /// </summary>
        /// <param name="key"> Key to check </param>
        public static bool IsKeyTriggered(Keys key)
        {
            return keyState.IsKeyDown(key) && !lastKeyState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines if the gamepad button is pressed
        /// </summary>
        /// <param name="button"> Gamepad button to check </param>
        public static bool IsGamePadButtonPressed(GamePadButton button)
        {
            return isPadStatePressed(padState, button);
        }

        /// <summary>
        /// Determines if the gamepad button is triggered
        /// </summary>
        /// <param name="button"> Gamepad button to check </param>
        public static bool IsGamePadButtonTriggered(GamePadButton button)
        {
            return isPadStatePressed(padState, button) &&
                !isPadStatePressed(lastPadState, button);
        }

        /// <summary>
        /// Determines if the mouse button is pressed
        /// </summary>
        /// <param name="button"> Mouse button to check </param>
        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return isMouseStatePressed(mouseState, button);
        }

        /// <summary>
        /// Determines if the mouse button is triggered
        /// </summary>
        /// <param name="button"> Mouse button to check </param>
        public static bool IsMouseButtonTriggered(MouseButton button)
        {
            return isMouseStatePressed(mouseState, button) &&
                !isMouseStatePressed(lastMouseState, button);
        }

        /// <summary>
        /// Determines if a mouse button is click (on mouse up)
        /// </summary>
        /// <param name="button"> Mouse button to check </param>
        public static bool IsMouseButtonClicked(MouseButton button)
        {
            return isMouseStatePressed(lastMouseState, button) &&
                !isMouseStatePressed(mouseState, button);
        }

        /// <summary>
        /// Update the input states
        /// </summary>
        public static void Update()
        {
            lastKeyState = keyState;
            keyState = Keyboard.GetState();

            lastPadState = padState;
            padState = GamePad.GetState(PlayerIndex.One);

            lastMouseState = mouseState;
            mouseState = Mouse.GetState();
        }

        /// <summary>
        /// Helper function for determining if the button is pressed with
        /// the given gamepad state
        /// </summary>
        static bool isPadStatePressed(GamePadState state, GamePadButton button)
        {
            switch (button)
            {
                case GamePadButton.Start:
                    return isButtonPressed(state.Buttons.Start);

                case GamePadButton.Back:
                    return isButtonPressed(state.Buttons.Back);

                case GamePadButton.A:
                    return isButtonPressed(state.Buttons.A);

                case GamePadButton.B:
                    return isButtonPressed(state.Buttons.B);

                case GamePadButton.X:
                    return isButtonPressed(state.Buttons.X);

                case GamePadButton.Y:
                    return isButtonPressed(state.Buttons.Y);

                case GamePadButton.LeftShoulder:
                    return isButtonPressed(state.Buttons.LeftShoulder);

                case GamePadButton.RightShoulder:
                    return isButtonPressed(state.Buttons.RightShoulder);

                case GamePadButton.LeftTrigger:
                    return inThreshold(state.Triggers.Left);

                case GamePadButton.RightTrigger:
                    return inThreshold(state.Triggers.Right);

                case GamePadButton.Up:
                    return isButtonPressed(state.DPad.Up) ||
                        inThreshold(state.ThumbSticks.Left.Y);

                case GamePadButton.Down:
                    return isButtonPressed(state.DPad.Down) ||
                        inThreshold(-state.ThumbSticks.Left.Y);

                case GamePadButton.Left:
                    return isButtonPressed(state.DPad.Left) ||
                        inThreshold(-state.ThumbSticks.Left.X);

                case GamePadButton.Right:
                    return isButtonPressed(state.DPad.Right) ||
                        inThreshold(state.ThumbSticks.Left.X);
            }

            return false;
        }

        /// <summary>
        /// Helper function for determining if the button is pressed with
        /// the given mouse state
        /// </summary>
        static bool isMouseStatePressed(MouseState state, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return isButtonPressed(state.LeftButton);

                case MouseButton.Middle:
                    return isButtonPressed(state.MiddleButton);

                case MouseButton.Right:
                    return isButtonPressed(state.RightButton);

                case MouseButton.X1:
                    return isButtonPressed(state.XButton1);

                case MouseButton.X2:
                    return isButtonPressed(state.XButton2);
            }

            return false;
        }

        /// <summary>
        /// Helper function for determining if the given button state
        /// is pressed
        /// </summary>
        static bool isButtonPressed(ButtonState state)
        {
            return (state == ButtonState.Pressed);
        }

        /// <summary>
        /// Helper function for determining if the given value is within the 
        /// constant threshold
        /// </summary>
        static bool inThreshold(float value)
        {
            return (value > pressThreshold);
        }
    }
}

