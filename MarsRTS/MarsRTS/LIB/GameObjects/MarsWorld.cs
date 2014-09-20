using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects.Entities;
using MarsRTS.LIB.GameObjects.Entities.Enemies;
using MarsRTS.LIB.GameObjects.Entities.Fixed;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.Screen;
using MarsRTS.LIB.Screen.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTSAssetData.Buildings;
using RTSAssetData.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsRTS.LIB.GameObjects
{
    public class MarsWorld
    {
        public static bool Debug = false;

        /// <summary>
        /// Total entites processed. Used for assigning each entity
        /// and ID
        /// </summary>
        public static int EntityCount = 0;

        /// <summary>
        /// Size of the world in grid spaces
        /// </summary>
        public static readonly Point Size = new Point(75, 75);

        public const int NodeWidth = 40;
        public const int NodeHeight = 20;

        const string buildingsPath = "Buildings/";
        const string enemiesPath = "Enemies/";

        GameplayDirector director;

        Camera camera;

        /// <summary>
        /// Viewport camera
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

        TileGrid<GridSpace> grid;

        /// <summary>
        /// Tile grid spaces
        /// </summary>
        public TileGrid<GridSpace> Grid
        {
            get { return grid; }
            set { grid = value; }
        }

        List<Entity> entities;
        List<Projectile> projectiles;

        BuildingEntity draggedBuilding;

        Dictionary<Building, BuildingProperties> buildingProperties =
            new Dictionary<Building, BuildingProperties>();

        Dictionary<Enemy, EnemyProperties> enemyProperties =
            new Dictionary<Enemy, EnemyProperties>();

        ResourceDisplay[] hudResources;

        ResourceBank bank;

        public ResourceBank Bank
        {
            get { return bank; }
            set { bank = value; }
        }

        bool canDrag = true;

        /// <summary>
        /// Determines if the world can be dragged
        /// </summary>
        public bool CanDrag
        {
            get { return canDrag; }
            set { canDrag = value; }
        }

        Texture2D backgroundTexture;

        readonly Color borderColor = Color.White * 0.15f;

        const int borderSize = 2;
        const int boundsPadding = 500;

        EntityBorder[] borders;

        BuildingContextMenu contextMenu;

        public Vector2 ContextMenuPosition
        {
            get { return contextMenu.Position; }
        }

        Rectangle bounds, viewport;

        /// <summary>
        /// World bounds
        /// </summary>
        public Rectangle Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        Play parent;

        WorldMode mode;

        public WorldMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public MarsWorld(Camera camera, Play parent)
        {
            this.camera = camera;
            this.parent = parent;

            director = new GameplayDirector(this);

            entities = new List<Entity>();
            projectiles = new List<Projectile>();

            bank = new ResourceBank();

            mode = WorldMode.None;
        }

        public void LoadContent(ContentManager content)
        {
            initResourceDisplays(content);
            loadBuildingProperties(content);
            loadAlienProperties(content);

            var graphicsDevice = parent.ScreenManager.GraphicsDevice;

            viewport = graphicsDevice.Viewport.Bounds;

            initGrid();
            initBorders();

            BuildingEntity.TileTexture =
                content.Load<Texture2D>("TileSelection");

            backgroundTexture = content.Load<Texture2D>("tileLarge");

            var transition = TimeSpan.FromMilliseconds(400);

            contextMenu =
                new BuildingContextMenu(this, content, transition, transition);

            Random r = new Random();

            //for (int x = 15; x < 17; x += 1)
            //{
            //    for (int y = 15; y < 30; y += 1)
            //    {
            //        AddBuilding(Building.Barricade, new Point(y, x));
            //    }
            //}

            for (int x = 1; x < 75; x++)
            {
                for (int y = 60; y < 62; y++)
                {
                    AddBuilding(Building.Barricade, new Point(y, x));
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            director.Update(gameTime);

            camera.Update(gameTime);

            entities.Sort();

            // update entities
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.IsRemoved)
                {
                    // invoke the deletion callback
                    if (entity.OnDeletion != null)
                        entity.OnDeletion(entity, EventArgs.Empty);

                    // remove the entity from the list
                    entities.RemoveAt(i--);
                }
                else
                {
                    entity.Update(gameTime);
                }
            }

            // update projectiles
            for (int j = 0; j < projectiles.Count; j++)
            {
                var projectile = projectiles[j];

                if (projectile.IsRemoved)
                {
                    // invoke the deletion callback
                    if (projectile.OnDeletion != null)
                        projectile.OnDeletion(projectile, EventArgs.Empty);

                    // remove the entity from the list
                    projectiles.RemoveAt(j--);
                }
                else
                {
                    projectile.Update(gameTime);
                }
            }

            if (mode == WorldMode.MovingBuilding)
            {
                var mouse = InputManager.MousePosition.ToVector();
                var node = GetNodePosition(mouse - camera.RawPosition);
                var bounds = draggedBuilding.Properties.GridBounds;

                node.X -= (int)Math.Round(bounds.Width / 2.0f);
                node.Y -= (int)Math.Round(bounds.Height / 2.0f);

                var max = new Point()
                {
                    X = Size.X - bounds.Width - 1,
                    Y = Size.Y - bounds.Height - 1,
                };

                node.X = (int)MathHelper.Clamp(node.X, 1, max.X);
                node.Y = (int)MathHelper.Clamp(node.Y, 1, max.Y);

                draggedBuilding.DragNodePosition = node;
            }
            else
            {
                contextMenu.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            drawBackground(spriteBatch);
            drawBorders(spriteBatch);

            // cache camera position
            var offset = camera.Position;

            foreach (var entity in entities)
                if (entity.UsesShadow)
                    entity.DrawShadow(spriteBatch, offset);

            foreach (var entity in entities)
                entity.Draw(spriteBatch, offset);

            foreach (var projectile in projectiles)
                projectile.Draw(spriteBatch, offset);

            contextMenu.Draw(spriteBatch);

            drawHUD(spriteBatch);
        }

        /// <summary>
        /// Callback when the world is clicked
        /// </summary>
        /// <param name="location"> Click location on screen </param>
        public void OnClick(Vector2 location)
        {
            if (mode == WorldMode.MovingBuilding)
            {
                draggedBuilding.StopDragging();

                mode = WorldMode.None;
            }
            else
            {
                if (contextMenu.State == MenuState.Active)
                {
                    contextMenu.OnClick(location);
                }
                else
                {
                    var building = (BuildingEntity)FindEntity(location,
                        EntityType.Building);

                    if (building != null)
                    {
                        contextMenu.Open(building);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an entity to the given list
        /// </summary>
        /// <param name="list"> Target entity list </param>
        /// <param name="entity"> New entity </param>
        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        /// <summary>
        /// Adds a projectile to the world
        /// </summary>
        /// <param name="projectile"> Projectile to add </param>
        public void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        /// <summary>
        /// Gets the projectiles in world
        /// </summary>
        public List<Projectile> GetProjectiles()
        {
            return projectiles;
        }

        /// <summary>
        /// Gets all entities in the given list
        /// </summary>
        /// <param name="ofType"> Target entity type </param>
        public List<Entity> GetEntities(EntityType ofType = EntityType.All)
        {
            if (ofType == EntityType.All)
                return entities;

            if (ofType == EntityType.Projectile)
                return (List<Entity>) projectiles.Cast<Entity>();

            return entities.Where(e => e.Type == ofType).ToList();
        }

        /// <summary>
        /// Finds all entites in range of the given position
        /// </summary>
        /// <param name="list"> Target entity list </param>
        /// <param name="position"> Origin of range </param>
        /// <param name="range"> Radius of the maximum range </param>
        public List<Entity> GetEntitiesInRange(Vector2 position, float range, EntityType ofType = EntityType.All)
        {
            float maxRange = range * range;

            List<Entity> searchEntities;

            if (ofType == EntityType.Projectile)
            {
                searchEntities = projectiles.Cast<Entity>().ToList();
            }
            else
            {
                // entities to search. filter by type if EntityType.All 
                // isn't set
                searchEntities = ofType == EntityType.All ? entities :
                    entities.Where(e => e.Type == ofType).ToList();
            }

            var results = new List<Entity>();

            foreach (var entity in searchEntities)
            {
                float distance =
                    Vector2.DistanceSquared(position, entity.GetCenter());

                if (!entity.IsRemoved && distance <= maxRange)
                    results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// Adds a building to the world's entities
        /// </summary>
        /// <param name="type"> Building type </param>
        /// <param name="location"> Target node index location </param>
        public void AddBuilding(Building type, Point location)
        {
            var building = new BuildingEntity(this, buildingProperties[type]);

            building.MoveTo(location);

            entities.Add(building);
        }

        /// <summary>
        /// Creates a new building entity
        /// </summary>
        /// <param name="type"> Building type to create </param>
        public BuildingEntity CreateBuilding(Building type)
        {
            return new BuildingEntity(this, buildingProperties[type]);
        }

        /// <summary>
        /// Removes the cost of a building from the resource totals
        /// </summary>
        /// <param name="building"> Building to purchase </param>
        public void PurchaseBuilding(BuildingEntity building)
        {
            bank.Spend(building.Properties.GetUpgradeCost(0));

            UpdateResourceHUD();
        }

        /// <summary>
        /// Finds a building at the given point
        /// </summary>
        /// <param name="node"> Building node position </param>
        /// <returns> null if not found </returns>
        public BuildingEntity FindBuilding(Point node)
        {
            return grid[node].Occupant;
        }

        /// <summary>
        /// Adds a enemy to the world's entities
        /// </summary>
        /// <param name="type"> Enemy type </param>
        /// <param name="position"> Target world position </param>
        public void AddEnemy(Enemy type, Vector2 position)
        {
            var enemy = new EnemyEntity(this, enemyProperties[type]);

            enemy.Position = position;

            entities.Add(enemy);
        }

        /// <summary>
        /// Creates a new enemy entity
        /// </summary>
        /// <param name="type"> Enemy type </param>
        public EnemyEntity CreateEnemy(Enemy type)
        {
            return new EnemyEntity(this, enemyProperties[type]);
        }

        public void OpenBuildingScreen(BuildingEntity building, bool isUpgrade)
        {
            parent.ScreenManager.AddScreen(
                new BuildingScreen(building, this, isUpgrade));
        }

        public void OpenLayoutManager()
        {
            parent.ScreenManager.AddScreen(new BaseLayoutManager(this));
        }

        public void BankResource(ResourceType type, int amount)
        {
            var resource = bank.Resources[type];

            bank.AddTo(type, amount);

            hudResources[(int)type].SetTotal(resource.Bank);
        }

        public void UpdateResourceHUD()
        {
            var resources = Enum.GetValues(typeof(ResourceType));

            foreach (var resource in resources)
            {
                var type = (ResourceType)resource;
                var hudResource = hudResources[(int)type];

                hudResource.SetTotal(bank.GetTotal(type));
                hudResource.SetMax(bank.GetMax(type));
            }
        }

        public void SetResourceMax(ResourceType type, int max)
        {
            hudResources[(int)type].SetMax(max);
        }

        public void SetDragging(BuildingEntity building)
        {
            mode = WorldMode.MovingBuilding;

            if (draggedBuilding != null)
                draggedBuilding.SetDragging(false);

            draggedBuilding = building;
            draggedBuilding.SetDragging(true);
        }

        /// <summary>
        /// Converts a screen position to a node index
        /// </summary>
        /// <param name="screenPosition"> Position on screen </param>
        public Point GetNodePosition(Vector2 screenPosition)
        {
            Vector2 offset = screenPosition /
                new Vector2(NodeWidth, NodeHeight);

            return new Point()
            {
                X = (int)Math.Round(offset.X + offset.Y) - 1,
                Y = -(int)Math.Round(-offset.X + offset.Y) - 1
            };
        }

        /// <summary>
        /// Finds an entity at the given location
        /// </summary>
        /// <param name="location"> Screen location </param>
        /// <param name="ofType"> Target entity type </param>
        public Entity FindEntity(Vector2 location, EntityType ofType = EntityType.All)
        {
            var point = location.ToPoint();

            // sort the entities so they are of proper z-index
            entities.Sort();

            // iterate backwards so we get the entities on top first
            for (int i = entities.Count - 1; i > -1; i--)
            {
                var entity = entities[i];

                // not of our target type
                if (ofType != EntityType.All && entity.Type != ofType)
                    continue;

                // isn't removed and our point is contained in the screen
                // rectangle
                if (!entity.IsRemoved &&
                    entity.GetScreenRectangle().Contains(point))
                    return entity;
            }

            // couldn't find an entity at this point
            return null;
        }

        /// <summary>
        /// Gets the default properties for a building type
        /// </summary>
        /// <param name="type"> Target building type </param>
        /// <returns></returns>
        public BuildingProperties GetBuildingProperties(Building type)
        {
            return buildingProperties[type];
        }

        /// <summary>
        /// Determines if the given position is inside the bounds of the world
        /// </summary>
        public bool IsNodeInsideBounds(Point node)
        {
            return new Rectangle(0, 0, Size.X - 1, Size.Y - 1).Contains(node);
        }

        /// <summary>
        /// Initializes the tile grid positions
        /// </summary>
        void initGrid()
        {
            grid = new TileGrid<GridSpace>(Size.X, Size.Y);

            Vector2 position = Vector2.Zero;

            for (int x = 0; x < Size.X; x++)
            {
                position.X = (NodeWidth / 2) * x;
                position.Y = (NodeHeight / 2) * x;

                for (int y = 0; y < Size.Y; y++)
                {
                    position.X += (NodeWidth / 2);
                    position.Y -= (NodeHeight / 2);

                    var node = grid[x, y];

                    node.Position = position;
                    node.TileIndex = 0;
                }
            }

            int bottom = Size.Y - 1,
                right = Size.X - 1;

            bounds.X = (int)grid[0, 0].Position.X;

            bounds.Y = (int)grid[0, bottom].Position.Y;

            bounds.Width = (int)grid[right, bottom].Position.X
                + NodeWidth - bounds.X;

            bounds.Height = (int)grid[right, 0].Position.Y
                + NodeHeight - bounds.Y;

            bounds.Inflate(boundsPadding, boundsPadding);

            var min = new Point()
            {
                X = -(bounds.Right - viewport.Width),
                Y = -(bounds.Bottom - viewport.Height)
            };

            var max = new Point()
            {
                X = -bounds.X,
                Y = -bounds.Y
            };

            camera.SetConstraints(min, max);

            camera.Position = viewport.Center.ToVector() -
                bounds.Center.ToVector();
        }

        void initBorders()
        {
            int right = Size.Y - 2,
                bottom = Size.X - 2;

            var border = new Vector2(borderSize);

            var topLeft = grid[1, 1].Position +
                new Vector2(0, NodeHeight / 2);

            var topRight = grid[1, right].Position +
                new Vector2(NodeWidth / 2, 0);

            var bottomLeft = grid[bottom, 1].Position +
                new Vector2(NodeWidth / 2, NodeHeight);

            var bottomRight = grid[bottom, right].Position +
                        new Vector2(NodeWidth, NodeHeight / 2);

            var points = new Vector2[,]
            {
                {
                    topLeft,
                    bottomLeft + new Vector2(2, 0)
                }, 
                {
                    bottomLeft,
                    bottomRight
                },
                {
                    bottomRight + border,
                    topRight - new Vector2(2, 0),
                },
                {
                    topRight,
                    topLeft + new Vector2(0, 2)
                }
            };

            borders = new EntityBorder[4];

            for (int i = 0; i < 4; i++)
            {
                borders[i] = new EntityBorder(points[i, 0], points[i, 1],
                    borderSize);
            }
        }

        void initResourceDisplays(ContentManager content)
        {
            string iconsPath = @"HUD/Icons/";

            int xPosition = 10,
                yPosition = 62;

            var iconOffset = new Vector2(5, 4);

            var icons = new Texture2D[3] {
                content.Load<Texture2D>(iconsPath + "ResourceCopper"),
                content.Load<Texture2D>(iconsPath + "ResourceIron"),
                content.Load<Texture2D>(iconsPath + "ResourceNickel")
            };

            var resources = Enum.GetValues(typeof(ResourceType));

            hudResources = new ResourceDisplay[resources.Length];

            for (int i = 0; i < bank.Resources.Count; i++)
            {
                var resource = (ResourceType)resources.GetValue(i);

                var location = new Point(xPosition, yPosition);

                hudResources[i] = new ResourceDisplay(icons[i], location, i);
            }
        }

        /// <summary>
        /// Load all building type properties
        /// </summary>
        /// <param name="content"> Content manager </param>
        void loadBuildingProperties(ContentManager content)
        {
            var buildingType = typeof(Building);

            var buildings = Enum.GetValues(buildingType);

            foreach (var building in buildings)
            {
                string asset = buildingsPath +
                    Enum.GetName(buildingType, building);

                var properties = content.Load<BuildingProperties>(asset);

                properties.LoadAnimations(content);
                properties.LoadSounds(content);

                buildingProperties[(Building)building] = properties;

                if (properties.Category == BuildingCategory.Resource)
                {
                    bank.SetMax(properties.Resource,
                        properties.GetUpgradeData(0).MaxResourceBank);

                    bank.SetTotal(properties.Resource, 10000);

                    hudResources[(int)properties.Resource].SetTotal(10000);
                }
            }
        }

        void loadAlienProperties(ContentManager content)
        {
            var alienType = typeof(Enemy);

            var enemies = Enum.GetValues(alienType);

            foreach (var building in enemies)
            {
                string asset = enemiesPath +
                    Enum.GetName(alienType, building);

                var properties = content.Load<EnemyProperties>(asset);

                properties.LoadAnimations(content);

                enemyProperties[(Enemy)building] = properties;
            }
        }

        /// <summary>
        /// Renders the background
        /// </summary>
        void drawBackground(SpriteBatch spriteBatch)
        {
            var position = camera.Position;

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearWrap,
                null,
                null);

            spriteBatch.Draw(backgroundTexture,
                getBackgroundBounds(),
                bounds,
                Color.White);

            spriteBatch.End();
            spriteBatch.Begin();
        }

        void drawBorders(SpriteBatch spriteBatch)
        {
            var position = camera.Position;

            foreach (var border in borders)
            {
                spriteBatch.Draw(MarsRTS.BlankTexture,
                    border.GetBounds(position),
                    MarsRTS.BlankTexture.Bounds,
                    borderColor,
                    border.Angle,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0);
            }
        }

        void drawHUD(SpriteBatch spriteBatch)
        {
            foreach (var resource in hudResources)
            {
                resource.Draw(spriteBatch);
            }
        }

        void drawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            spriteBatch.Draw(MarsRTS.BlankTexture, bounds, bounds, color);
        }

        Rectangle getBackgroundBounds()
        {
            var temp = bounds;
            var position = camera.Position;

            temp.X += (int)position.X;
            temp.Y += (int)position.Y;

            return temp;
        }
    }
}
