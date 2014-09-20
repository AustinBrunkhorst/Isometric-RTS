using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSAssetData;
using System;

namespace MarsRTS.LIB
{
    public class Animation
    {
        Texture2D texture;

        /// <summary>
        /// Texture representing the animation
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
        }

        TimeSpan frameDuration, frameChange;

        /// <summary>
        /// Duration of each frame
        /// </summary>
        public TimeSpan FrameDuration
        {
            get { return frameDuration; }
        }

        /// <summary>
        /// Frame center offset
        /// </summary>
        public readonly Vector2 Center;

        bool isPlaying;

        /// <summary>
        /// Determines if the animation is currently playing
        /// </summary>
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        bool isLooping;

        /// <summary>
        /// Determines if the animation should loop
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
        }

        public bool IsFinished
        {
            get { return frameIndex == frameCount - 1; }
        }

        int frameCount;

        /// <summary>
        /// Number of frames in the animation
        /// </summary>
        public int FrameCount
        {
            get { return frameCount; }
        }

        int frameWidth, frameHeight;

        /// <summary>
        /// Width of a single frame in the animation
        /// </summary>
        public int FrameWidth
        {
            // Assume square frames.
            get { return frameWidth; }
        }

        /// <summary>
        /// Height of a single frame in the animation
        /// </summary>
        public int FrameHeight
        {
            get { return frameHeight; }
        }

        /// <summary>
        /// Size of each frame in the animation
        /// </summary>
        public Vector2 FrameSize
        {
            get { return new Vector2(frameWidth, frameHeight); }
        }

        int frameIndex, framesPerLine;

        /// <summary>
        /// The current frame index
        /// </summary>
        public int FrameIndex
        {
            get { return frameIndex; }
            set
            {
                frameIndex = value;
                updateFrameRect();
            }
        }

        /// <summary>
        /// Copy of this animation at the current frame
        /// </summary>
        public Animation FrameCopy
        {
            get
            {
                return new Animation(texture, frameDuration, frameWidth, false)
                {
                    frameIndex = frameIndex,
                    isPlaying = false
                };
            }
        }

        public Animation(Texture2D texture)
        {
            this.texture = texture;
            this.frameCount = this.framesPerLine = 1;
            this.frameWidth = texture.Width;
            this.frameHeight = texture.Height;
            this.isLooping = false;

            Center = new Vector2(texture.Width, texture.Height) / 2;

            this.Reset();
        }

        public Animation(Texture2D texture, TimeSpan frameDuration, int frameWidth, bool isLooping)
        {
            this.texture = texture;
            this.frameDuration = frameDuration;
            this.frameCount = texture.Width / frameWidth;
            this.framesPerLine = texture.Width / frameWidth;
            this.frameWidth = frameWidth;
            this.frameHeight = texture.Height;
            this.isLooping = isLooping;

            Center = new Vector2(this.frameWidth, this.FrameHeight) / 2;

            this.Reset();
        }

        public Animation(Texture2D texture, TimeSpan frameDuration, int frameCount, Point frameSize, bool isLooping)
        {
            this.texture = texture;
            this.frameDuration = frameDuration;
            this.frameCount = frameCount;
            this.framesPerLine = texture.Width / frameSize.X;
            this.frameWidth = frameSize.X;
            this.frameHeight = frameSize.Y;
            this.isLooping = isLooping;

            Center = new Vector2(this.frameWidth, this.frameHeight) / 2;

            this.Reset();
        }

        Rectangle frameRect;

        /// <summary>
        /// Plays the animation
        /// </summary>
        public void Play()
        {
            isPlaying = true;
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Resets the animation
        /// </summary>
        public void Reset()
        {
            frameIndex = 0;

            frameChange = frameDuration;
            frameRect = new Rectangle(0, 0, frameWidth, frameHeight);
        }

        /// <summary>
        /// Update animation
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!isPlaying || frameCount == 1)
                return;

            frameChange -= gameTime.ElapsedGameTime;

            if (frameChange < TimeSpan.Zero)
            {
                if (isLooping)
                {
                    frameIndex = (frameIndex + 1) % frameCount;
                }
                else if (frameIndex + 1 < frameCount)
                {
                    frameIndex++;
                }

                frameChange = frameDuration;

                updateFrameRect();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color)
        {
            spriteBatch.Draw(texture, destinationRectangle, frameRect, color);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            spriteBatch.Draw(texture, position, frameRect, color);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            spriteBatch.Draw(texture, destinationRectangle, frameRect, color, rotation, origin, effects, layerDepth);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            spriteBatch.Draw(texture, position, frameRect, color, rotation, origin, scale, effects, layerDepth);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            spriteBatch.Draw(texture, position, frameRect, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Creates a new animation instance from animation data
        /// </summary>
        public static Animation FromData(AnimationData data)
        {
            var frameDuration = TimeSpan.FromMilliseconds(data.FrameDuration);

            if (data.FrameCount == 0)
            {
                return new Animation(data.Texture,
                    frameDuration,
                    data.FrameSize.X,
                    data.IsLooping);
            }

            return new Animation(data.Texture,
                frameDuration,
                data.FrameCount,
                data.FrameSize,
                data.IsLooping);
        }

        void updateFrameRect()
        {
            if (framesPerLine == 0)
                return;

            frameRect.Location = new Point()
            {
                X = (frameIndex % framesPerLine) * frameWidth,
                Y = (frameIndex / framesPerLine) * frameHeight
            };
        }

        Rectangle getDestinationRect(Vector2 position)
        {
            return new Rectangle((int)position.X,
                (int)position.Y,
                frameWidth,
                texture.Height);
        }
    }
}
