using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class RadioTrigger : MonoBehaviour
{
    public AudioSource radioAudioSource; 
    public float lowVolume = 0f; 
    public float highVolume = 0.5f; 
    public float fadeDuration = 1.0f; 
    public float timeInTriggerToFade = 2.0f; 
    public Image blackScreen; 
    public Transform player; 
    public Transform initialPosition; 

    private float timeInTrigger = 0.0f;
    private bool isPlayerInTrigger = false;

    private void Start()
    {
        
        if (radioAudioSource != null)
        {
            radioAudioSource.volume = lowVolume;
        }

        
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger)
        {
            timeInTrigger += Time.deltaTime;

            if (timeInTrigger >= timeInTriggerToFade)
            {
                StartCoroutine(FadeToBlackAndResetPlayer());
                isPlayerInTrigger = false; 
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && radioAudioSource != null)
        {
            StartCoroutine(FadeAudioSource.StartFade(radioAudioSource, fadeDuration, highVolume));
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && radioAudioSource != null)
        {
            StartCoroutine(FadeAudioSource.StartFade(radioAudioSource, fadeDuration, lowVolume));
            isPlayerInTrigger = false;
            timeInTrigger = 0.0f; 
        }
    }

    private IEnumerator FadeToBlackAndResetPlayer()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);

            Color color = blackScreen.color;
            float fadeTime = 1.0f;
            float elapsedTime = 0.0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeTime);
                blackScreen.color = color;
                yield return null;
            }

           
            player.position = initialPosition.position;
            player.rotation = initialPosition.rotation;

           
            yield return new WaitForSeconds(1.0f);

            elapsedTime = 0.0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
                blackScreen.color = color;
                yield return null;
            }

            blackScreen.gameObject.SetActive(false);
        }
    }
}

public static class FadeAudioSource
{
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        yield break;
    }
}
