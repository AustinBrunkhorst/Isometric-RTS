using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects.Entities.Enemies;
using MarsRTS.LIB.Input;
using MarsRTS.LIB.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSAssetData.Buildings;
using System;
using System.Collections.Generic;

namespace MarsRTS.LIB.GameObjects.Entities.Fixed
{
    public class BuildingEntity : HealthEntity
    {
        static Texture2D tileTexture;

        public static Texture2D TileTexture
        {
            get { return tileTexture; }
            set { tileTexture = value; }
        }

        readonly Color dragModeTint = Color.White * 0.40f;
        readonly Color validPlacementTint = Color.White * 0.20f;
        readonly Color invalidPlacementTint = new Color(180, 33, 33) * 0.20f;

        readonly Color validBuildingPlacementTint = Color.White * 0.65f;
        readonly Color invalidBuildingPlacementTint = new Color(180, 33, 33) * 0.65f;

        readonly Color borderColor = Color.White * 0.55f;
        readonly Color invalidBorderColor = new Color(180, 33, 33) * 0.55f;

        /// <summary>
        /// Animations available to this building
        /// </summary>
        Dictionary<string, Animation> animations =
            new Dictionary<string, Animation>();

        /// <summary>
        /// Sounds available to this building
        /// </summary>
        Dictionary<string, SoundEffect> sounds =
            new Dictionary<string, SoundEffect>();

        BuildingProperties properties;

        /// <summary>
        /// Building properties
        /// </summary>
        public BuildingProperties Properties
        {
            get { return properties; }
        }

        Rectangle lastDragPlacement;

        public Point DragNodePosition
        {
            set
            {
                if (lastDragPlacement.Location != value)
                {
                    lastDragPlacement.Location = value;

                    position = world.Grid[value].Position;

                    validateDragPlacement();
                }
            }
        }

        /// <summary>
        /// Current animation used by this building
        /// </summary>
        string currentAnimation;

        /// <summary>
        /// Building type
        /// </summary>
        public Building BuildingType
        {
            get { return properties.Type; }
        }

        /// <summary>
        /// Building category
        /// </summary>
        public BuildingCategory Category
        {
            get { return properties.Category; }
        }

        bool isDragging, isPlacementValid;

        /// <summary>
        /// Determines if this building is being dragged in world or the
        /// base layout manager
        /// </summary>
        public bool IsDragging
        {
            get { return isDragging; }
            set { isDragging = value; }
        }

        /// <summary>
        /// Determines if the building placement is valid when moving
        /// </summary>
        public bool IsPlacementValid
        {
            get { return isPlacementValid; }
            set { isPlacementValid = value; }
        }

        EnemyEntity lockOnEnemy;

        EntityBorder[] borders;

        const int borderSize = 1;

        TimeSpan lastFired;

        public BuildingEntity(MarsWorld world, BuildingProperties properties)
            : base(world)
        {
            this.Type = EntityType.Building;

            this.properties = properties;
            this.healthBar = new HealthBar(properties.HealthBarSize);
            this.size = new Point(properties.ScreenRectangle.Width,
                properties.ScreenRectangle.Height);

            this.Health = GetMaxHealth();

            // initialize animations
            initAnimations();

            // initialize sounds
            initSounds();

            // set our animation to idle by default
            SetAnimation("Idle");

            initBorders();

            // set our grid bounds to the property's grid bounds
            updateGridBounds(properties.GridBounds);

            // set the size of our drag placement rectangle
            lastDragPlacement.Width = properties.GridBounds.Width;
            lastDragPlacement.Height = properties.GridBounds.Height;
        }

        public override void Update(GameTime gameTime)
        {
            /*// update our current animation
            animations[currentAnimation].Update(gameTime);

            if (UsesShadow)
                animations["Shadow"].Update(gameTime);*/

            if (world.Mode == WorldMode.MovingBuilding)
            {
                if (isDragging)
                {
                    if (isPlacementValid)
                        tint = validBuildingPlacementTint;
                    else
                        tint = invalidBuildingPlacementTint;
                }
                else
                {
                    tint = dragModeTint;
                }
            }
            else
            {
                tint = Color.White;
            }

            if (InputManager.IsKeyPressed(Keys.LeftShift))
            {
                var offset = properties.GridOffset;

                if (InputManager.IsKeyTriggered(Keys.Left))
                    offset.X--;
                if (InputManager.IsKeyTriggered(Keys.Right))
                    offset.X++;
                if (InputManager.IsKeyTriggered(Keys.Up))
                    offset.Y--;
                if (InputManager.IsKeyTriggered(Keys.Down))
                    offset.Y++;

                properties.GridOffset = offset;
            }

            if (InputManager.IsKeyPressed(Keys.LeftControl))
            {
                var offset = properties.HealthBarOffset;

                if (InputManager.IsKeyTriggered(Keys.Left))
                    offset.X--;
                if (InputManager.IsKeyTriggered(Keys.Right))
                    offset.X++;
                if (InputManager.IsKeyTriggered(Keys.Up))
                    offset.Y--;
                if (InputManager.IsKeyTriggered(Keys.Down))
                    offset.Y++;

                properties.HealthBarOffset = offset;

                ShowHealthBar();
            }
            else
            {
                if (Health == GetMaxHealth())
                    HideHealthBar();
                else
                    ShowHealthBar();
            }

            if (properties.Category == BuildingCategory.Defense)
            {
                lastFired -= gameTime.ElapsedGameTime;

                if (lastFired <= TimeSpan.Zero)
                {
                    defend();

                    lastFired = TimeSpan.FromMilliseconds(
                        properties.UpgradeData.DefenseInterval);
                }

                //var barrel = position + properties.GridOffset + properties.BarrelOffset;
                //var mouse = InputManager.MousePosition.ToVector() - world.Camera.RawPosition;

                //var angle = (float)Math.Atan2(barrel.Y - mouse.Y,
                //    barrel.X - mouse.X) + MathHelper.ToRadians(90);

                ////setTowerAngle(angle);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            var shift = InputManager.IsKeyPressed(Keys.LeftShift);

            if (world.Mode == WorldMode.MovingBuilding || shift)
            {
                Rectangle bounds = properties.GridBounds;
                Color tileTint = validPlacementTint;

                if (isDragging)
                {
                    bounds = lastDragPlacement;

                    if (!isPlacementValid)
                        tileTint = invalidPlacementTint;
                }

                for (int x = bounds.X; x < bounds.Right; x++)
                {
                    for (int y = bounds.Y; y < bounds.Bottom; y++)
                    {
                        spriteBatch.Draw(tileTexture,
                            world.Grid[x, y].Position + camera,
                            tileTint);
                    }
                }

                drawBorders(spriteBatch);
            }

            // draw the current animation
            animations[currentAnimation].Draw(spriteBatch,
                position + properties.GridOffset + camera,
                tint);

            if (shift)
            {
                Font.Draw(spriteBatch,
                    "Myriad",
                    properties.GridOffset.ToString(),
                    GetCenter() + camera,
                    15,
                    Color.White);
            }

            if (InputManager.IsKeyPressed(Keys.LeftControl))
            {
                Font.Draw(spriteBatch,
                    "Myriad",
                    properties.HealthBarOffset.ToString(),
                    GetCenter() + camera,
                    15,
                    Color.White);
            }

            base.Draw(spriteBatch, camera);
        }

        public override void DrawShadow(SpriteBatch spriteBatch, Vector2 camera)
        {
            // draw the current animation
            animations["Shadow"].Draw(spriteBatch,
                position + properties.GridOffset + camera,
                tint);
        }

        public override void Delete()
        {
            if (!IsRemoved)
            {
                world.Camera.Shake(1.25f);
            }

            base.Delete();

            setPassiblity(properties.GridBounds, true);
        }

        public override Vector2 GetSortOffset()
        {
            return properties.GridOffset + properties.DepthPointOffset;
        }

        public override Vector2 GetCenter()
        {
            return GetScreenRectangle().Center.ToVector();
        }

        public override Rectangle GetScreenRectangle()
        {
            var offset = position + properties.GridOffset;

            var bounds = new Rectangle()
            {
                X = (int)offset.X + properties.ScreenRectangle.X,
                Y = (int)offset.Y + properties.ScreenRectangle.Y,
                Width = size.X,
                Height = size.Y
            };

            return bounds;
        }

        public override Vector2 GetHealthBarPosition()
        {
            return position + properties.HealthBarOffset;
        }

        public override float GetMaxHealth()
        {
            return properties.UpgradeData.MaxHealth;
        }

        /// <summary>
        /// Gets a random node to move to for attacking entities.
        /// </summary>
        /// <returns> null if no node is available </returns>
        public AttackNode GetAttackTargetNode()
        {
            var availableNodes = new List<AttackNode>();
            var bounds = properties.GridBounds;

            // seek top and bottom
            for (int x = bounds.X; x < bounds.Right; x++)
            {
                availableNodes.Add(
                    new AttackNode(
                        new Point(x, bounds.Y - 1),
                        (int)IdleDirection.NORTHEAST // |/
                    )
                );
                availableNodes.Add(
                    new AttackNode(
                        new Point(x, bounds.Bottom),
                        (int)IdleDirection.SOUTHWEST // /|
                    )
                );
            }

            // seek left and right
            for (int y = bounds.Y; y < bounds.Bottom; y++)
            {
                availableNodes.Add(
                    new AttackNode(
                        new Point(bounds.X - 1, y),
                        (int)IdleDirection.SOUTHEAST // |\
                    )
                );
                availableNodes.Add(
                    new AttackNode(
                        new Point(bounds.Right, y),
                        (int)IdleDirection.NORTHWEST // \|
                    )
                );
            }

            // remove invalid nodes (outside world & impassible)
            for (int i = 0; i < availableNodes.Count; i++)
            {
                var node = availableNodes[i];

                if (!world.IsNodeInsideBounds(node.Destination) ||
                    !world.Grid[node.Destination].IsPassible)
                    availableNodes.RemoveAt(i--);
            }

            // return null - we coudln't find any available nodes
            if (availableNodes.Count == 0)
                return null;

            // return a random node
            return availableNodes[random.Next(availableNodes.Count)];
        }

        /// <summary>
        /// Called when this building opens a context menu. The items returned
        /// will be populated as buttons in the context menu
        /// </summary>
        public List<ContextMenuItem> GetContextMenuItems()
        {
            var items = new List<ContextMenuItem>();

            var move = new ContextMenuItem("Move");

            move.OnClick += delegate
            {
                world.SetDragging(this);
            };

            var upgrade = new ContextMenuItem("Upgrade");

            upgrade.OnClick += delegate
            {
                world.OpenBuildingScreen(this, true);
            };

            var range = new ContextMenuItem("Show Range");

            range.OnClick += delegate
            {

            };

            var collect = new ContextMenuItem("Collect All");

            collect.OnClick += delegate
            {
                world.BankResource(properties.Resource, 250);
            };

            var layoutManager = new ContextMenuItem("Manage Layout");

            layoutManager.OnClick += delegate
            {
                world.OpenLayoutManager();
            };

            switch (properties.Category)
            {
                case BuildingCategory.General:
                    items.Add(upgrade);

                    if (BuildingType == Building.Base && properties.Level > 0)
                        items.Add(layoutManager);

                    break;
                case BuildingCategory.Defense:
                    items.Add(upgrade);
                    items.Add(range);
                    break;
                case BuildingCategory.Resource:
                    items.Add(collect);
                    items.Add(upgrade);
                    break;
            }

            items.Add(move);

            return items;
        }

        /// <summary>
        /// Gets the display data used in the building screen
        /// </summary>
        /// <param name="level"> Target upgrade data level </param>
        public object GetDisplayData(int level)
        {
            var upgrade = properties.GetUpgradeData(level);

            var stats = new List<BuildingStatDisplay>();

            switch (properties.Type)
            {
                case Building.Base:
                    return upgrade.Description;
                case Building.ResourceCopper:
                case Building.ResourceNickel:
                case Building.ResourceIron:
                    stats.Add(new BuildingStatDisplay("Collection Per Minute",
                        0.75f));
                    stats.Add(new BuildingStatDisplay("Storage",
                        0.95f));
                    break;
                case Building.ShortRangeTower:
                case Building.LongRangeTower:
                    stats.Add(new BuildingStatDisplay("Range",
                        (float)random.NextDouble()));
                    stats.Add(new BuildingStatDisplay("Damage",
                        (float)random.NextDouble()));
                    stats.Add(new BuildingStatDisplay("Fire Rate",
                        (float)random.NextDouble()));
                    break;
            }

            return stats;
        }

        /// <summary>
        /// Moves the building to the new location
        /// </summary>
        /// <param name="location"> Target location </param>
        public void MoveTo(Point location)
        {
            var bounds = properties.GridBounds;

            // set the location to the new bounds
            bounds.Location = location;

            // update the new bounds
            updateGridBounds(bounds);
        }

        /// <summary>
        /// Upgrades the building to the given level
        /// </summary>
        /// <param name="level"> Target level. -1 goes to the next level </param>
        public void Upgrade(int level = -1)
        {
            world.Bank.Spend(properties.GetUpgradeCost(level));

            world.UpdateResourceHUD();

            properties.Upgrade(level);
        }

        /// <summary>
        /// Sets the drag state of the building
        /// </summary>
        /// <param name="state"> Target dragging state </param>
        public void SetDragging(bool state)
        {
            isDragging = state;

            if (state == true)
            {
                isPlacementValid = true;

                lastDragPlacement.Location = properties.GridBounds.Location;
            }
        }

        /// <summary>
        /// Attempts to place the building at it's current location from
        /// moving it in world
        /// </summary>
        public void StopDragging()
        {
            SetDragging(false);

            if (isPlacementValid)
            {
                updateGridBounds(lastDragPlacement);
            }

            position = world.Grid[properties.GridBounds.Location].Position;
        }

        /// <summary>
        /// Sets the new animation
        /// </summary>
        /// <param name="animation"> Target animation </param>
        public void SetAnimation(string animation)
        {
            if (currentAnimation != null)
                animations[currentAnimation].Reset();

            currentAnimation = animation;

            animations[animation].Play();
        }

        void defend()
        {
            var center = GetCenter();
            var data = properties.UpgradeData;

            if (lockOnEnemy != null)
            {
                if (lockOnEnemy.IsRemoved)
                {
                    lockOnEnemy = null;
                    return;
                }

                var distance = Vector2.DistanceSquared(center, lockOnEnemy.Position);

                var min = properties.MinRange * properties.MinRange;
                var max = data.Range * data.Range;

                if (distance < min || distance > max)
                {
                    lockOnEnemy = null;
                    return;
                }

                var barrel = position + properties.GridOffset +
                    properties.BarrelOffset;

                var enemy = lockOnEnemy.GetAttackOffset();

                var angle = (float)Math.Atan2(barrel.Y - enemy.Y,
                    barrel.X - enemy.X) + MathHelper.ToRadians(90);

                setTowerAngle(angle);
                shootBullet(enemy);
            }
            else
            {
                var enemies = world.GetEntitiesInRange(
                    center,
                    data.Range,
                    EntityType.AI);

                if (enemies.Count == 0)
                    return;

                lockOnEnemy = (EnemyEntity)enemies[0];
            }
        }

        void shootBullet(Vector2 target)
        {
            var data = properties.UpgradeData;
            var frame = animations["Idle"].FrameIndex;
            var barrel = position +
                properties.GridOffset +
                properties.BarrelOffsets[frame];

            var bullet = new Bullet(world, barrel, animations["Bullet"]);

            bullet.Damage = data.Damage;
            bullet.Speed = data.BulletSpeed;
            bullet.ExplosionRadius = data.AttackExplosionRange;

            bullet.Velocity = -Vector2.Normalize(barrel - target);

            float pitch = (float)random.NextDouble() * 0.5f - 0.25f;

            //sounds["Shoot"].Play(1.0f, (float)random.NextDouble() - 0.5f, 0);

            world.AddProjectile(bullet);
        }

        void validateDragPlacement()
        {
            var buildings = world.GetEntities(EntityType.Building);

            isPlacementValid = true;

            foreach (var entity in buildings)
            {
                if (this == entity)
                    continue;

                var bounds = ((BuildingEntity)entity).properties.GridBounds;

                if (bounds.Intersects(lastDragPlacement))
                {
                    isPlacementValid = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Initializes the building's animations
        /// </summary>
        void initAnimations()
        {
            // iterate through all animations contained in properies and set
            // replace it with an actual animation in our dictionary
            foreach (var animation in properties.Animations)
                animations[animation.Key] =
                    Animation.FromData(animation.Value);

            // don't need the animation data anymore
            properties.Animations = null;

            UsesShadow = animations.ContainsKey("Shadow");
        }

        /// <summary>
        /// Initializes building's sounds
        /// </summary>
        void initSounds()
        {
            if (properties.Sounds == null)
                return;

            foreach (var sound in properties.Sounds)
                sounds[sound.Key] = sound.Value.Sound;

            properties.Sounds = null;
        }

        void initBorders()
        {
            var bounds = properties.GridBounds;

            int right = bounds.Bottom - 1,
                 bottom = bounds.Width - 1;

            var border = new Vector2(borderSize);

            var topLeft = world.Grid[bounds.Location].Position +
                new Vector2(0, MarsWorld.NodeHeight / 2);

            var topRight = world.Grid[bounds.Y, right].Position +
                new Vector2(MarsWorld.NodeWidth / 2, 0);

            var bottomLeft = world.Grid[bottom, bounds.X].Position +
                new Vector2(MarsWorld.NodeWidth / 2, MarsWorld.NodeHeight);

            var bottomRight = world.Grid[bottom, right].Position +
                new Vector2(MarsWorld.NodeWidth, MarsWorld.NodeHeight / 2);

            var offset =
                new Vector2(-MarsWorld.NodeHeight, MarsWorld.NodeHeight / 2);

            var points = new Vector2[,]
            {
                {
                    topLeft + offset,
                    bottomLeft + new Vector2(1, 0) + offset
                }, 
                {
                    bottomLeft + offset,
                    bottomRight + offset
                },
                {
                    bottomRight + border + offset,
                    topRight - new Vector2(1, 0) + offset,
                },
                {
                    topRight + offset,
                    topLeft + new Vector2(0, 1) + offset
                }
            };

            borders = new EntityBorder[4];

            for (int i = 0; i < 4; i++)
            {
                borders[i] = new EntityBorder(points[i, 0], points[i, 1],
                    borderSize);
            }
        }

        void drawBorders(SpriteBatch spriteBatch)
        {
            Color color = borderColor;

            if (isDragging && !isPlacementValid)
                color = invalidBorderColor;

            foreach (var border in borders)
            {
                spriteBatch.Draw(MarsRTS.BlankTexture,
                    border.GetBounds(position + world.Camera.RawPosition),
                    MarsRTS.BlankTexture.Bounds,
                    color,
                    border.Angle,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0);
            }
        }

        /// <summary>
        /// Updates this building's grid bounds
        /// </summary>
        /// <param name="bounds"> new grid bounds </param>
        void updateGridBounds(Rectangle bounds)
        {
            // set the old bound's tiles to passible
            setPassiblity(properties.GridBounds, true);
            setOccupancy(properties.GridBounds, null);

            properties.GridBounds = bounds;

            // find the new position on the grid
            position = world.Grid[bounds.X, bounds.Y].Position;

            // set the new bound's tiles to the passiblity set in properties
            setPassiblity(bounds, properties.IsPassible);
            setOccupancy(properties.GridBounds, this);
        }

        /// <summary>
        /// Sets all node's passibility inside the given bounds to the
        /// given state
        /// </summary>
        /// <param name="bounds"> Target bounds </param>
        /// <param name="state"> New state </param>
        void setPassiblity(Rectangle bounds, bool state)
        {
            for (int x = bounds.X; x < bounds.Right; x++)
            {
                for (int y = bounds.Y; y < bounds.Bottom; y++)
                {
                    // set passibility to the given state at x, y
                    world.Grid[x, y].IsPassible = state;
                }
            }
        }

        void setOccupancy(Rectangle bounds, BuildingEntity occupant)
        {
            for (int x = bounds.X; x < bounds.Right; x++)
            {
                for (int y = bounds.Y; y < bounds.Bottom; y++)
                {
                    // set passibility to the given state at x, y
                    world.Grid[x, y].Occupant = occupant;
                }
            }
        }

        void setTowerAngle(float angle)
        {
            var degrees = MathHelper.ToDegrees(angle);

            if (properties.Type == Building.ShortRangeTower)
            {
                degrees += 180;
            }
            else if (properties.Type == Building.LongRangeTower)
            {
                degrees = 360 - (degrees - 50);
            }

            var frame = (int)Math.Floor(degrees / 10) % 36;

            if (frame < 0)
                frame = 36 + frame;

            animations["Idle"].FrameIndex = frame;
            animations["Shadow"].FrameIndex = frame;
        }
    }
}
