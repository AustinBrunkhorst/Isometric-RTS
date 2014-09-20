using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RTSAssetData.Buildings;
using System.Collections.Generic;

namespace RTSAssetData.Enemies
{
    public struct EnemyProperties
    {
        Enemy type;

        public Enemy Type
        {
            get { return type; }
            set { type = value; }
        }

        BuildingCategory preferredCategory;

        /// <summary>
        /// Building category that this enemy will target with priority
        /// </summary>
        public BuildingCategory PreferredCategory
        {
            get { return preferredCategory; }
            set { preferredCategory = value; }
        }

        Vector2 healthBarOffset, targetNodeOffset, depthPointOffset,
            attackOffset;

        /// <summary>
        /// Offset to draw the building's healthbar
        /// </summary>
        public Vector2 HealthBarOffset
        {
            get { return healthBarOffset; }
            set { healthBarOffset = value; }
        }

        /// <summary>
        /// Offset for target node positions
        /// </summary>
        public Vector2 TargetNodeOffset
        {
            get { return targetNodeOffset; }
            set { targetNodeOffset = value; }
        }

        /// <summary>
        /// Offset to compare enemy for depth sorting
        /// </summary>
        public Vector2 DepthPointOffset
        {
            get { return depthPointOffset; }
            set { depthPointOffset = value; }
        }

        /// <summary>
        /// Offset for buildings to shoot at
        /// </summary>
        public Vector2 AttackOffset
        {
            get { return attackOffset; }
            set { attackOffset = value; }
        }

        Point walkspeedRange, size, healthBarSize;

        /// <summary>
        /// Min/Max walk speeds (x = min, y = max)
        /// </summary>
        public Point WalkspeedRange
        {
            get { return walkspeedRange; }
            set { walkspeedRange = value; }
        }

        /// <summary>
        /// Size of this enemy's screen rectangle
        /// </summary>
        public Point Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Size of the enemie's healthbar
        /// </summary>
        public Point HealthBarSize
        {
            get { return healthBarSize; }
            set { healthBarSize = value; }
        }

        Rectangle screenRectangle;

        /// <summary>
        /// Screen rectangle offset for the enemy
        /// </summary>
        public Rectangle ScreenRectangle
        {
            get { return screenRectangle; }
            set { screenRectangle = value; }
        }

        float maxHealth, attackInterval, attackDamage;

        /// <summary>
        /// Represents the max health of this entity
        /// </summary>
        public float MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        /// <summary>
        /// Interval in milliseconds the enemy attacks
        /// </summary>
        public float AttackInterval
        {
            get { return attackInterval; }
            set { attackInterval = value; }
        }

        /// <summary>
        /// Damage dealt each attack
        /// </summary>
        public float AttackDamage
        {
            get { return attackDamage; }
            set { attackDamage = value; }
        }

        Dictionary<string, AnimationData> animations;

        [ContentSerializer(CollectionItemName = "Animation")]
        public Dictionary<string, AnimationData> Animations
        {
            get { return animations; }
            set { animations = value; }
        }

        public void LoadAnimations(ContentManager content)
        {
            foreach (var animation in animations)
            {
                animation.Value.Load(content);
            }
        }
    }
}
