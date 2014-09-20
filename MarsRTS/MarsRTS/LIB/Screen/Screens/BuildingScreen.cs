using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects;
using MarsRTS.LIB.GameObjects.Entities.Fixed;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.Screen.Screens.ScreenObjects;
using MarsRTS.LIB.Screen.Screens.ScreenObjects.Upgrade;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSAssetData.Buildings;
using System;
using System.Collections.Generic;

namespace MarsRTS.LIB.Screen.Screens
{
    public class BuildingScreen : GameScreen
    {
        EventHandler onCancel, onPurchase;

        public EventHandler OnCancel
        {
            get { return onCancel; }
            set { onCancel = value; }
        }

        public EventHandler OnPurchase
        {
            get { return onPurchase; }
            set { onPurchase = value; }
        }

        Texture2D menuTexure;

        Texture2D[] resourceIcons, affordableIcons;

        Vector2 menuPosition, targetMenuPosition, titleOffset;

        readonly Vector2 headingOffset = new Vector2(12, 5);
        readonly Vector2 infoOffset = new Vector2(18, 241);

        readonly Point resourceBoxSize = new Point(126, 126);

        ResourceBox[] resourceBoxes;

        List<BuildingStatDisplay> stats;

        ButtonItem[] buttons;

        readonly Color backgroundColor = Color.Black * 0.75f;
        readonly Color headingColor = new Color(175, 175, 175);
        readonly Color titleColor = new Color(204, 204, 204);
        readonly Color descriptionColor = Color.White;

        DisplayType displayType;

        BuildingEntity building;

        MarsWorld world;

        int[] statWidths;

        int targetLevel;

        const int headingFontSize = 27;
        const int titleFontSize = 19;
        const int descriptionFontSize = 14;
        const int descriptionLineWidth = 467;

        string heading, title, descriptionText;

        readonly string okText;

        const string headingFont = "Franchise";
        const string titleFont = "Franchise";
        const string descriptionFont = "Myriad";

        float transition;

        bool isUpgrade, purchaseAffordable = true;

        public BuildingScreen(BuildingEntity building, MarsWorld world, bool isUpgrade)
        {
            this.building = building;
            this.world = world;
            this.isUpgrade = isUpgrade;

            var properties = building.Properties;

            if (isUpgrade)
            {
                heading = "UPGRADE BUILDING";
                title = string.Format("{0} level {1}",
                    properties.Description, properties.Level + 1);

                okText = "Upgrade";

                targetLevel = properties.Level + 1;
            }
            else
            {
                heading = "PURCHASE BUILDING";
                title = properties.Description;
                okText = "Purchase";

                targetLevel = 0;
            }

            IsOverlay = true;

            TransitionInDuration = TransitionOutDuration =
                TimeSpan.FromMilliseconds(250);
        }

        public override void LoadContent(ContentManager content)
        {
            string iconsPath = @"HUD/Icons/";

            menuTexure = content.Load<Texture2D>(
                @"HUD/Building/MenuPurchase");

            resourceIcons = new Texture2D[3] {
                content.Load<Texture2D>(iconsPath + "ResourceCopper"),
                content.Load<Texture2D>(iconsPath + "ResourceIron"),
                content.Load<Texture2D>(iconsPath + "ResourceNickel")
            };

            affordableIcons = new Texture2D[2] {
                content.Load<Texture2D>(iconsPath + "IconNotAffordable"),
                content.Load<Texture2D>(iconsPath + "IconAffordable")
            };

            init();
        }

        public override void HandleInput()
        {
            if (InputManager.IsKeyTriggered(Keys.Escape))
            {
                ExitScreen();
            }

            updateButtons();
        }

        public override void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            base.Update(gameTime, otherScreenFocused, covered);

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
        }

        public override void Draw()
        {
            SpriteBatch.Begin();

            SpriteBatch.Draw(MarsRTS.BlankTexture,
                Viewport.Bounds,
                Viewport.Bounds,
                backgroundColor * transition);

            SpriteBatch.Draw(menuTexure,
                menuPosition,
                Color.White * transition);

            Font.Draw(SpriteBatch,
                headingFont,
                heading,
                menuPosition + headingOffset,
                headingFontSize,
                headingColor * transition);

            drawResourceBoxes();

            Font.Draw(SpriteBatch,
                titleFont,
                title,
                menuPosition + titleOffset,
                titleFontSize,
                titleColor * transition);

            drawUpgradeInfo();

            var buttonOffset = menuPosition - targetMenuPosition;

            foreach (var button in buttons)
            {
                button.Draw(SpriteBatch, buttonOffset, transition);
            }

            SpriteBatch.End();
        }

        void init()
        {
            var titleSize = Font.Measure(titleFont, title, titleFontSize);

            targetMenuPosition = Viewport.Bounds.Center.ToVector() -
                menuTexure.Bounds.Center.ToVector();

            menuPosition.Y = targetMenuPosition.Y;

            titleOffset = new Vector2()
            {
                X = menuTexure.Width - titleSize.X - 15,
                Y = 12
            };

            initDisplayType();
            initResourceBoxes();
            initButtons();
        }

        void initDisplayType()
        {
            var displayData = building.GetDisplayData(targetLevel);

            if (displayData is string)
            {
                displayType = DisplayType.Description;

                descriptionText = Font.WordWrap(descriptionFont,
                    (string)displayData,
                    descriptionFontSize,
                    descriptionLineWidth);
            }
            else if (displayData is List<BuildingStatDisplay>)
            {
                displayType = DisplayType.Stats;

                var statSize = new Vector2(0, 60);

                stats = (List<BuildingStatDisplay>)displayData;

                for (int i = 0; i < stats.Count; i++)
                {
                    stats[i].Position = infoOffset + (i * statSize);
                }
            }
        }

        void initResourceBoxes()
        {
            var resources = Enum.GetValues(typeof(ResourceType));

            var spacing = (menuTexure.Width -
                (resourceBoxSize.X * resources.Length)) / resources.Length;

            int offsetTop = 83;

            resourceBoxes = new ResourceBox[resources.Length];

            var costs = building.Properties.GetUpgradeCost(targetLevel);

            for (int i = 0; i < resources.Length; i++)
            {
                var resource = (ResourceType)resources.GetValue(i);

                int total = world.Bank.GetTotal(resource);

                int cost = costs.FromType(resource);

                var bounds = new Rectangle()
                {
                    X = (i * (spacing + resourceBoxSize.X)) + (spacing / 2),
                    Y = offsetTop,
                    Width = resourceBoxSize.X,
                    Height = resourceBoxSize.Y
                };

                bool affordable = total >= cost;

                if (!affordable)
                    purchaseAffordable = false;

                resourceBoxes[i] = new ResourceBox(resourceIcons[i],
                    affordableIcons[affordable ? 1 : 0],
                    bounds,
                    Enum.GetName(resource.GetType(), resource),
                    cost,
                    total);
            }
        }

        void initButtons()
        {
            var cancelSize = ButtonItem.GetSize("Cancel");
            var okSize = ButtonItem.GetSize(okText);

            var menuPadding = new Vector2(10);
            var shadowOffset = new Vector2(2, 3);

            var okPosition = new Vector2()
            {
                X = (targetMenuPosition.X + menuTexure.Width) -
                    okSize.X - shadowOffset.X,
                Y = (targetMenuPosition.Y + menuTexure.Height) -
                    okSize.Y - shadowOffset.Y
            };

            okPosition -= menuPadding;

            var cancelPosition = new Vector2()
            {
                X = okPosition.X - okSize.X - 5,
                Y = okPosition.Y
            };

            buttons = new ButtonItem[2] {
                new ButtonItem(cancelPosition, "Cancel", false),
                new ButtonItem(okPosition, okText, !purchaseAffordable)
            };

            // close
            buttons[0].OnClick += delegate
            {
                if (onCancel != null)
                    onCancel(null, null);

                ExitScreen();
            };

            // ok
            buttons[1].OnClick += delegate
            {
                if (isUpgrade)
                {
                    building.Upgrade();
                }
                else
                {
                    world.PurchaseBuilding(building);

                    world.AddEntity(building);
                    world.SetDragging(building);
                }

                if (onPurchase != null)
                    onPurchase(null, null);

                ExitScreen();
            };
        }

        void drawResourceBoxes()
        {
            foreach (var box in resourceBoxes)
            {
                box.Draw(SpriteBatch, menuPosition, transition);
            }
        }

        void drawUpgradeInfo()
        {
            if (displayType == DisplayType.Description)
            {
                Font.Draw(SpriteBatch,
                    descriptionFont,
                    descriptionText,
                    menuPosition + infoOffset,
                    descriptionFontSize,
                    descriptionColor * transition);
            }
            else if (displayType == DisplayType.Stats)
            {
                foreach (var stat in stats)
                {
                    stat.Draw(SpriteBatch, menuPosition, transition);
                }
            }
        }

        void drawRectangle(Rectangle bounds, Color color)
        {
            SpriteBatch.Draw(MarsRTS.BlankTexture, bounds, color);
        }

        void updateButtons()
        {
            var mouse = InputManager.MousePosition;

            var cursor = Cursors.Default;

            foreach (var button in buttons)
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
