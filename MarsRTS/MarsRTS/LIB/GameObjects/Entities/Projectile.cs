using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.GameObjects.Entities
{
    public class Projectile : Entity
    {
        protected float speed;

        /// <summary>
        /// Projectile movement speed
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        protected int damage;

        /// <summary>
        /// Damage dealt from this projectile
        /// </summary>
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        protected Animation animation;

        public Animation Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        public Projectile(MarsWorld world, Vector2 position)
            : base(world, position)
        {
            IsCollidable = true;
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            position += velocity * delta * speed;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 camera)
        {
            animation.Draw(spriteBatch, position + camera, Tint);
        }
    }
}
