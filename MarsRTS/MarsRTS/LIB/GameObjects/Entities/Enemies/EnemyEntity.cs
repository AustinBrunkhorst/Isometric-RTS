using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.GameObjects.Entities.Fixed;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSAssetData.Buildings;
using RTSAssetData.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsRTS.LIB.GameObjects.Entities.Enemies
{
    public class EnemyEntity : HealthEntity
    {
        /// <summary>
        /// Animations available to this enemy
        /// </summary>
        Dictionary<string, Animation> animations =
            new Dictionary<string, Animation>();

        /// <summary>
        /// Properties of this enemy
        /// </summary>
        EnemyProperties properties;

        /// <summary>
        /// Enemy walking speed
        /// </summary>
        float walkSpeed;

        /// <summary>
        /// Angle faced for walking
        /// </summary>
        float walkAngle;

        /// <summary>
        /// Duration since the enemy last attacked its target building
        /// </summary>
        TimeSpan lastAttacked;

        /// <summary>
        /// Interval for this enemy to attack
        /// </summary>
        TimeSpan attackInterval;

        /// <summary>
        /// Current animation to draw
        /// </summary>
        string currentAnimation = "Idle";

        /// <summary>
        /// Shadow animation respective to the current animation
        /// </summary>
        string shadowAnimation = "ShadowIdle";

        /// <summary>
        /// Building destintation path for this enemy
        /// </summary>
        List<Point> path = new List<Point>();

        /// <summary>
        /// Building that is being targeted for attacks
        /// </summary>
        BuildingEntity targetBuilding;

        /// <summary>
        /// Determines if this enenmy is entering from outside the bounds of
        /// the map
        /// </summary>
        bool isEntering;

        /// <summary>
        /// determines if this enemy has a target to move to, and attack
        /// </summary>
        bool hasTarget;

        /// <summary>
        /// Determines if this enemy is attacking a building
        /// </summary>
        bool attacking;

        /// <summary>
        /// Minimum frame for walking animations (left)
        /// </summary>
        int minFrame;

        /// <summary>
        /// Maximum frame for walking animations (right)
        /// </summary>
        int maxFrame = 1;

        /// <summary>
        /// Idle frame to use when the enemy reaches a target destination
        /// </summary>
        int destinationFrame;

        /// <summary>
        /// Duration before the walk animation alternates
        /// </summary>
        TimeSpan lastWalkAlternation;

        public EnemyEntity(MarsWorld world, EnemyProperties properties)
            : base(world)
        {
            this.Type = EntityType.AI;

            this.properties = properties;

            this.walkSpeed = random.Next(properties.WalkspeedRange.X,
                properties.WalkspeedRange.Y);

            this.attackInterval =
                TimeSpan.FromMilliseconds(properties.AttackInterval);

            this.Health = GetMaxHealth();

            IsCollidable = true;

            healthBar = new HealthBar(properties.HealthBarSize);

            // delete the enemy
            OnDeath += (a, b) => { Delete(); };

            // initialize animations
            initAnimations();
        }

        /// <summary>
        /// Update enemy logic
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // update animations
            updateAnimations(gameTime);

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // if the enemy doesn't have a target and isn't entering,
            // find a new target building
            if (!isEntering && !hasTarget)
            {
                aquireNewTargetBuilding();
            }

            float speed = delta * walkSpeed;

            // follow target path if the enemy isn't attacking
            if (!attacking && path.Count > 1)
            {
                // get the position of our current target
                var target = world.Grid[path[0]].Position +
                    properties.TargetNodeOffset;

                // set our enemy walking
                currentAnimation = "Walk";
                shadowAnimation = "ShadowWalk";

                if (Vector2.Distance(target, position) <= speed)
                {
                    // we aren't entering anymore
                    isEntering = false;

                    path.RemoveAt(0);

                    // new node
                    var node = world.Grid[path[0]];

                    // new target position
                    var newTarget = node.Position +
                        properties.TargetNodeOffset;

                    if (node.Occupant != null &&
                        node.Occupant.BuildingType == Building.Barricade)
                    {
                        var tmpBuilding = targetBuilding;

                        targetBuilding = node.Occupant;
                        attacking = true;

                        targetBuilding.OnDeath += delegate
                        {
                            if (targetBuilding != null)
                                targetBuilding.Delete();

                            targetBuilding = tmpBuilding;

                            attacking = false;
                            hasTarget = true;

                            setTargetPosition(newTarget);
                        };
                    }
                    else
                    {
                        // set the new target position
                        setTargetPosition(newTarget);
                    }

                    // reached the destination, start attacking
                    if (path.Count == 1)
                    {
                        attacking = true;

                        // set our idle animation
                        currentAnimation = "Idle";
                        shadowAnimation = "ShadowIdle";

                        // set our idle animation frame to the frame 
                        // prescribed by the building attack node
                        setIdleFrame(destinationFrame);
                    }
                }
            }
            else if (attacking)
            {
                lastAttacked -= gameTime.ElapsedGameTime;

                if (lastAttacked <= TimeSpan.Zero)
                {
                    lastAttacked = attackInterval;

                    // attack the target building, we've reached our attack
                    // interval
                    attackTargetBuilding();
                }

                // make sure we're idle
                velocity = Vector2.Zero;
            }

            // move the enemy based on our velocity
            position += velocity * speed;

            // update enemy bullet logic
            checkBulletCollision();
        }

        /// <summary>
        /// Draw the enemy
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            // draw our current animation
            animations[currentAnimation].Draw(spriteBatch,
                position + camera,
                tint);

            base.Draw(spriteBatch, camera);
        }

        /// <summary>
        /// Draw the enemy shadow
        /// </summary>
        public override void DrawShadow(SpriteBatch spriteBatch, Vector2 camera)
        {
            // draw our current animation's shadow
            animations[shadowAnimation].Draw(spriteBatch,
                position + camera,
                tint);
        }

        /// <summary>
        /// Offset to use for sorting
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetSortOffset()
        {
            return properties.DepthPointOffset;
        }

        /// <summary>
        /// Gets the offset for defense towers to shoot at
        /// </summary>
        /// <returns></returns>
        public Vector2 GetAttackOffset()
        {
            return position + properties.AttackOffset;
        }

        /// <summary>
        /// Gets the screen rectangle for this enemy
        /// </summary>
        public override Rectangle GetScreenRectangle()
        {
            var rectangle = properties.ScreenRectangle;

            rectangle.X += (int)position.X;
            rectangle.Y += (int)position.Y;

            return rectangle;
        }

        /// <summary>
        /// Gets the max health of this enemy
        /// </summary>
        public override float GetMaxHealth()
        {
            return properties.MaxHealth;
        }

        /// <summary>
        /// Sets a path of nodes for this enemy to walk
        /// </summary>
        /// <param name="nodes"> Target path</param>
        public void SetPath(List<Point> nodes)
        {
            path = nodes;
            /*path.Clear();

            // skip even numbers to allow for smooth walking
            for (int i = 0; i < nodes.Count; i++)
                if (i > nodes.Count - 3 || i % 2 == 0)
                    path.Add(nodes[i]);*/

            // make sure we set our target
            hasTarget = true;
        }

        public void SetEntryTarget(Point node)
        {
            // set entry flag
            isEntering = true;

            var target = world.Grid[node].Position +
                properties.TargetNodeOffset;

            // set the target position of our node
            setTargetPosition(target);
        }

        public override Vector2 GetHealthBarPosition()
        {
            return position + properties.HealthBarOffset;
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

        BuildingEntity findClosestBuilding()
        {
            // init targets list
            List<Entity> targets;

            // buildings in world
            var buildings = world.GetEntities(EntityType.Building);

            // try to find buildings with the target category
            var specific = buildings.Cast<BuildingEntity>().
                Where(
                    b => b.Category == properties.PreferredCategory &&
                         b.BuildingType != Building.Barricade
                ).Cast<Entity>().ToList();

            // use the specific list if it exists
            if (specific.Count > 0)
                targets = specific;
            else
                targets = buildings;

            Entity entity = null;
            float closest = float.MaxValue;

            // iterate though the buildings we gathered and find the closest
            // one
            foreach (BuildingEntity building in targets)
            {
                if (building.IsRemoved ||
                    building.BuildingType == Building.Barricade)
                    continue;

                float distance = Vector2.Distance(position, building.Position);

                if (distance < closest)
                {
                    closest = distance;
                    entity = building;
                }
            }

            return (BuildingEntity)entity;
        }

        /// <summary>
        /// Calculates the angle between two vectors
        /// </summary>
        float angleBetween(Vector2 a, Vector2 b)
        {
            var delta = a - b;

            return (float)Math.Atan2(delta.Y, delta.X);
        }

        /// <summary>
        /// Acquires a new target building, and starts walking towards it
        /// </summary>
        void aquireNewTargetBuilding()
        {
            var building = findClosestBuilding();

            // we couldn't find any buildings
            if (building == null)
            {
                Delete();

                return;
            }

            setTargetBuilding(building);

            var attackNode = targetBuilding.GetAttackTargetNode();

            // we couldn't find any target nodes for this building
            if (attackNode == null)
            {
                aquireNewTargetBuilding();

                return;
            }

            // calculate the shortest path between our position, and the 
            // target destination
            var nodes = world.Grid.FindPath(GetNearestNode(),
                attackNode.Destination, false);

            if (nodes.Count == 0)
                return;

            destinationFrame = attackNode.IdleFrame;

            // set our path to the calculated path
            SetPath(nodes);

            var target = world.Grid[nodes[0]].Position +
                properties.TargetNodeOffset;

            setTargetPosition(target);
        }

        /// <summary>
        /// Attacks the building targeted
        /// </summary>
        void attackTargetBuilding()
        {
            targetBuilding.Hurt(properties.AttackDamage);
        }

        /// <summary>
        /// Sets the new target building
        /// </summary>
        /// <param name="building"> Target building </param>
        void setTargetBuilding(BuildingEntity building)
        {
            building.OnDeath += (a, b) =>
            {
                if (targetBuilding != null)
                    targetBuilding.Delete();

                hasTarget = attacking = false;
            };

            targetBuilding = building;
        }

        /// <summary>
        /// Calculates angles and such between a target point for the enemy
        /// </summary>
        /// <param name="target"> Target position </param>
        void setTargetPosition(Vector2 target)
        {
            walkAngle = angleBetween(target, position);

            velocity = Vector2Extensions.FromAngle(walkAngle);

            setWalkAngle(walkAngle + MathHelper.PiOver2);
        }

        /// <summary>
        /// Checks for collision between bullets
        /// </summary>
        void checkBulletCollision()
        {
            var bullets = world.GetProjectiles().Cast<Bullet>();
            var screenRect = GetScreenRectangle();

            // iterate though all projectiles and check for collision
            foreach (var bullet in bullets)
            {
                var rect = GetScreenRectangle();

                if (screenRect.Intersects(bullet.GetBounds()))
                {
                    Hurt(bullet.Damage);

                    var near = world.GetEntitiesInRange(position,
                        bullet.ExplosionRadius, EntityType.AI).
                        Cast<EnemyEntity>();

                    foreach (var enemy in near)
                    {
                        var distance =
                            Vector2.Distance(enemy.position, position);

                        if (distance < bullet.ExplosionRadius)
                        {
                            enemy.Hurt(bullet.Damage * 0.75f);
                        }
                    }

                    bullet.Delete();
                }
            }
        }

        /// <summary>
        /// Initializes animations
        /// </summary>
        void initAnimations()
        {
            // load animations from their data in our properties
            foreach (var animation in properties.Animations)
            {
                animations[animation.Key] = Animation.FromData(animation.Value);
            }

            // don't need the animation data anymore
            properties.Animations = null;

            UsesShadow = true;
        }

        /// <summary>
        /// Updates enemy animations
        /// </summary>
        void updateAnimations(GameTime gameTime)
        {
            if (currentAnimation == "Walk")
            {
                var walk = animations["Walk"];

                lastWalkAlternation -= gameTime.ElapsedGameTime;

                if (lastWalkAlternation <= TimeSpan.Zero)
                {
                    walk.FrameIndex = (walk.FrameIndex == minFrame) ?
                        maxFrame : minFrame;

                    lastWalkAlternation = walk.FrameDuration;
                }
            }

            animations[shadowAnimation].FrameIndex =
                animations[currentAnimation].FrameIndex;
        }

        /// <summary>
        /// Updates enemy walking angle
        /// </summary>
        /// <param name="angle"> Target walk angle</param>
        void setWalkAngle(float angle)
        {
            // convert angle to degrees
            var degrees = MathHelper.ToDegrees(angle);

            // find reference angle for negative angles
            if (degrees < 0)
                degrees = 360 + degrees;

            // find the nearest 45 degree angle for this angle. Used for
            // finding the respective walking animation frame index
            var nearest = (int)Math.Floor(degrees / 45) * 2;

            minFrame = nearest;
            maxFrame = nearest + 1;
        }

        /// <summary>
        /// Sets the frame of the idle index and animation
        /// </summary>
        /// <param name="frame"> Target idle animation frame </param>
        void setIdleFrame(int frame)
        {
            animations["Idle"].FrameIndex =
                animations["ShadowIdle"].FrameIndex = frame;
        }
    }
}
