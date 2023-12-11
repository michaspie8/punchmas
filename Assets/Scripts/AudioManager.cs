using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
    [Header("Typing")]
   public  AudioSource typingAudioSource;


    [Header("Music")]
    //Songs
    public AudioClip[] songs;
    public AudioSource musicSource;
    public string currentSongName;


    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
        musicSource = this.gameObject.GetComponent<AudioSource>();
        typingAudioSource = this.gameObject.AddComponent<AudioSource>();
        
    }

    public static AudioManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {

    }


    public void PlayMusic(string songName, bool loop)
    {
        AudioClip song = null;
        foreach (AudioClip clip in songs)
        {
            if (clip.name == songName)
            {
                song = clip;
            }
        }
        if (song != null)
        {
            //stopping waiting to prevent form unwanted behaviour
            StopCoroutine("WaitForSongToEnd");
            //stop the current song, and play the new one
            musicSource.Stop();
            musicSource.clip = song;
            musicSource.Play();
            
            musicSource.loop = loop;
            
            currentSongName = songName;
            
        }
        else
        {
            Debug.LogWarning("Failed to find song with name: " + songName);
        }
    } 
    public void PlayMusic(string songName, bool loop, float delay)
    {
        AudioClip song = null;
        foreach (AudioClip clip in songs)
        {
            if (clip.name == songName)
            {
                song = clip;
            }
        }
        if (song != null)
        {
            //stopping waiting to prevent form unwanted behaviour
            StopCoroutine("WaitForSongToEnd");
            //stop the current song, and play the new one
            musicSource.Stop();
            musicSource.clip = song;
            musicSource.PlayDelayed(delay);
            
            musicSource.loop = loop;
            
            currentSongName = songName;
            
        }
        else
        {
            Debug.LogWarning("Failed to find song with name: " + songName);
        }
    }

    public void PlayAfter(string songName, bool loop)
    {
        //Wait for the current song to end
        StartCoroutine(WaitForSongToEnd());

        IEnumerator WaitForSongToEnd()
        {
            yield return new WaitUntil(() => musicSource.isPlaying == false);
            
            PlayMusic(songName, loop);
        }
    }
    




    public void PlayTypingSound(int currentDisplayedCharacterCount, char currentCharacter, TypingAudioInfoSO currentAudioInfo, bool makePredictable)
    {
        // set variables for the below based on our config
        AudioClip[] dialogueTypingSoundClips = currentAudioInfo.typingSoundClips;
        int frequencyLevel = currentAudioInfo.frequencyLevel;
        float minPitch = currentAudioInfo.minPitch;
        float maxPitch = currentAudioInfo.maxPitch;
        bool stopAudioSource = currentAudioInfo.stopAudioSource;

        // play the sound based on the config
        if (currentDisplayedCharacterCount % frequencyLevel == 0)
        {
            if (stopAudioSource)
            {
                typingAudioSource.Stop();
            }
            AudioClip soundClip = null;
            // create predictable audio from hashing
            if (makePredictable)
            {
                int hashCode = currentCharacter.GetHashCode();
                // sound clip
                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;
                soundClip = dialogueTypingSoundClips[predictableIndex];
                // pitch
                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchRangeInt = maxPitchInt - minPitchInt;
                // cannot divide by 0, so if there is no range then skip the selection
                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minPitchInt;
                    float predictablePitch = predictablePitchInt / 100f;
                    typingAudioSource.pitch = predictablePitch;
                }
                else
                {
                    typingAudioSource.pitch = minPitch;
                }
            }
            // otherwise, randomize the audio
            else
            {
                // sound clip
                int randomIndex = Random.Range(0, dialogueTypingSoundClips.Length);
                soundClip = dialogueTypingSoundClips[randomIndex];
                // pitch
                typingAudioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            // play sound
            typingAudioSource.PlayOneShot(soundClip);
        }
    }
}