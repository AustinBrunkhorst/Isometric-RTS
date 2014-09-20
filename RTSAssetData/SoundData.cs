using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace RTSAssetData
{
    public class SoundData
    {
        private SoundEffect sound;

        private string soundPath;

        [ContentSerializerIgnore]
        public SoundEffect Sound
        {
            get { return sound; }
            set { sound = value; }
        }

        /// <summary>
        ///     Asset path for the sound
        /// </summary>
        public string SoundPath
        {
            get { return soundPath; }
            set { soundPath = value; }
        }

        public void Load(ContentManager content)
        {
            sound = content.Load<SoundEffect>(soundPath);
        }
    }
}