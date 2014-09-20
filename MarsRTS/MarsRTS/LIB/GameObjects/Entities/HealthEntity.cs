using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB.GameObjects.Entities
{
    public class HealthEntity : Entity
    {
        public EventHandler OnDeath;

        float health;

        public float Health
        {
            get { return health; }
            set
            {
                float before = health;

                health = value;

                if (before != value)
                    updateHealth();
            }
        }

        readonly Color emptyColor = new Color(255, 18, 0);
        readonly Color midColor = new Color(246, 253, 0);
        readonly Color fullColor = new Color(11, 255, 0);

        Color healthColor;

        bool drawHealthBar = true;

        protected HealthBar healthBar;

        public HealthEntity(MarsWorld world)
            : base(world)
        {
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            if (drawHealthBar)
            {
                healthBar.Draw(spriteBatch,
                    GetHealthBarPosition() + camera,
                    healthColor);
            }

            base.Draw(spriteBatch, camera);
        }

        /// <summary>
        /// Sets the entities current health
        /// </summary>
        /// <param name="health"> Target health </param>
        public void SetHealth(float health)
        {
            Health = health;
        }

        /// <summary>
        /// Hurts the entity
        /// </summary>
        /// <param name="amount"> Damage amount </param>
        public void Hurt(float amount)
        {
            Health -= amount;

            if (health <= 0 && OnDeath != null)
                OnDeath(this, EventArgs.Empty);

            Health = Math.Max(0, health);
        }

        /// <summary>
        /// Heals the entity 
        /// </summary>
        /// <param name="amount"> Heal amount </param>
        public void Heal(float amount)
        {
            Health = Math.Min(GetMaxHealth(), health + amount);
        }

        public void ShowHealthBar()
        {
            drawHealthBar = true;
        }

        public void HideHealthBar()
        {
            drawHealthBar = false;
        }

        public float GetHealthPercentage()
        {
            return health / GetMaxHealth();
        }

        /// <summary>
        /// Entities max health
        /// </summary>
        public virtual float GetMaxHealth()
        {
            return 100.0f;
        }

        /// <summary>
        /// Position to draw the healthbar
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetHealthBarPosition()
        {
            return position;
        }

        void updateHealth()
        {
            drawHealthBar = health != GetMaxHealth();

            float percentage = GetHealthPercentage();

            Color max, min;

            if (percentage >= 0.5f)
            {
                max = fullColor;
                min = midColor;
            }
            else
            {
                max = midColor;
                min = emptyColor;
            }

            healthColor = Color.Lerp(min, max, MathHelper.SmoothStep(0, 1, percentage));

            if (healthBar != null)
                healthBar.SetPercentage(percentage);
        }
    }
}
