using MarsRTS.LIB.Extensions;
using Microsoft.Xna.Framework;

namespace MarsRTS.LIB.GameObjects.Entities
{
    public class Bullet : Projectile
    {
        int explosionRadius;

        /// <summary>
        /// Radius for explosions
        /// </summary>
        public int ExplosionRadius
        {
            get { return explosionRadius; }
            set { explosionRadius = value; }
        }

        public Bullet(MarsWorld world, Vector2 position, Animation animation)
            : base(world, position)
        {
            this.position = position;
            this.animation = animation;
        }

        public override void Update(GameTime gameTime)
        {
            // delete bullets outside the world
            if (!world.Bounds.Contains(GetBounds()))
                Delete();

            base.Update(gameTime);
        }

        /// <summary>
        /// Gets the bounding rectangle of this bullet
        /// </summary>
        public Rectangle GetBounds()
        {
            var size = animation.FrameSize;

            return new Rectangle()
            {
                Location = position.ToPoint(),
                Width = (int)size.X,
                Height = (int)size.Y
            };
        }
    }
}
