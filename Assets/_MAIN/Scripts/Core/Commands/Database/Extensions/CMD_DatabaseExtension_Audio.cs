using System;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_Audio : CMD_DatabaseExtension
    {
        //parameters available to function
        //sfx & voice
        private static string[] PARAM_SFX = new string[] { "-s", "-sfx" };
        private static string[] PARAM_VOLUME = new string[] { "-v", "-vol", "-volume" };
        private static string[] PARAM_PITCH = new string[] { "-p", "-pitch" };
        private static string[] PARAM_LOOP = new string[] { "-l", "-loop" };

        //song & ambience
        private static string[] PARAM_CHANNEL = new string[] { "-c", "-channel" };
        //private static string[] PARAM_IMMDEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_START_VOLUME = new string[] { "-sv", "-startvolume" };
        private static string[] PARAM_SONG = new string[] { "-s", "-song" };
        private static string[] PARAM_AMBIENCE = new string[] { "-a", "-ambience" };


        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("playsfx", new Action<string[]>(PlaySFX));
            database.AddCommand("stopsfx", new Action<string>(StopSFX));

            database.AddCommand("playvoice", new Action<string[]>(PlayVoice));
            database.AddCommand("stopvoice", new Action<string>(StopSFX));

            database.AddCommand("playsong", new Action<string[]>(PlaySong));
            database.AddCommand("playambience", new Action<string[]>(PlayAmbience));

            database.AddCommand("stopsong", new Action<string>(StopSong));
            database.AddCommand("stopambience", new Action<string>(StopAmbience));
        }

        private static void PlaySFX(string[] data)
        {
            string filePath;
            float volume, pitch;
            bool loop;

            var parameters = ConvertDataToParameters(data);

            //try to get the name or path to the sound effect
            parameters.TryGetValue(PARAM_SFX, out filePath);

            //try to get the volume of the sound
            parameters.TryGetValue(PARAM_VOLUME, out volume, defaultValue: 1f);

            //try to get the pitch of the sound
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);

            //try to get the sound if it loops
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: false);

            //run the logic
            string resourcesPath = FilePaths.GetPathToResource(FilePaths.resources_sfx, filePath);
            AudioClip sound = Resources.Load<AudioClip>(resourcesPath);

            if (sound == null)
            {
                Debug.LogError($"Could not load sound effect '{filePath}'. Please make sure this exist in the SFX folder!");
                return;
            }

            AudioManager.instance.PlaySoundEffect(sound, volume: volume, pitch: pitch, loop: loop, filePath: resourcesPath);
        }

        private static void PlayVoice(string[] data)
        {
            string filePath;
            float volume, pitch;
            bool loop;

            var parameters = ConvertDataToParameters(data);

            //try to get the name or path to the sound effect
            parameters.TryGetValue(PARAM_SFX, out filePath);

            //try to get the volume of the sound
            parameters.TryGetValue(PARAM_VOLUME, out volume, defaultValue: 1f);

            //try to get the pitch of the sound
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);

            //try to get the sound if it loops
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: false);

            //run the logic
            AudioClip sound = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_voices, filePath));

            if (sound == null)
            {
                Debug.LogError($"Could not load sound voice from '{filePath}'. Please make sure this file exist in the 'Voices' folder!");
                return;
            }

            AudioManager.instance.PlayVoice(sound, volume: volume, pitch: pitch, loop: loop);
        }

        private static void StopSFX(string data)
        {
            AudioManager.instance.StopSoundEffect(data);
        }

        private static void PlaySong(string[] data)
        {
            string filePath;
            int channel;

            var parameters = ConvertDataToParameters(data);

            //try to get the name or path to the song
            parameters.TryGetValue(PARAM_SONG, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_music, filePath);

            //try to get the channel to play the song on
            parameters.TryGetValue(PARAM_CHANNEL, out channel, defaultValue: 1);

            PlayTrack(filePath, channel, parameters);
        }

        private static void PlayAmbience(string[] data)
        {
            string filePath;
            int channel;

            var parameters = ConvertDataToParameters(data);

            //try to get the name or path to the song
            parameters.TryGetValue(PARAM_AMBIENCE, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_ambience, filePath);

            //try to get the channel to play the song on
            parameters.TryGetValue(PARAM_CHANNEL, out channel, defaultValue: 0);

            PlayTrack(filePath, channel, parameters);
        }

        private static void PlayTrack(string filepath, int channel, CommandParameters parameters)
        {
            bool loop;
            float volumeCap;
            float startVolume;
            float pitch;

            //track = song = soundtrack
            //try to get the max volume of the track
            parameters.TryGetValue(PARAM_VOLUME, out volumeCap, defaultValue: 1f);

            //try to get the start volume of the track
            parameters.TryGetValue(PARAM_START_VOLUME, out startVolume, defaultValue: 0f);

            //try to get the pitch of the track
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);

            //try to get if this track loops
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: true);

            //run the logic
            AudioClip sound = Resources.Load<AudioClip>(filepath);

            if (sound == null)
            {
                Debug.Log($"Unable to load Voice from '{filepath}', please ensure the voice file exist or in the right format!");
                return;
            }

            AudioManager.instance.PlayTrack(sound, channel, loop, startVolume, volumeCap, pitch, filepath);
        }

        private static void StopTrack(string data)
        {
            if (int.TryParse(data, out int channel))
                AudioManager.instance.StopTrack(channel);
            else
                AudioManager.instance.StopTrack(data);
        }

        private static void StopSong(string data)
        {
            //if no data, auto fill it with the default track for the song/ambience
            if (data == string.Empty)
                StopTrack("1");
            else
                StopTrack(data);
        }

        private static void StopAmbience(string data)
        {
            if (data == string.Empty)
                StopTrack("0");
            else
                StopTrack(data);
        }


    }
}
