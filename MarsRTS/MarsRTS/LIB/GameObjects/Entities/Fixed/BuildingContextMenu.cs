using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class BuildingContextMenu
    {
        Vector2 titleOffset, titlePosition, levelPosition;

        public Vector2 Position
        {
            get { return menuBounds.Location.ToVector(); }
        }

        Point buttonSize = new Point(0, 26);

        Rectangle menuBounds;

        List<ContextMenuItem> menuItems = new List<ContextMenuItem>();

        MarsWorld world;

        const int minMenuWidth = 145;
        const int menuPadding = 10;
        const int buttonPadding = 10;

        const int titleTextSize = 15;
        const int levelTextSize = 14;
        const int buttonTextSize = 12;

        readonly Color menuBackgroundColor = new Color(70, 70, 70) * 0.9f;
        readonly Color buttonBackgroundColor = new Color(49, 49, 49);
        readonly Color buttonBackgroundColorHover = new Color(97, 97, 97);
        readonly Color buttonBackgroundColorActive = new Color(31, 31, 31);

        readonly Color titleColor = Color.White;
        readonly Color levelColor = Color.White;
        readonly Color buttonTextColor = new Color(175, 175, 175);

        MenuState state = MenuState.Hidden;

        public MenuState State
        {
            get { return state; }
            set { state = value; }
        }

        TimeSpan transitionInDuration, transitionOutDuration;

        float transitionOffset, transition;

        string title, level;

        const string titleFont = "Franchise";
        const string levelFont = "Myriad";
        const string buttonFont = "Myriad";

        public bool IsTransitioning
        {
            get
            {
                return state == MenuState.TransitionIn ||
                    state == MenuState.TransitionOut;
            }
        }

        public BuildingContextMenu(MarsWorld world, ContentManager content, TimeSpan transitionInDuration, TimeSpan transitionOutDuration)
        {
            this.world = world;
            this.transitionInDuration = transitionInDuration;
            this.transitionOutDuration = transitionOutDuration;
        }

        public void Open(BuildingEntity building)
        {
            state = MenuState.Hidden;

            menuItems = building.GetContextMenuItems();

            if (menuItems == null)
                return;

            state = MenuState.TransitionIn;

            var properties = building.Properties;

            title = properties.Description;
            level = string.Format("Level {0}", properties.Level + 1);

            var titleSize = Font.Measure(titleFont, title, titleTextSize);
            var levelSize = Font.Measure(levelFont, level, levelTextSize);

            menuBounds.Width = (int)Math.Max(titleSize.X + 12, minMenuWidth);

            titlePosition = new Vector2()
            {
                X = (int)(menuBounds.Width - titleSize.X) / 2,
                Y = 10
            };

            levelPosition = new Vector2()
            {
                X = (int)(menuBounds.Width - levelSize.X) / 2,
                Y = (int)(titlePosition.Y + titleSize.Y) + 5
            };

            titleOffset = new Vector2()
            {
                X = (int)(menuBounds.Width - titleSize.X) / 2,
                Y = (int)(titlePosition.Y + titleSize.Y +
                    levelSize.Y + menuPadding)
            };

            buttonSize.X = menuBounds.Width - (2 * buttonPadding);

            for (int i = 0; i < menuItems.Count; i++)
            {
                var item = menuItems[i];

                var size = Font.Measure(buttonFont, item.Text, buttonTextSize);

                item.ButtonPosition = new Vector2()
                {
                    X = buttonPadding,
                    Y = i * (buttonSize.Y + 5) + titleOffset.Y
                };

                item.TextPosition = new Vector2()
                {
                    X = (int)(item.ButtonPosition.X +
                        ((buttonSize.X - size.X) / 2)),
                    Y = (int)(item.ButtonPosition.Y +
                        ((buttonSize.Y - size.Y) / 2)),
                };

                item.Size = buttonSize;
            }

            menuBounds.Height =
                (int)menuItems[menuItems.Count - 1].ButtonPosition.Y +
                    buttonSize.Y + menuPadding;

            var menuSize = new Vector2(menuBounds.Width, menuBounds.Height);

            var offset = InputManager.MousePosition.ToVector();

            menuBounds.Location = (offset - world.Camera.RawPosition -
                (menuSize / 2)).ToPoint();
        }

        public void Close(bool immediate = false)
        {
            if (immediate)
            {
                state = MenuState.Hidden;
                transitionOffset = 0;
            }
            else if (state != MenuState.Hidden)
            {
                state = MenuState.TransitionOut;
            }

            MarsRTS.SetCursor(Cursors.Default);
        }

        public void Update(GameTime gameTime)
        {
            if (state == MenuState.Hidden)
                return;

            updateTransition(gameTime);

            var mouse = InputManager.MousePosition;

            if (!getMenuBounds(world.Camera.RawPosition).Contains(mouse))
            {
                Close();

                return;
            }

            bool mouseDown =
                InputManager.IsMouseButtonPressed(MouseButton.Left);

            var cursor = Cursors.Default;

            foreach (var item in menuItems)
            {
                var offset = (menuBounds.Location.ToVector() +
                    world.Camera.RawPosition).ToPoint();

                var bounds = item.GetBounds(offset);

                if (bounds.Contains(mouse))
                {
                    if (mouseDown)
                        item.SetState(ButtonState.Active);
                    else
                        item.SetState(ButtonState.Hover);

                    cursor = Cursors.Pointer;
                }
                else
                {
                    item.SetState(ButtonState.None);
                }
            }

            MarsRTS.SetCursor(cursor);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // don't want to draw if the menu isn't open
            if (state == MenuState.Hidden)
                return;

            var offset = menuBounds.Location.ToVector() +
                world.Camera.RawPosition;

            // background
            spriteBatch.Draw(MarsRTS.BlankTexture,
                getMenuBounds(world.Camera.RawPosition),
                MarsRTS.BlankTexture.Bounds,
                menuBackgroundColor * transition);

            Font.Draw(spriteBatch,
                titleFont,
                title,
                titlePosition + offset,
                scaledHeight(titleTextSize),
                titleColor * transition);

            Font.Draw(spriteBatch,
                levelFont,
                level,
                levelPosition + offset,
                scaledHeight(levelTextSize),
                levelColor * transition);

            var white = Color.White * transition;

            // buttons
            foreach (var item in menuItems)
            {
                Color color = Color.White;

                switch (item.State)
                {
                    case ButtonState.None:
                        color = buttonBackgroundColor;
                        break;
                    case ButtonState.Hover:
                        color = buttonBackgroundColorHover;
                        break;
                    case ButtonState.Active:
                        color = buttonBackgroundColorActive;
                        break;
                }

                drawRectangle(spriteBatch,
                    item.GetBounds(offset.ToPoint()),
                    color * transition);

                Font.Draw(spriteBatch,
                    buttonFont,
                    item.Text,
                    item.TextPosition + offset,
                    buttonTextSize,
                    buttonTextColor * transition);
            }
        }

        public void OnClick(Vector2 location)
        {
            var point = location.ToPoint();

            foreach (var item in menuItems)
            {
                if (item.GetBounds(menuBounds.Location).Contains(point))
                {
                    item.SetState(ButtonState.Active);

                    MarsRTS.SetCursor(Cursors.Default);

                    if (item.OnClick != null)
                    {
                        item.OnClick(item.Tag, EventArgs.Empty);
                    }

                    Close(true);

                    return;
                }
            }
        }

        void updateTransition(GameTime gameTime)
        {
            if (!IsTransitioning)
                return;

            int direction;

            if (state == MenuState.TransitionIn)
                direction = 1;
            else
                direction = -1;

            TimeSpan time;

            if (direction == 1)
                time = transitionInDuration;
            else
                time = transitionOutDuration;

            float delta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                          time.TotalMilliseconds);

            transitionOffset += delta * direction;

            transition = easeTransition(transitionOffset);

            if (transitionOffset <= 0 || transitionOffset >= 1)
            {
                transitionOffset = MathHelper.Clamp(transitionOffset, 0, 1);

                if (state == MenuState.TransitionIn)
                {
                    state = MenuState.Active;
                }
                else
                {
                    state = MenuState.Hidden;
                }
            }
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture, bounds, color);
        }

        float easeTransition(float t)
        {
            return t < .5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
        }

        Vector2 scaledHeight(float size)
        {
            return new Vector2(size, size * transition);
        }

        Vector2 scaledHeight(float width, float height)
        {
            return new Vector2(width, height * transition);
        }

        Rectangle getMenuBounds(Vector2 offset)
        {
            return new Rectangle()
            {
                X = menuBounds.X + (int)offset.X,
                Y = menuBounds.Y + (int)offset.Y,
                Width = menuBounds.Width,
                Height = menuBounds.Height
            };
        }
    }
}
