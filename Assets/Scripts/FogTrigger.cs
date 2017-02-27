using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTrigger : MonoBehaviour {

	public GameObject fog;
    bool destroyFog = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (fog != null && destroyFog)
        {
			ParticleSystem fogSystem = fog.GetComponent<ParticleSystem>();
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogSystem.particleCount];
			fogSystem.GetParticles (particles);

//			fogSystem.startColor = new Color(fogSystem.startColor.r, fogSystem.startColor.g, fogSystem.startColor.b, fogSystem.startColor.a * 0.9f);
//
//			if (fogSystem.startColor.a < 0.1)
//            {
//                Destroy(fog, 5);
//            }

			fogSystem.Stop ();

			if (fogSystem.particleCount == 0) {
				Destroy (fog);
			}
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        destroyFog = true;
    }
}
