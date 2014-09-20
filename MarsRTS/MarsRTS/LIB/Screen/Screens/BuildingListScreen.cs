using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects;
using MarsRTS.LIB.GameObjects.Entities.Fixed;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.Screen.Screens.ScreenObjects;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSAssetData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsRTS.LIB.Screen.Screens
{
    class BuildingListScreen : GameScreen
    {
        Texture2D menuTexture;

        Vector2 menuPosition, targetMenuPosition;

        MarsWorld world;

        ButtonItem[] categoryButtons;

        Dictionary<BuildingCategory, List<BuildingIcon>> buildingIcons =
            new Dictionary<BuildingCategory, List<BuildingIcon>>();

        List<BuildingIcon> currentIcons = new List<BuildingIcon>();

        static BuildingCategory currentCategory = BuildingCategory.General;

        const int iconsPerRow = 3;
        const int maxRows = 2;
        const int iconPadding = 10;

        Vector2[] iconPositions;

        float transition;

        const float iconYOffset = 76;

        public BuildingListScreen(MarsWorld world)
        {
            this.world = world;

            TransitionInDuration = TransitionOutDuration =
                TimeSpan.FromMilliseconds(250);

            IsOverlay = true;
        }

        public override void LoadContent(ContentManager content)
        {
            menuTexture =
                content.Load<Texture2D>(@"HUD/Building/MenuList");

            init(content);
        }

        public override void HandleInput()
        {
            if (InputManager.IsKeyTriggered(Keys.Escape))
            {
                ExitScreen();
            }

            updateCategoryButtons();
            updateBuildingIcons();
        }

        public override void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            if (IsTransitioning)
            {
                transition = easeTransition(0, TransitionOffset);

                menuPosition.X = targetMenuPosition.X *
                    easeTransition(0.60f, TransitionOffset);
            }
            else if (TransitionOffset == 1)
            {
                transition = 1;
                menuPosition = targetMenuPosition;
            }

            base.Update(gameTime, otherScreenFocused, covered);
        }

        public override void Draw()
        {
            SpriteBatch.Begin();

            SpriteBatch.Draw(MarsRTS.BlankTexture,
                Viewport.Bounds,
                Viewport.Bounds,
                Color.Black * TransitionOffset * 0.75f);

            SpriteBatch.Draw(menuTexture,
                menuPosition,
                Color.White * TransitionOffset);

            drawCategoryButtons(SpriteBatch);

            drawBuildingIcons(SpriteBatch);

            SpriteBatch.End();
        }

        void openCategoryTab(BuildingCategory category)
        {
            for (int i = 0; i < categoryButtons.Length; i++)
            {
                categoryButtons[i].SetActive(i == (int)category);
            }

            currentIcons = buildingIcons[category];

            currentCategory = category;
        }

        void init(ContentManager content)
        {
            targetMenuPosition = Viewport.Bounds.Center.ToVector() -
                menuTexture.Bounds.Center.ToVector();

            menuPosition.Y = targetMenuPosition.Y;

            initCategories(content);
            initCategoryButtons();
        }

        void initCategories(ContentManager content)
        {
            var categories = Enum.GetValues(typeof(BuildingCategory));

            var buildings =
                Enum.GetValues(typeof(Building)).OfType<Building>().ToList();

            foreach (BuildingCategory category in categories)
            {
                var inCategory = (List<Building>)buildings
                    .Where(building =>
                        world.GetBuildingProperties(building).Category ==
                        category)
                    .ToList();

                buildingIcons[category] = new List<BuildingIcon>();

                foreach (var building in inCategory)
                {
                    var properties = world.GetBuildingProperties(building);

                    var texture =
                        content.Load<Texture2D>(properties.IconTexture);

                    var icon = new BuildingIcon(
                        texture,
                        properties.Description,
                        0,
                        4);

                    icon.OnClick += delegate
                    {
                        var purchase = world.CreateBuilding(building);

                        var buildingScreen =
                            new BuildingScreen(purchase, world, false);

                        buildingScreen.OnPurchase += delegate
                        {
                            ExitScreen();
                        };

                        ScreenManager.AddScreen(buildingScreen);
                    };

                    buildingIcons[category].Add(icon);
                }
            }

            var rowWidth = (BuildingIcon.Size.X + iconPadding) * iconsPerRow;
            var offsetX = (menuTexture.Width - rowWidth) / 2;

            iconPositions = new Vector2[maxRows * iconsPerRow];

            for (int i = 0; i < categories.Length; i++)
            {
                int row = i / iconsPerRow;

                iconPositions[i] = new Vector2()
                {
                    X = offsetX + (i * (BuildingIcon.Size.X + iconPadding)),
                    Y = iconYOffset +
                        (row * (BuildingIcon.Size.Y + iconPadding))
                };
            }
        }

        void initCategoryButtons()
        {
            var categories = Enum.GetValues(typeof(BuildingCategory));

            var categoryPadding = 5;

            var categoriesWidth = categoryPadding * categories.Length;

            for (int i = 0; i < categories.Length; i++)
            {
                categoriesWidth += ButtonItem.GetSize(Enum.GetName(
                    typeof(BuildingCategory), categories.GetValue(i))).X;
            }

            var buttonOffset = new Vector2()
            {
                X = (menuTexture.Width - categoriesWidth) / 2,
                Y = 11
            };

            categoryButtons = new ButtonItem[categories.Length];

            for (int i = 0; i < categories.Length; i++)
            {
                var category = (BuildingCategory)categories.GetValue(i);

                var categoryName =
                    Enum.GetName(typeof(BuildingCategory), category);

                var button = new ButtonItem(buttonOffset + targetMenuPosition,
                    categoryName, false);

                button.OnClick += delegate
                {
                    openCategoryTab(category);
                };

                categoryButtons[i] = button;

                buttonOffset.X +=
                    ButtonItem.GetSize(categoryName).X + categoryPadding;
            }

            categoryButtons[(int)currentCategory].SetActive(true);
            currentIcons = buildingIcons[currentCategory];
        }

        void updateCategoryButtons()
        {
            var mouse = InputManager.MousePosition;

            var cursor = Cursors.Default;

            foreach (var button in categoryButtons)
            {
                if (!button.Disabled && button.Bounds.Contains(mouse))
                {
                    if (InputManager.IsMouseButtonClicked(MouseButton.Left))
                    {
                        if (button.OnClick != null)
                            button.OnClick(null, EventArgs.Empty);

                        button.SetHovering(false);
                    }
                    else
                    {
                        button.SetHovering(true);

                        cursor = Cursors.Pointer;
                    }
                }
                else
                {
                    button.SetHovering(false);
                }
            }

            MarsRTS.SetCursor(cursor);
        }

        void drawCategoryButtons(SpriteBatch spriteBatch)
        {
            var offset = menuPosition - targetMenuPosition;

            foreach (var button in categoryButtons)
            {
                button.Draw(spriteBatch, offset, transition);
            }
        }

        void updateBuildingIcons()
        {
            var mouse = InputManager.MousePosition;
            var cursor = MarsRTS.Cursor;

            for (int i = 0; i < currentIcons.Count; i++)
            {
                var icon = currentIcons[i];

                var position = menuPosition + iconPositions[i];

                var bounds = new Rectangle()
                {
                    X = (int)position.X,
                    Y = (int)position.Y,
                    Width = BuildingIcon.Size.X,
                    Height = BuildingIcon.Size.Y,
                };

                if (bounds.Contains(mouse))
                {
                    if (InputManager.IsMouseButtonClicked(MouseButton.Left))
                    {
                        if (icon.OnClick != null)
                            currentIcons[i].OnClick(null, null);

                        return;
                    }

                    icon.SetHovering(true);
                    cursor = Cursors.Pointer;
                }
                else
                {
                    icon.SetHovering(false);
                }
            }

            MarsRTS.SetCursor(cursor);
        }

        void drawBuildingIcons(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < currentIcons.Count; i++)
            {
                currentIcons[i].Draw(spriteBatch,
                    menuPosition + iconPositions[i],
                    transition);
            }
        }

        /// <summary>
        /// Cubic ease function to apply to the current transition
        /// </summary>
        /// <param name="t"> Transition offset to interpolate </param>
        float easeTransition(float min, float time)
        {
            return MathHelper.SmoothStep(min, 1, time);
        }
    }
}
