using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTrigger : MonoBehaviour {

    public ParticleSystem fog;
    bool destroyFog = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (fog != null && destroyFog)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fog.particleCount];
            fog.GetParticles(particles);

            fog.startColor = new Color(fog.startColor.r, fog.startColor.g, fog.startColor.b, fog.startColor.a * 0.9f);

            if (fog.startColor.a < 0.1)
            {
                Destroy(fog, 5);
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        destroyFog = true;
    }
}
