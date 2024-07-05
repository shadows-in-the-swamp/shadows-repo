using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
     public AudioClip[] musicClips; 
    private AudioSource audioSource;
    private int currentClipIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayNextClip();
    }

    void Update()
    {
       
        if (!audioSource.isPlaying)
        {
            PlayNextClip();
        }
    }

    void PlayNextClip()
    {
        Debug.Log("HOLA"); 
        if (musicClips.Length == 0)
            return;

        audioSource.clip = musicClips[currentClipIndex];
        audioSource.Play();

       
        currentClipIndex = (currentClipIndex + 1) % musicClips.Length;
    }
}

