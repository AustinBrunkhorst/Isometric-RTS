using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTSAssetData
{
    public class AnimationData
    {
        string texturePath;

        /// <summary>
        /// Content path to the animation's texture
        /// </summary>
        public string TexturePath
        {
            get { return texturePath; }
            set { texturePath = value; }
        }

        Point frameSize;

        /// <summary>
        /// Size in pixels of each animation frame
        /// </summary>
        public Point FrameSize
        {
            get { return frameSize; }
            set { frameSize = value; }
        }

        int frameCount, frameDuration;

        /// <summary>
        /// Number of frames in this animation
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int FrameCount
        {
            get { return frameCount; }
            set { frameCount = value; }
        }

        /// <summary>
        /// Duration in milliseconds for each animation frame
        /// </summary>
        public int FrameDuration
        {
            get { return frameDuration; }
            set { frameDuration = value; }
        }

        bool isLooping;

        /// <summary>
        /// Determines if the animation loops or not
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set { isLooping = value; }
        }

        Texture2D texture;

        [ContentSerializerIgnore]
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        /// <summary>
        /// Load animation texture data
        /// </summary>
        /// <param name="content"> Content manager </param>
        public void Load(ContentManager content)
        {
            texture = content.Load<Texture2D>(texturePath);
        }
    }
}
