using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RadioTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioMixer audioMixer;
    public string snapshotNormal = "Normal";
    public string snapshotRadio = "Radio";
    public float transitionTime = 0.5f; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioMixer.FindSnapshot(snapshotRadio).TransitionTo(transitionTime);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioMixer.FindSnapshot(snapshotNormal).TransitionTo(transitionTime);
        }
    }
}
