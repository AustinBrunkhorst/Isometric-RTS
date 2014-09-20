using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects;
using MarsRTS.LIB.GameObjects.Entities;
using MarsRTS.LIB.GameObjects.Entities.Fixed;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.Screen.Screens.ScreenObjects.Layout;
using MarsRTS.LIB.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSAssetData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsRTS.LIB.Screen.Screens
{
    class BaseLayoutManager : GameScreen
    {
        /// <summary>
        /// Placard background texture
        /// </summary>
        Texture2D placardTexture;

        /// <summary>
        /// Background texture for the menu
        /// </summary>
        Texture2D menuBackgroundTexture;

        /// <summary>
        /// Repeatable grid line texture
        /// </summary>
        Texture2D gridTexture;

        /// <summary>
        /// Node border texture. Slightly darker than white to make a nice
        /// contrast with the color drawn for each building
        /// </summary>
        Texture2D nodeBorderTexture;

        /// <summary>
        /// Main font
        /// </summary>
        SpriteFont font;

        /// <summary>
        /// Sound played when a placement is invalid
        /// </summary>
        SoundEffect invalidPlacementSound;

        /// <summary>
        /// Base size of the manager screen
        /// </summary>
        readonly Vector2 boxSize = new Vector2(800, 600);

        /// <summary>
        /// Position to draw the grid viewport relative to "boxOffset"
        /// </summary>
        readonly Vector2 gridOffset = new Vector2(227, 60);

        /// <summary>
        /// Size of the grid viewport
        /// </summary>
        readonly Point gridRectSize = new Point(550, 495);

        /// <summary>
        /// Base size of each grid node drawn
        /// </summary>
        readonly Point gridNodeSize = new Point(28, 28);

        /// <summary>
        /// Size of the exit button
        /// </summary>
        readonly Point exitButtonSize = new Point(64, 28);

        /// <summary>
        /// Rectangle representing the bounds of the layout grid in pixels
        /// </summary>
        readonly Rectangle gridBounds;

        /// <summary>
        /// Location of the the count circle relative to the placard texture
        /// </summary>
        readonly Vector2 placardCountOffset = new Vector2(151, 8);

        /// <summary>
        /// Square size of the placard count circle
        /// </summary>
        readonly Vector2 placardCountSize = new Vector2(33);

        /// <summary>
        /// Offset for "boxSize" to be drawn (center screen)
        /// </summary>
        Vector2 boxOffset;

        /// <summary>
        /// Target box offset. Used for transitioning position
        /// </summary>
        Vector2 targetBoxOffset;

        /// <summary>
        /// Viewport offset position for the node grid. static because we want
        /// to save it when the screen is closed
        /// </summary>
        static Vector2 gridCamera;

        const float placardCountFontScale = 1.10f;

        const int nodeBorderSize = 1;

        const int nodeClickDistanceThreshold = 5;

        Point lastMouseDown, mouseNodeLocation;

        Rectangle targetViewportBounds;

        Viewport defaultViewport, gridViewport;

        MarsWorld world;

        Dictionary<Building, List<BuildingEntity>> buildings =
            new Dictionary<Building, List<BuildingEntity>>();

        BuildingEntity draggedBuilding;

        Rectangle draggedBuildingBounds;

        List<InventoryPlacard> placards = new List<InventoryPlacard>();

        readonly Color backgroundColor = Color.Black * 0.75f;
        readonly Color gridBackgroundColor = new Color(239, 242, 185);
        readonly Color placardTextColor = new Color(103, 103, 103);
        readonly Color placardCountColor = Color.White;

        /// <summary>
        /// Determines if the mouse was inside the grid viewport last time
        /// it was pressed
        /// </summary>
        bool wasMouseInGridLastPressed;

        /// <summary>
        /// Determines if we should draw the mouse node location
        /// </summary>
        bool shouldDrawNodeLocation;

        float transition;

        public BaseLayoutManager(MarsWorld world)
        {
            this.world = world;

            gridBounds = new Rectangle()
            {
                Width = MarsWorld.Size.X * gridNodeSize.X,
                Height = MarsWorld.Size.Y * gridNodeSize.Y
            };

            IsOverlay = true;

            TransitionInDuration =
                TransitionOutDuration = TimeSpan.FromMilliseconds(250);
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            menuBackgroundTexture =
                content.Load<Texture2D>(@"HUD/LayoutManager/MenuBackground");

            gridTexture =
                content.Load<Texture2D>(@"HUD/Entities/LayoutGridNode");

            font =
                content.Load<SpriteFont>(@"Fonts/MainFont");

            placardTexture =
                content.Load<Texture2D>(@"HUD/Entities/BuildingPlacard");

            nodeBorderTexture =
                content.Load<Texture2D>(@"HUD/Entities/NodeBorder");

            invalidPlacementSound =
                content.Load<SoundEffect>(@"Audio/Effects/InvalidPlacement");

            init();
        }

        public override void HandleInput()
        {
            if (InputManager.IsKeyTriggered(Keys.Escape))
            {
                // reset the dragged building if it exists
                if (draggedBuilding != null)
                {
                    resetDraggedBuilding();
                }

                ExitScreen();
            }

            // current mouse position
            var mouse = InputManager.MousePosition;

            // mouse position at last update
            var lastMouse = InputManager.LastMousePosition;

            // determines if the mouse is inside the grid bounds
            shouldDrawNodeLocation = gridViewport.Bounds.Contains(mouse);

            // mouse position converted to a grid node index
            mouseNodeLocation = getGridMouseNodeIndex(mouse);

            // determine if the mouse node has changed since the last update
            if (mouseNodeLocation != getGridMouseNodeIndex(lastMouse))
            {
                // invoke the mouse node changed callback if the mouse is
                // actually inside the grid bounds
                if (shouldDrawNodeLocation)
                    onMouseNodeChanged(mouseNodeLocation);
            }

            var exitButton = new Rectangle()
            {
                X = (int)(boxOffset.X + boxSize.X) - exitButtonSize.X,
                Y = (int)boxOffset.Y,
                Width = exitButtonSize.X,
                Height = exitButtonSize.Y
            };

            bool mouseInExit = exitButton.Contains(mouse);

            if (mouseInExit)
            {
                MarsRTS.SetCursor(Cursors.Pointer);
            }
            else
            {
                MarsRTS.SetCursor(Cursors.Default);
            }

            if (InputManager.IsMouseButtonPressed(MouseButton.Left))
            {
                // initially pressed, set last mouse down to the mouse's
                // current position
                if (InputManager.IsMouseButtonTriggered(MouseButton.Left))
                {
                    lastMouseDown = mouse;

                    wasMouseInGridLastPressed = shouldDrawNodeLocation;
                }

                // drag only if the last pressed mouse location was inside
                // the grid viewport
                if (wasMouseInGridLastPressed)
                    dragGrid(mouse, lastMouse);
            }
            else if (InputManager.IsMouseButtonClicked(MouseButton.Left))
            {
                if (mouseInExit)
                {
                    if (draggedBuilding != null)
                    {
                        resetDraggedBuilding();
                    }

                    ExitScreen();
                }

                if (shouldDrawNodeLocation)
                {
                    // distance between the mouse's current position and
                    // when it was last pressed
                    var distance = Vector2.Distance(
                        mouse.ToVector(),
                        lastMouseDown.ToVector());

                    // can we detect a click?
                    if (distance <= nodeClickDistanceThreshold)
                    {
                        // we aren't dragging a building, try and detect
                        // a building where we clicked.
                        if (draggedBuilding == null)
                        {
                            var point = getGridMousePosition(mouse);

                            point.X += (int)gridCamera.X;
                            point.Y += (int)gridCamera.Y;

                            // find a building at the mouse position
                            var building = getBuildingFromPoint(point);

                            // start dragging this building if it's found
                            if (building != null)
                                setDraggedBuilding(building);
                        }
                        else
                        {
                            // we're already dragging a building - place it
                            placeDraggedBuilding();
                        }
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenFocused, bool covered)
        {
            base.Update(gameTime, otherScreenFocused, covered);

            // dp some easing if we are transitioning
            if (IsTransitioning)
            {
                // ease the transition
                transition = easeTransition(0, TransitionOffset);

                // offset for slide elements
                var slideOffset = easeTransition(0.60f, TransitionOffset);

                // ease the box offset
                boxOffset.X =
                    (int)Math.Round(slideOffset * targetBoxOffset.X);

                // ease the viewport offset
                gridViewport.X =
                    (int)Math.Round(slideOffset * targetViewportBounds.X);
            }
            else if (TransitionOffset == 1)
            {
                transition = 1;

                boxOffset = targetBoxOffset;
                gridViewport.Bounds = targetViewportBounds;
            }
        }

        public override void Draw()
        {
            SpriteBatch.Begin();

            // draw overlay background
            drawRectangle(Viewport.Bounds, backgroundColor * transition);

            // menuback
            SpriteBatch.Draw(menuBackgroundTexture,
                boxOffset,
                Color.White * transition);

            // draw our building placards
            drawPlacards();

            if (draggedBuilding != null)
                drawDraggedBuildingText();

            if (shouldDrawNodeLocation)
                drawNodeLocation();

            SpriteBatch.End();

            // set the viewport to the grid viewport - this will allow us 
            // to draw the grid clipped by the grid rectangle.
            Viewport = gridViewport;

            // SamplerState.LinearWrap allows us to wrap the grid line texture
            // inside the grid rectangle
            SpriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearWrap,
                null,
                null);

            // draw the grid lines and buildings
            drawGrid();

            // if we are dragging a building, draw it
            if (draggedBuilding != null)
                drawDraggingBuilding();

            SpriteBatch.End();

            // reset the viewport back to the default window viewport
            Viewport = defaultViewport;
        }

        void drawPlacards()
        {
            // draw all placards
            foreach (var placard in placards)
            {
                var position = placard.Position + boxOffset;

                // placard background
                SpriteBatch.Draw(placardTexture,
                    position,
                    placard.Color * transition);

                // building description
                SpriteBatch.DrawString(font,
                    placard.Text,
                    position + placard.TextOffset,
                    placardTextColor * transition);

                // total members of this building group
                string count = placard.Members.ToString();

                // calculate the draw text size of the count text
                var countSize = font.MeasureString(count) *
                    placardCountFontScale;

                // center count on the placard texture circle
                var countOffset = (placardCountSize - countSize) / 2 +
                    new Vector2(-1, 1);

                // draw the building member count
                SpriteBatch.DrawString(font,
                    count,
                    position + placardCountOffset + countOffset,
                    placardCountColor * transition,
                    0,
                    Vector2.Zero,
                    placardCountFontScale,
                    SpriteEffects.None,
                    0);
            }
        }

        /// <summary>
        /// Draws the grid lines and buildings
        /// </summary>
        void drawGrid()
        {
            // draw grid lines
            SpriteBatch.Draw(gridTexture,
                gridCamera,
                gridBounds,
                Color.White * transition);

            // iterate through groups and their buildings, and draw them
            foreach (var group in buildings)
            {
                foreach (var building in group.Value)
                {
                    // we don't want to draw it if it's being dragged
                    if (building.IsDragging)
                        continue;

                    // building properties
                    var properties = building.Properties;

                    // note - the x and y bounds values are flipped
                    var destination =
                        getAbsoluteGridBounds(properties.GridBounds);

                    // transition color
                    var color = properties.InventoryColor * transition;

                    // draw building
                    drawRectangle(destination, color);
                }
            }
        }

        /// <summary>
        /// Draws the building being dragged
        /// </summary>
        void drawDraggingBuilding()
        {
            // dragged building rectangle bounds
            var bounds = new Rectangle()
            {
                X = draggedBuildingBounds.X + (int)gridCamera.X,
                Y = draggedBuildingBounds.Y + (int)gridCamera.Y,
                Width = draggedBuildingBounds.Width,
                Height = draggedBuildingBounds.Height,
            };

            // use the smaller number between the transition and 65%
            var opacity = Math.Min(transition, 0.65f);

            var inventoryColor = draggedBuilding.Properties.InventoryColor;

            Color color;

            // if the building is valid, use its inventory color. If not,
            // interpolate to be a tint of red
            if (draggedBuilding.IsPlacementValid)
                color = inventoryColor;
            else
                color = Color.Lerp(inventoryColor, Color.Red, 0.25f);

            // draw the bounds of the dragged building
            drawRectangle(bounds, color * opacity);
        }

        /// <summary>
        /// Draws info text for the currently dragged building
        /// </summary>
        void drawDraggedBuildingText()
        {
            var properties = draggedBuilding.Properties;

            var message = properties.Description +
                " level " + (properties.Level + 1);

            var messageSize = font.MeasureString(message);

            var location = new Vector2()
            {
                X = gridViewport.Bounds.Right - messageSize.X,
                Y = gridViewport.Bounds.Bottom + 10
            };

            SpriteBatch.DrawString(font,
                message,
                location,
                Color.White * transition);
        }

        /// <summary>
        /// Draws the mouse node location
        /// </summary>
        void drawNodeLocation()
        {
            var text = String.Format("{0}, {1}", mouseNodeLocation.X,
                mouseNodeLocation.Y);

            var location = new Vector2()
            {
                X = gridViewport.Bounds.X,
                Y = gridViewport.Bounds.Bottom + 10
            };

            SpriteBatch.DrawString(font,
                text,
                location,
                Color.White * transition);
        }

        /// <summary>
        /// Draws a rectangle on screen
        /// </summary>
        /// <param name="bounds"> Rectangle bounds </param>
        /// <param name="color"> Rectangle color </param>
        /// <param name="border"> Whether or not to draw a border around the 
        /// rectangle </param>
        void drawRectangle(Rectangle bounds, Color color, bool border = true)
        {
            SpriteBatch.Draw(MarsRTS.BlankTexture, bounds, color);

            if (border)
                drawBorder(bounds, color);
        }

        /// <summary>
        /// Draws a border around a rectangle
        /// </summary>
        /// <param name="bounds"> Target rectangle </param>
        /// <param name="color"> Border color </param>
        void drawBorder(Rectangle bounds, Color color)
        {
            Rectangle[] borders = new Rectangle[]
            {
                new Rectangle() { // top
                    X = bounds.X - 1,
                    Y = bounds.Y - 1,
                    Width = bounds.Width,
                    Height = nodeBorderSize
                },
                new Rectangle() { // right
                    X = bounds.Right - nodeBorderSize,
                    Y = bounds.Y - 1,
                    Width = nodeBorderSize,
                    Height = bounds.Height
                },
                new Rectangle() { // bottom
                    X = bounds.X,
                    Y = bounds.Bottom - nodeBorderSize,
                    Width = bounds.Width,
                    Height = nodeBorderSize,
                },
                new Rectangle() { // left
                    X = bounds.X - 1,
                    Y = bounds.Y,
                    Width = nodeBorderSize,
                    Height = bounds.Height
                },
            };

            // draw the borders
            foreach (var border in borders)
                SpriteBatch.Draw(nodeBorderTexture, border, color);
        }

        /// <summary>
        /// Drags the grid between two mouse points
        /// </summary>
        /// <param name="mouse"> Current mouse position </param>
        /// <param name="lastMouse"> Last mouse position </param>
        void dragGrid(Point mouse, Point lastMouse)
        {
            // delta movement between our two points
            var delta = (mouse.ToVector() - lastMouse.ToVector());

            // add the amount we've dragged to the camera position
            gridCamera += delta;

            // clamp camera x value to show only the grid
            gridCamera.X = MathHelper.Clamp(gridCamera.X,
                -(gridBounds.Width - gridViewport.Width) + gridNodeSize.X,
                -gridNodeSize.X);

            // clamp camera y value to show only the grid
            gridCamera.Y = MathHelper.Clamp(gridCamera.Y,
                -(gridBounds.Height - gridViewport.Height) + gridNodeSize.Y,
                -gridNodeSize.Y);
        }

        /// <summary>
        /// Sets a building as being dragged
        /// </summary>
        /// <param name="building"> Target building to drag </param>
        void setDraggedBuilding(BuildingEntity building)
        {
            // remove dragging state from the previous building if it exists
            if (draggedBuilding != null)
                draggedBuilding.SetDragging(false);

            // set dragging state
            building.SetDragging(true);

            // set the new dragged building
            draggedBuilding = building;

            // calculate the absolute bounds for this building
            draggedBuildingBounds = getAbsoluteGridBounds(
                building.Properties.GridBounds);

            // offset by the camera
            draggedBuildingBounds.X -= (int)gridCamera.X;
            draggedBuildingBounds.Y -= (int)gridCamera.Y;
        }

        /// <summary>
        /// Resets the dragged building
        /// </summary>
        void resetDraggedBuilding()
        {
            draggedBuilding.SetDragging(false);
            draggedBuilding = null;
        }

        /// <summary>
        /// Places the building being dragged
        /// </summary>
        void placeDraggedBuilding()
        {
            // placement is valid, set the new location
            if (draggedBuilding.IsPlacementValid)
            {
                // node index from the building bounds
                var newLocation = new Point()
                {
                    Y = draggedBuildingBounds.X / gridNodeSize.X,
                    X = draggedBuildingBounds.Y / gridNodeSize.Y
                };

                // move to the building to its new location
                draggedBuilding.MoveTo(newLocation);
                // stop dragging the building
                draggedBuilding.SetDragging(false);
                // remove our reference
                draggedBuilding = null;
            }
            else
            {
                // the placement was invalid, play the error sound
                invalidPlacementSound.Play();
            }
        }

        /// <summary>
        /// Called when the mouse changes node positions
        /// </summary>
        /// <param name="node"> New node position </param>
        void onMouseNodeChanged(Point node)
        {
            if (draggedBuilding != null)
            {
                var bounds = draggedBuilding.Properties.GridBounds;

                int x = node.X - (bounds.Width / 2),
                    y = node.Y - (bounds.Height / 2);

                // clamp values along the x axis inside the world
                x = (int)MathHelper.Clamp(x, 1,
                    MarsWorld.Size.X - bounds.Height - 1);
                // clamp values along the y axis inside the world
                y = (int)MathHelper.Clamp(y, 1,
                    MarsWorld.Size.Y - bounds.Width - 1);

                draggedBuildingBounds.Location = new Point()
                {
                    X = x * gridNodeSize.X,
                    Y = y * gridNodeSize.Y
                };

                var placementBounds = new Rectangle()
                {
                    Location = new Point(y, x),
                    Width = bounds.Width,
                    Height = bounds.Height
                };

                draggedBuilding.IsPlacementValid =
                    isValidBuildingPlacement(placementBounds);
            }
        }

        /// <summary>
        /// Initializes sizes, and other variables needing to be cached
        /// </summary>
        void init()
        {
            // vector representing the viewport size
            var view = new Vector2(Viewport.Width, Viewport.Height);

            // center the box on the viewport
            targetBoxOffset = (view - boxSize) / 2;

            // account for the menu texture shadow
            targetBoxOffset -= new Vector2(2, 1);

            boxOffset.Y = targetBoxOffset.Y;

            var grid = targetBoxOffset + gridOffset;

            targetViewportBounds = new Rectangle()
            {
                X = (int)grid.X,
                Y = (int)grid.Y,
                Width = gridRectSize.X,
                Height = gridRectSize.Y
            };

            gridViewport = new Viewport(targetViewportBounds);

            defaultViewport = Viewport;

            initGroups();
            initPlacards();
        }

        /// <summary>
        /// Initializes building groups
        /// </summary>
        void initGroups()
        {
            // order each building by description and group them
            // by buildnig category
            var groups = world.GetEntities(EntityType.Building).
                OrderBy(e => ((BuildingEntity)e).Properties.Description).
                GroupBy(e => ((BuildingEntity)e).BuildingType);

            // append a casted linq search to our cached building categories
            foreach (var group in groups)
            {
                buildings[(Building)group.Key] =
                   (List<BuildingEntity>)group.Cast<BuildingEntity>().ToList();
            }
        }

        /// <summary>
        /// Initializes building group placards
        /// </summary>
        void initPlacards()
        {
            int i = 0;

            // x offset from 
            const int xOffset = 16;
            const int yOffset = 80;
            const int xTextOffset = 10;
            const int verticalSpacing = 10;

            foreach (var building in buildings)
            {
                var properties = building.Value[0].Properties;

                var position = new Vector2()
                {
                    X = xOffset,
                    Y = yOffset +
                        (i * (placardTexture.Height + verticalSpacing)),
                };

                var offset = new Vector2()
                {
                    X = xTextOffset,
                    Y = (int)Math.Round((placardTexture.Height -
                        font.MeasureString(properties.Description).Y) / 2)
                };

                var placard = new InventoryPlacard(building.Key,
                    position,
                    offset,
                    properties,
                    building.Value.Count);

                placards.Add(placard);

                i++;
            }
        }

        bool isValidBuildingPlacement(Rectangle bounds)
        {
            foreach (var group in buildings)
            {
                foreach (var building in group.Value)
                {
                    // ignore this - it's being dragged
                    if (building.IsDragging)
                        continue;

                    // is the building's bounds and the target bounds
                    // colliding? If so, the placement isn't valid
                    if (building.Properties.GridBounds.Intersects(bounds))
                        return false;
                }
            }

            // this placement is valid as we couldn't find any intersections
            return true;
        }

        /// <summary>
        /// Checks a point on screen to be contained in a building
        /// </summary>
        /// <param name="point"> Target screen position </param>
        BuildingEntity getBuildingFromPoint(Point point)
        {
            foreach (var group in buildings)
            {
                foreach (var building in group.Value)
                {
                    // building screen position
                    var bounds =
                        getAbsoluteGridBounds(building.Properties.GridBounds);

                    // is the mouse position contained in the bounds?
                    if (bounds.Contains(point))
                    {
                        return building;
                    }
                }
            }

            // couldn't find any buildings at this point
            return null;
        }

        /// <summary>
        /// Converts a building's grid bounds to a rectangle position on screen
        /// </summary>
        /// <param name="bounds"> Relative bulding grid bounds </param>
        Rectangle getAbsoluteGridBounds(Rectangle bounds)
        {
            // NOTICE: x and y are flipped - it's just how the isometric view
            // is set up
            return new Rectangle()
            {
                X = bounds.Y * gridNodeSize.Y +
                    (int)gridCamera.X,
                Y = bounds.X * gridNodeSize.X +
                    (int)gridCamera.Y,
                Width = gridNodeSize.X * bounds.Height,
                Height = gridNodeSize.Y * bounds.Width
            };
        }

        Point getGridMousePosition(Point mouse)
        {
            return (mouse.ToVector() - boxOffset -
                    gridOffset - gridCamera).ToPoint();
        }

        /// <summary>
        /// Converts a point on screen to a grid node index
        /// </summary>
        /// <param name="mouse"> Mouse screen position </param>
        Point getGridMouseNodeIndex(Point mouse)
        {
            var relative = getGridMousePosition(mouse);

            return new Point()
            {
                X = relative.X / gridNodeSize.Y,
                Y = relative.Y / gridNodeSize.Y
            };
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
