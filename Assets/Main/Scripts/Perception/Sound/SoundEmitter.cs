using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] protected List<SoundConfiguration> _soundConfigurations;
    [SerializeField] protected Sound _soundPrefab;
    [SerializeField] protected GameObject _producer;

    public GameObject Producer
    {
        get
        {
            return _producer;
        }
    }
    public virtual void Emit(string soundReference, bool child, bool loop, float intensityFactor)
    {
        SoundConfiguration soundConfiguration = GetConfiguration(soundReference);
        if (soundConfiguration.HasClips)
        {
            Sound sound;
            if (child)
            {
                sound = Instantiate(_soundPrefab, transform);
            }
            else
            {
                sound = Instantiate(_soundPrefab,transform.position, _soundPrefab.transform.rotation);
            }
            sound.Initialize(this, soundConfiguration.RandomClip(), loop, soundConfiguration.Intensity * intensityFactor);
        }
    }
    protected virtual SoundConfiguration GetConfiguration(string soundReference)
    {
        foreach (var sound in _soundConfigurations)
        {
            if (sound.Reference == soundReference)
            {
                return sound;
            }
        }
        return new SoundConfiguration();
    }
}

[Serializable]
public struct SoundConfiguration
{
    [SerializeField] string _reference;
    public readonly string Reference
    {
        get
        {
            return _reference;
        }
    }
    [SerializeField] float _intensity;
    public readonly float Intensity
    {
        get
        {
            return _intensity;
        }
    }
    [SerializeField] List<AudioClip> _clips;
    public readonly bool HasClips
    {
        get
        {
            return _clips.Count > 0;
        }
    }

    public readonly AudioClip RandomClip()
    {
        return _clips[UnityEngine.Random.Range(0,_clips.Count)];
    }

}
