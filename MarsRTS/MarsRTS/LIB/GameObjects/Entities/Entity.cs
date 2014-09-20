using MarsRTS.LIB.GameObjects.Entities.Fixed;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSAssetData.Buildings;
using System;

namespace MarsRTS.LIB.GameObjects.Entities
{
    public abstract class Entity : IComparable<Entity>
    {
        EventHandler onDeletion;

        /// <summary>
        /// Callback when this entity is deleted
        /// </summary>
        public EventHandler OnDeletion
        {
            get { return onDeletion; }
            set { onDeletion = value; }
        }

        EntityType type;

        /// <summary>
        /// Entity's type
        /// </summary>
        public EntityType Type
        {
            get { return type; }
            set { type = value; }
        }

        protected Vector2 position, velocity;

        /// <summary>
        /// Entity position
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Entity velocity
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        protected Point size;

        public Point Size
        {
            get { return size; }
            set { size = value; }
        }

        protected Color tint = Color.White;

        readonly Color debugColor = Color.Orange * 0.65f;

        /// <summary>
        /// Entity color tint
        /// </summary>
        public Color Tint
        {
            get { return tint; }
            set { tint = value; }
        }

        protected float collisionRadius;

        public float CollisionRadius
        {
            get { return collisionRadius; }
            set { collisionRadius = value; }
        }

        bool removed, isFixed, isCollidable, usesShadow;

        /// <summary>
        /// Determines if this entity is flagged for removal
        /// </summary>
        public bool IsRemoved
        {
            get { return removed; }
            set { removed = value; }
        }

        /// <summary>
        /// Determines if this entity is fixed
        /// </summary>
        public bool IsFixed
        {
            get { return isFixed; }
            set { isFixed = value; }
        }

        /// <summary>
        /// Determines if this entity can collide with other entities
        /// </summary>
        public bool IsCollidable
        {
            get { return isCollidable; }
            set { isCollidable = value; }
        }

        /// <summary>
        /// Determines if this entity renders a shadow
        /// </summary>
        public bool UsesShadow
        {
            get { return usesShadow; }
            set { usesShadow = value; }
        }

        /// <summary>
        /// World instance the entity belongs to
        /// </summary>
        protected MarsWorld world;

        /// <summary>
        /// Singleton random instance for entities
        /// </summary>
        protected static Random random = new Random();

        int id;

        /// <summary>
        /// ID number of this entity
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        float distanceValue;

        /// <summary>
        /// Temporary value stored for sorting distance
        /// </summary>
        public float DistanceValue
        {
            get { return distanceValue; }
            set { distanceValue = value; }
        }

        public Entity(MarsWorld world)
        {
            this.world = world;
            this.ID = MarsWorld.EntityCount++;
        }

        public Entity(MarsWorld world, Vector2 position)
        {
            this.world = world;
            this.position = position;
            this.ID = MarsWorld.EntityCount++;
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            // draw the collision rectangle if debugging
            if (MarsWorld.Debug)
            {
                var bounds = GetScreenRectangle();

                bounds.X += (int)camera.X;
                bounds.Y += (int)camera.Y;

                spriteBatch.Draw(MarsRTS.BlankTexture,
                    bounds,
                    bounds,
                    debugColor);
            }
        }

        public virtual void DrawShadow(SpriteBatch spriteBatch, Vector2 camera)
        {

        }

        /// <summary>
        /// Checks for collision with another entity
        /// </summary>
        public virtual bool CheckCollisionWithEntity(Entity e)
        {
            // don't want to check for collision with ourself
            if (e == this)
                return false;

            // do the radii collide
            return Vector2.DistanceSquared(GetCenter(), e.GetCenter()) <
                Math.Pow(collisionRadius + e.collisionRadius, 2);
        }

        /// <summary>
        /// Pushes the entity with the given vector
        /// </summary>
        /// <param name="amount"></param>
        public virtual void Push(Vector2 amount)
        {
            velocity += amount;
        }

        /// <summary>
        /// Flags entity for deletion
        /// </summary>
        public virtual void Delete()
        {
            removed = true;

            // invoke the deletion callback if it exists
            if (OnDeletion != null)
                OnDeletion(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the center of this entity on screen
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetCenter()
        {
            return position + (new Vector2(size.X, size.Y) / 2);
        }

        /// <summary>
        /// Gets the bounding rectangle of this entity on screen
        /// </summary>
        public virtual Rectangle GetScreenRectangle()
        {
            return new Rectangle()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Width = size.X,
                Height = size.Y
            };
        }

        /// <summary>
        /// Gets the the position of the nearest node (in node units)
        /// </summary>
        /// <returns></returns>
        public virtual Point GetNearestNode()
        {
            var node = world.GetNodePosition(position);

            // clamp values inside the world
            node.X = (int)MathHelper.Clamp(node.X, 0, MarsWorld.Size.X - 1);
            node.Y = (int)MathHelper.Clamp(node.Y, 0, MarsWorld.Size.Y - 1);

            return node;
        }

        public virtual int CompareTo(Entity b)
        {
            if (b == null)
                return 0;

            var compareA = position + GetSortOffset();
            var compareB = b.position + b.GetSortOffset();

            if (compareA.Y < compareB.Y)
                return -1;

            if (compareA.Y > compareB.Y)
                return 1;

            if (compareA.X < compareB.X)
                return -1;

            if (compareA.X > compareB.X)
                return 1;

            if (size.X < b.size.X)
                return -1;

            if (size.X > b.size.X)
                return 1;

            if (size.Y < b.size.Y)
                return -1;

            if (size.Y > b.size.Y)
                return 1;

            if ((type == EntityType.AI && b.type == EntityType.AI) ||
                (this is BuildingEntity && b is BuildingEntity &&
                ((BuildingEntity)this).BuildingType == Building.Barricade &&
                ((BuildingEntity)b).BuildingType == Building.Barricade))
            {
                return (ID - b.ID);
            }

            return 0;
        }

        /// <summary>
        /// Offset for sorting
        /// </summary>
        public virtual Vector2 GetSortOffset()
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Resolves collision with another entity
        /// </summary>
        protected virtual void resolveCollisionWithFixedEntity(Entity e)
        {
            e.velocity = Vector2.Zero;
        }
    }
}
