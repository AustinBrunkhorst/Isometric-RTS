using Microsoft.Xna.Framework.Content;

namespace RTSAssetData.Buildings
{
    public struct BuildingUpgradeData
    {
        string description;

        [ContentSerializer(Optional = true)]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        int range, defenseInterval, bulletSpeed, damage, attackExplosionRange,
            maxHealth, repairDuration, maxResourceBank;

        [ContentSerializer(Optional = true)]
        public int Range
        {
            get { return range; }
            set { range = value; }
        }

        [ContentSerializer(Optional = true)]
        public int DefenseInterval
        {
            get { return defenseInterval; }
            set { defenseInterval = value; }
        }

        [ContentSerializer(Optional = true)]
        public int BulletSpeed
        {
            get { return bulletSpeed; }
            set { bulletSpeed = value; }
        }

        [ContentSerializer(Optional = true)]
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        [ContentSerializer(Optional = true)]
        public int AttackExplosionRange
        {
            get { return attackExplosionRange; }
            set { attackExplosionRange = value; }
        }

        [ContentSerializer(Optional = true)]
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        [ContentSerializer(Optional = true)]
        public int RepairDuration
        {
            get { return repairDuration; }
            set { repairDuration = value; }
        }

        [ContentSerializer(Optional = true)]
        public int MaxResourceBank
        {
            get { return maxResourceBank; }
            set { maxResourceBank = value; }
        }
    }
}
