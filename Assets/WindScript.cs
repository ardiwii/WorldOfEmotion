using UnityEngine;
using System.Collections;

public class WindScript : MonoBehaviour {

	/* on a scale of 0 m/s to 15 m/s in real life
	 * main: 0 - 2
	 * turbulence: 0 - 2.5
	 * pulse magnitude: 0 - 1
	 * pulse frequency: 0 - 0.2
	 */ 

	public void GenerateWind(PhysicalData pd){
		float overallWindSpeed = pd.windSpeed / 15f;
		float windDirection = pd.windDegree;
		transform.rotation = Quaternion.Euler (new Vector3 (0f, windDirection, 0f));
		GetComponent<WindZone> ().windMain = overallWindSpeed * 2f;
		GetComponent<WindZone> ().windTurbulence = overallWindSpeed * 3f;
		GetComponent<WindZone> ().windPulseMagnitude = 0.5f + (overallWindSpeed * 1.0f);
		GetComponent<WindZone> ().windPulseFrequency = overallWindSpeed * 0.2f;
	}
}
