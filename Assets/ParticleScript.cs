using UnityEngine;
using System.Collections;

public class ParticleScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/* 0 to 8 intensity
	 * 1000/500 to 10000/5000
	 */
	public void GenerateParticleEffect(PhysicalData pd){
		if (!pd.weatherName.Contains ("Rain")) {
			GetComponent<ParticleSystem> ().Stop ();
		} else {
			int maxParticle = (int)((pd.rainIntensity / 8) * 9000 + 1000);
			int emissionRate = (int)((pd.rainIntensity / 8) * 4000 + 1000);
			GetComponent<ParticleSystem> ().maxParticles = maxParticle;
			ParticleSystem.EmissionModule emis = GetComponent<ParticleSystem> ().emission;
			emis.rate = new ParticleSystem.MinMaxCurve (emissionRate);
		}
	}
}
