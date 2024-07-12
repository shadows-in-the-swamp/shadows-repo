using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    public Light[] lights; 
    public float minIntensity = 0.1f; 
    public float maxIntensity = 1.0f; 
    public float flickerSpeed = 0.1f; 

    private bool isFlickering = false; 

    void Start()
    {
       
        StartCoroutine(FlickerLights());
    }

    private IEnumerator FlickerLights()
    {
        isFlickering = true;
        while (isFlickering)
        {
            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.intensity = Random.Range(minIntensity, maxIntensity);
                }
            }
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    
    public void StopFlickering()
    {
        isFlickering = false;
    }

    
    public void StartFlickering()
    {
        if (!isFlickering)
        {
            StartCoroutine(FlickerLights());
        }
    }
}
