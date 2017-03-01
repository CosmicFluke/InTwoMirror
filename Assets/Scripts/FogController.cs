using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FogController {

	/**
	 * Fades out the passed in fog object, once all fog particles have dissappeared, the containing object is destroyed.
	 */
	public static void FadeOutFog (GameObject fog)
	{
		ParticleSystem fogSystem = fog.GetComponent<ParticleSystem> ();
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogSystem.particleCount];
		fogSystem.GetParticles (particles);

		fogSystem.Stop ();

		// Destroy the fog until the maximum lifetime of each particle has gone
		MonoBehaviour.Destroy (fog, fogSystem.main.startLifetime.constantMax);
	}
}