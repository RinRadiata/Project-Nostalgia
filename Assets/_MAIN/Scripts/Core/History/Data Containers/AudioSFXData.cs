using System.Collections.Generic;
using UnityEngine;

namespace History
{
    [System.Serializable]
    public class AudioSFXData
    {
        public string filePath;
        public string fileName;
        public float volume;
        public float pitch;

        //capture all the looping sfx as long as we make sure they are looping
        public static List<AudioSFXData> Capture() 
        {
            List<AudioSFXData> audioList = new List<AudioSFXData>();

            AudioSource[] sfx = AudioManager.instance.allSFX;

            foreach (var sound in sfx)
            {
                if (!sound.loop)
                    continue;

                AudioSFXData data = new AudioSFXData();
                data.volume = sound.volume;
                data.pitch = sound.pitch;
                data.fileName = sound.clip.name;

                //index 0 is just formarter (left side) and index 2 is the ending container (right side)
                //but [1] would be the name in between so we could get the resource path from there
                string resoucePath = sound.gameObject.name.Split(AudioManager.SFX_NAME_FORMAT_CONTAINERS)[1];

                data.filePath = resoucePath;
                audioList.Add(data);
            }

            return audioList;
        }

        public static void Apply(List<AudioSFXData> sfx)
        {
            List<string> cache = new List<string>();

            foreach (var sound in sfx)
            {
                if (!AudioManager.instance.isPlayingSoundEffect(sound.fileName))
                    AudioManager.instance.PlaySoundEffect(sound.filePath, volume: sound.volume, pitch: sound.pitch, loop: true);
            }

            foreach (var source in AudioManager.instance.allSFX)
            {
                if (!cache.Contains(source.clip.name))
                    AudioManager.instance.StopSoundEffect(source.clip);
            }
        }
    }
}