using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace RTSAssetData
{
    public class SoundData
    {
        SoundEffect sound;

        [ContentSerializerIgnore]
        public SoundEffect Sound
        {
            get { return sound; }
            set { sound = value; }
        }

        string soundPath;

        /// <summary>
        /// Asset path for the sound
        /// </summary>
        public string SoundPath
        {
            get { return soundPath; }
            set { soundPath = value; }
        }

        public void Load(ContentManager content)
        {
            this.sound = content.Load<SoundEffect>(soundPath);
        }
    }
}
