using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace RTSAssetData.Buildings
{
    /// <summary>
    /// Represents various building properties
    /// </summary>
    public struct BuildingProperties
    {
        Building type;

        /// <summary>
        /// Building type
        /// </summary>
        public Building Type
        {
            get { return type; }
            set { type = value; }
        }

        ResourceType resource;

        /// <summary>
        /// Building resource type (if applicable)
        /// </summary>
        [ContentSerializer(Optional = true)]
        public ResourceType Resource
        {
            get { return resource; }
            set { resource = value; }
        }

        string description, iconTexture;

        /// <summary>
        /// Building description
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Texture path for this building's HUD icon
        /// </summary>
        public string IconTexture
        {
            get { return iconTexture; }
            set { iconTexture = value; }
        }

        BuildingCategory category;

        /// <summary>
        /// Category this building belongs in
        /// </summary>
        public BuildingCategory Category
        {
            get { return category; }
            set { category = value; }
        }

        int level, maxLevel, minRange;

        /// <summary>
        /// Current upgrade level
        /// </summary>
        [ContentSerializerIgnore]
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        /// <summary>
        /// Maximum upgrade level
        /// </summary>
        public int MaxLevel
        {
            get { return maxLevel; }
            set { maxLevel = value; }
        }

        /// <summary>
        /// Minimum range for a building to shoot at enemies
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int MinRange
        {
            get { return minRange; }
            set { minRange = value; }
        }

        Vector2 gridOffset, depthPointOffset, healthBarOffset,
            projectileOffset, barrelOffset;

        /// <summary>
        /// Animation position offset in world
        /// </summary>
        public Vector2 GridOffset
        {
            get { return gridOffset; }
            set { gridOffset = value; }
        }

        /// <summary>
        /// Offset to compare building for sorting
        /// </summary>
        public Vector2 DepthPointOffset
        {
            get { return depthPointOffset; }
            set { depthPointOffset = value; }
        }

        /// <summary>
        /// Offset to draw the building's healthbar
        /// </summary>
        public Vector2 HealthBarOffset
        {
            get { return healthBarOffset; }
            set { healthBarOffset = value; }
        }

        /// <summary>
        /// Offset to emit projectiles from this building
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector2 ProjectileOffset
        {
            get { return projectileOffset; }
            set { projectileOffset = value; }
        }

        /// <summary>
        /// Offset to calculate angle between the building and an enemy
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector2 BarrelOffset
        {
            get { return barrelOffset; }
            set { barrelOffset = value; }
        }

        Point healthBarSize;

        /// <summary>
        /// Size of the building's healthbar
        /// </summary>
        public Point HealthBarSize
        {
            get { return healthBarSize; }
            set { healthBarSize = value; }
        }

        Rectangle gridBounds, screenRectangle;

        /// <summary>
        /// Bounds in world that this building consumes
        /// </summary>
        public Rectangle GridBounds
        {
            get { return gridBounds; }
            set { gridBounds = value; }
        }

        /// <summary>
        /// Rectangle offset representing the visible bounding rectangle on
        /// screen
        /// </summary>
        public Rectangle ScreenRectangle
        {
            get { return screenRectangle; }
            set { screenRectangle = value; }
        }

        bool isPassible;

        /// <summary>
        /// Determines if this building will make the bounds in world that 
        /// it consumes passible
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool IsPassible
        {
            get { return isPassible; }
            set { isPassible = value; }
        }

        /// <summary>
        /// Represents offsets for each frame to shoot bullets from on 
        /// defensive buildings
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector2[] BarrelOffsets;

        [ContentSerializer(ElementName = "UpgradeCosts", CollectionItemName = "Cost")]
        EntityCost[] upgradeCosts;

        [ContentSerializer(ElementName = "UpgradeData", CollectionItemName = "Upgrade")]
        BuildingUpgradeData[] upgradeData;

        /// <summary>
        /// Upgrade data for the current level
        /// </summary>
        [ContentSerializerIgnore]
        public BuildingUpgradeData UpgradeData
        {
            get { return upgradeData[level]; }
        }

        Dictionary<string, AnimationData> animations;

        /// <summary>
        /// Building animations
        /// </summary>
        [ContentSerializer(CollectionItemName = "Animation")]
        public Dictionary<string, AnimationData> Animations
        {
            get { return animations; }
            set { animations = value; }
        }

        Dictionary<string, SoundData> sounds;

        /// <summary>
        /// Building sounds
        /// </summary>
        [ContentSerializer(Optional = true, CollectionItemName = "Sound")]
        public Dictionary<string, SoundData> Sounds
        {
            get { return sounds; }
            set { sounds = value; }
        }

        Color inventoryColor;

        /// <summary>
        /// Color this building will be represented with in inventory
        /// </summary>
        public Color InventoryColor
        {
            get { return inventoryColor; }
            set { inventoryColor = value; }
        }

        /// <summary>
        /// Gets the cost of an upgrade
        /// </summary>
        /// <param name="level"> Target upgrade level. -1 uses the next level's
        /// cost </param>
        public EntityCost GetUpgradeCost(int level = -1)
        {
            return upgradeCosts[level == -1 ? this.level + 1 : level];
        }

        /// <summary>
        /// Get building upgrade data
        /// </summary>
        /// <param name="level"> Target upgrade level. -1 uses the next 
        /// upgrade level </param>
        public BuildingUpgradeData GetUpgradeData(int level = -1)
        {
            return upgradeData[level == -1 ? this.level + 1 : level];
        }

        /// <summary>
        /// Upgrades this building
        /// </summary>
        /// <param name="newLevel"> Target level if any </param>
        public void Upgrade(int newLevel = -1)
        {
            level = (newLevel == -1) ?
                Math.Min(maxLevel - 1, level + 1) : newLevel;
        }

        /// <summary>
        /// Load animation textures
        /// </summary>
        /// <param name="content"> Content manager </param>
        public void LoadAnimations(ContentManager content)
        {
            foreach (var animation in animations)
            {
                animation.Value.Load(content);
            }
        }

        /// <summary>
        /// Loads building sounds
        /// </summary>
        /// <param name="content"> Content manager </param>
        public void LoadSounds(ContentManager content)
        {
            if (sounds == null)
                return;

            foreach (var sound in sounds)
            {
                sound.Value.Load(content);
            }
        }
    }
}
