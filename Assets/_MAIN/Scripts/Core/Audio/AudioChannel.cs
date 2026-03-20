using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";
    public int channelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;

    public AudioTrack activeTrack { get; private set; } = null;

    private List<AudioTrack> tracks = new List<AudioTrack>();

    bool isVolumeLeveling => co_volumeLeveling != null;
    Coroutine co_volumeLeveling = null;

    public AudioChannel(int channel)
    {
        channelIndex = channel;
        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, string filePath)
    {
        if (TrygetTrack(clip.name, out AudioTrack existingTrack))
        {
            if (!existingTrack.isPlaying)
                existingTrack.Play();

            SetActiveTrack(existingTrack);

            return existingTrack;
        }

        // otherwise a new track is created becomes the active track
        AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, AudioManager.instance.musicMixer, filePath);
        track.Play();

        SetActiveTrack(track);

        return track;
    }

    public bool TrygetTrack(string trackName, out AudioTrack value)
    {
        trackName = trackName.ToLower();

        foreach (var track in tracks)
        {
            if (track.name.ToLower() == trackName)
            {
                value = track;
                return true;
            }
        }

        value = null;
        return false;
    }

    private void SetActiveTrack(AudioTrack track)
    {
        if (!tracks.Contains(track))
            tracks.Add(track);

        activeTrack = track;

        TryStartVolumeLeveling();
    }

    private void TryStartVolumeLeveling()
    {
        if (!isVolumeLeveling)
            co_volumeLeveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
    }

    private IEnumerator VolumeLeveling()
    {

        // This loop continues as long as:
        // - There is an active track and (more than one track exists OR the active track's volume is not at its cap)
        //   OR
        // - There is no active track but there are still tracks in the list.
        while ((activeTrack != null && (tracks.Count > 1 || tracks.Count > 1 || activeTrack.volume != activeTrack.volumeCap)) || (activeTrack == null && tracks.Count > 0))
        {
            // iterate through all tracks and set their volume to 0 or the active track's volumeCap
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                AudioTrack track = tracks[i];
                    
                float targetVol = activeTrack == track ? track.volumeCap : 0;

                if (track == activeTrack && track.volume == targetVol)
                    continue;

                track.volume = Mathf.MoveTowards(track.volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);

                if (track != activeTrack && track.volume == 0)
                {
                    DestroyTack(track);
                }
            }

            yield return null;
        }

        co_volumeLeveling = null;
    }

    private void DestroyTack(AudioTrack track)
    {
        if (tracks.Contains(track))
            tracks.Remove(track);

        Object.Destroy(track.root);
    }

    public void StopTrack(bool immediate = false)
    {
        if (activeTrack == null)
            return;

        if (immediate)
        {
            DestroyTack(activeTrack);
            activeTrack = null;
        }
        else
        {
            activeTrack = null;
            TryStartVolumeLeveling();
        }
    }
}
