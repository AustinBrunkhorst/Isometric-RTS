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
        ///// <summary>
        ///     Content path to the animation's texture
        /// </summary>th; }
            set { texturePath = value; }
        }

        Point frameSize;

        /// <summary>
        /// Size in pixels of each anim/// <summary>
        ///     Size in pixels of each animation frame
        /// </summary>et { return frameSize; }
            set { frameSize = value; }
        }

        int frameCount, frameDuration;

        /// <summary>
 /// <summary>
        ///     Number of frames in this animation
        /// </summary>rializer(Optional = true)]
        public int FrameCount
        {
            get { return frameCou/// <summary>
        ///     Duration in milliseconds for each animation frame
        /// </summary> in milliseconds for each animation frame
        /// </sum/// <summary>
        ///     Determines if the animation loops or not
        /// </summary>            set { frameDuration = value; }
        }

        bool isLooping;

        /// <summary>
        /// Determines if the animation loops or not
        /// </summary>
        public bool IsLooping
        {/// <summary>
        ///     Load animation texture data
        /// </summary>
        /// <param name="content"> Content manager </param>ializerIgnore]
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
