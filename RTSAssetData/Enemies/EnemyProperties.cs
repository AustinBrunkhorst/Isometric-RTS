using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RTSAssetData.Buildings;

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
            set /// <summary>
        ///     Building category that this enemy will target with priority
        /// </summary>
            attackOffset;

        /// <summary>
        /// Offset to dr/// <summary>
        ///     Offset to draw the building's healthbar
        /// </summary>       {
            get { return healthBarOffset; }
            set { healthBarOffset = value; }
        }

        /// <summary>
        /// Offset for targe/// <summary>
        ///     Offset for target node positions
        /// </summary>    {
            get { return targetNodeOffset; }
            set { targetNodeOffset = value; }
        }

        /// <summary>
        /// Offset to compare en/// <summary>
        ///     Offset to compare enemy for depth sorting
        /// </summary>{
            get { return depthPointOffset; }
            set { depthPointOffset = value; }
        }

        /// <summary>
        /// Offset for buildings to /// <summary>
        ///     Offset for buildings to shoot at
        /// </summary>       get { return attackOffset; }
            set { attackOffset = value; }
        }

        Point walkspeedRange, size, healthBarSize;

        //// <summary>
        ///     Min/Max walk speeds (x = min, y = max)
        /// </summary>     public Point WalkspeedRange
        {
            get { return walkspeedRange; }
            set { walkspeedRange = value; }
        }

        /// </// <summary>
        ///     Size of this enemy's screen rectangle
        /// </summary> public Point Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summ/// <summary>
        ///     Size of the enemie's healthbar
        /// </summary>lic Point HealthBarSize
        {
            get { return healthBarSize; }
            set { healthBarSize = value; }
        }

        Rectangle scr/// <summary>
        ///     Screen rectangle offset for the enemy
        /// </summary>       /// </summary>
        public Rectangle ScreenRectangle
        {
            get { return screenRectangle; }
            set { screenRectangle = value; }
/// <summary>
        ///     Represents the max health of this entity
        /// </summary>       /// Represents the max health of this entity
     /// <summary>
        ///     Interval in milliseconds the enemy attacks
        /// </summary> }
            set { maxHealth = value; }
        }

      /// <summary>
        ///     Damage dealt each attack
        /// </summary>  /// </summary>
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
