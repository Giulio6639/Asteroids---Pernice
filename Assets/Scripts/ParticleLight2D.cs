using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ParticleLight2D : MonoBehaviour
{

    public ParticleSystem ps;

    Light2D light2D;
    float initialIntensity;
    void Start()
    {
        light2D = GetComponent<Light2D>();
        initialIntensity = light2D.intensity; 
    }

    void Update()
    {
        if (ps.isPlaying) 
        {
            light2D.intensity = initialIntensity * ( (ps.main.duration - ps.totalTime) / ps.main.duration );
        }
    }
}
