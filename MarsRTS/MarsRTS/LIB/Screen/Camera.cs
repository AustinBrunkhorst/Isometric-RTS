using MarsRTS.LIB.Extensions;
using Microsoft.Xna.Framework;
using System;

namespace MarsRTS.LIB.Screen
{
    public class Camera
    {
        Vector2 position, target, speed, shakeSpeed;

        public Vector2 Position
        {
            get
            {
                float dx = (float)(shakeAnimation * Math.PI * shakeSpeed.X),
                      dy = (float)(shakeAnimation * Math.PI * shakeSpeed.Y),
                      sx = (float)Math.Round(Math.Cos(dx) * shakeAmplitude),
                      sy = (float)Math.Round(Math.Sin(dy) * shakeAmplitude);

                return position + new Vector2(sx, sy * 0.7f);
            }

            set
            {
                position.X = MathHelper.Clamp(value.X, min.X, max.X);
                position.Y = MathHelper.Clamp(value.Y, min.Y, max.Y);
            }
        }

        /// <summary>
        /// Absolute position without shake
        /// </summary>
        public Vector2 RawPosition
        {
            get { return position; }
        }

        public Vector2 Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public Vector2 ShakeSpeed
        {
            get { return shakeSpeed; }
            set { shakeSpeed = value; }
        }

        Point min, max;

        public Point Min
        {
            get { return min; }
            set { min = value; }
        }

        public Point Max
        {
            get { return max; }
            set { max = value; }
        }

        float shakeAmplitude, shakeAnimation;

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Position += (target - position) * delta * speed;

            shakeAnimation += delta;
            shakeAmplitude += -shakeAmplitude * delta * 2.5f;
        }

        /// <summary>
        /// Sets the camera's target position
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Vector2 target)
        {
            this.target = target;
        }

        /// <summary>
        /// Sets the restraints of the camera position
        /// </summary>
        /// <param name="min"> Minimum position </param>
        /// <param name="max"> Maximum position </param>
        public void SetConstraints(Point min, Point max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Moves the camera with the given vector
        /// </summary>
        /// <param name="delta"> Move offset </param>
        public void Move(Vector2 offset)
        {
            Position += offset;
        }

        /// <summary>
        /// Shakes the screen with the give amount
        /// </summary>
        /// <param name="amount"></param>
        public void Shake(float amount)
        {
            shakeAmplitude += amount;
        }

        /// <summary>
        /// Converts a point in world to a relative screen position
        /// </summary>
        /// <param name="objPosition"> Position of the object in world </param>
        public Vector2 ScreenOffset(Vector2 objPosition, bool raw = true)
        {
            // subtract raw if flag is set, otherwise the position with shake
            return objPosition - (raw ? position : Position);
        }

        /// <summary>
        /// Converts a point in world to a relative screen position
        /// </summary>
        /// <param name="objPosition"> Position of the object in world </param>
        public Vector2 ScreenOffset(Point objPosition, bool raw = true)
        {
            // subtract raw if flag is set, otherwise the position with shake
            return objPosition.ToVector() - (raw ? position : Position);
        }
    }
}
