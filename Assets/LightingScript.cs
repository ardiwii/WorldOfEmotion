using UnityEngine;
using System.Collections;

public class LightingScript : MonoBehaviour {

	private Material atmosphere;
	public int timezone;
	public int timeshift;

	// Use this for initialization
	void Start () {
		
	}

	public void RandomizeLight(){
		Color main_color = Random.ColorHSV ();
		//Color fog_color = new Color (main_color.r * (Random.Range (0.05f, 0.6f)), main_color.g * (Random.Range (0.05f, 0.6f)), main_color.b * (Random.Range (0.05f, 0.6f)));
		GetComponent<Light> ().color = main_color;
		GetComponent<Light> ().intensity = Random.Range(0.4f,1.5f);
		//RenderSettings.fogColor = main_color;
		RenderSettings.fogDensity = Random.Range (0.002f, 0.02f);
		RenderSettings.skybox.SetColor ("_SkyTint", main_color);
		RenderSettings.skybox.SetFloat("_AtmosphereThickness", Random.Range(0.3f,2f));
		Debug.Log ("light color randomized");
	}

	/* guide for range:
	 * skybox value (0.2 to 0.9)
	 * skybox exposure (0 to 1.6 (in extreme, pitch black and peak light))
	 * light intensity (0 to 1.5)
	 * light value (0.2 to 0.7 -> 0.7 to 1, shift the saturation towards 0.1 (or less))
	 * fog color value (0.2 to 0.6)
	 * fog saturation (0 to 0.25)
	 * fog density (0 to 0.008 -> may be more than 0.008 if temperature is extremely low)
	 * atmosphere thickness (0.3 to 1.8)
	 */
	public void GenerateLight(PhysicalData physdata){
		float worldBrightness = ConvertTimeToWorldBrightness(physdata);
		Debug.Log ("world brightness level: " + worldBrightness);
		GetComponent<Light> ().color = GenerateLightColor(physdata, worldBrightness);
		GetComponent<Light> ().intensity = worldBrightness * 1.5f;
		GenerateSkybox (physdata, worldBrightness);
		GenerateFog (physdata, worldBrightness);
		Debug.Log ("light color generated");
	}

	public void GenerateSkybox(PhysicalData pd, float wb){
		float hue = ConvertLongitudeToHue (pd.longitude);
		float sat = ConvertTemperatureToSaturation (pd.temperature);
		float val = 0.2f + 0.7f * wb;
		sat *= SaturationWeatherModifer (pd);
		RenderSettings.skybox.SetFloat ("_Exposure", wb * 1.6f);
		RenderSettings.skybox.SetColor ("_SkyTint", Color.HSVToRGB(hue,sat,val));
		float atmThickness = Mathf.Clamp(1f + (((pd.pressure - 910)/150) * 1f),0.7f,2f);
		RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmThickness);
	}

	public Color GenerateLightColor(PhysicalData pd, float wb){
		float hue = ConvertLongitudeToHue (pd.longitude);
		float sat = ConvertTemperatureToSaturation (pd.temperature)*0.5f;
		float val = 0.2f + wb * 0.8f;
		sat = SaturationCut (sat, val);
		sat *= SaturationWeatherModifer (pd);
		Debug.Log (hue + "," + sat + "," + val);
		return Color.HSVToRGB (hue, sat, val);
	}

	public void GenerateFog (PhysicalData pd, float wb){
		float hue = ConvertLongitudeToHue (pd.longitude);
		float sat = ConvertTemperatureToSaturation (pd.temperature)*0.25f;
		float val = 0.2f + wb * 0.4f;
		RenderSettings.fogColor = Color.HSVToRGB(hue, sat, val);
		float density = Mathf.Pow((pd.humidity / 100),2f) * 0.008f;
		RenderSettings.fogDensity = density*FogDensityWeatherModifer(pd);
	}

	public int TimeToMinute(System.DateTime dt, bool withTimezone){
		int time;
		if (timezone == 99 || !withTimezone) {
			dt = dt.ToLocalTime ();
			return dt.Hour * 60 + dt.Minute;
		} else {
			time = (int) ((dt.Hour + (double)timezone)*60 + dt.Minute) ;
			if (time > 1440) {
				time -= 1440;
			} else if (time < 0) {
				time += 1440;
			}
			return time;
		}
	}

	/* all numbers should be used to all parameters that change according to world's light brightness
	 * Time -> World Brightness -> color hue, value, light intensity, skybox exposure, etc
	 * pitch black: ((sunrise + 1day - sunset) / 2) + sunset (0)
	 * nighttime (sunset + 1hour < currenttime < sunrise + 23hours) : abs(pitchblack - currenttime) (0 to 0.2)
	 * dawn (surise-1h to sunrise+1h) : (0.2 to 0.5) || shifts hue to red (to 0 or to 1, whichever nearer) with 1 - abs(sunrise - currenttime)
	 * peak sunlight: ((sunset - sunrise) / 2) + sunrise (1)
	 * daytime (sunrise+1h < currenttime < sunset-1h) : abs(peaksunlight - currenttime) (0.5 to 1)
	 * dusk (sunset-1h to sunset+1h) : (0.5 to 0.2) || hue shifts same as dawn
	 */
	public float ConvertTimeToWorldBrightness(PhysicalData pd){
		float worldBrightness;
		System.DateTime ct = System.DateTime.Now.AddHours(timeshift);
		int sunset = TimeToMinute (pd.sunsetTime, true);
		int sunrise = TimeToMinute (pd.sunriseTime, true);
		int currenttime = TimeToMinute (ct, false); 
		Debug.Log ("sunrise time in minutes" + sunrise);
		Debug.Log ("sunset time in minutes" + sunset);
		Debug.Log ("current time in minutes" + currenttime);
		int peakSunlight = (int)(sunset - sunrise) / 2 + sunrise;
		//nighttime after datechange
		if (currenttime >= 0 && currenttime < sunrise - 60) {
			int nightLength = sunrise - (sunset - 1440);
			int pitchBlack = (nightLength / 2) + (sunset - 1440);
			worldBrightness = ((Mathf.Abs ((float)(pitchBlack - currenttime)) / nightLength) * 0.2f) + 0f;
		}
		//dawn
		else if (currenttime >= sunrise - 60 && currenttime < sunrise + 60) {
			worldBrightness = 0.2f + ((float)(currenttime - (sunrise - 60)) / 120) * 0.3f;
		}
		//daytime
		else if (currenttime >= sunrise + 60 && currenttime < sunset - 60) {
			int dayLength = sunset - sunrise;
			worldBrightness = 1 - ((Mathf.Abs ((float)(peakSunlight - currenttime)) / dayLength) * 0.5f);
		}
		//dusk
		else if (currenttime >= sunset - 60 && currenttime < sunset + 60) {
			worldBrightness = 0.5f - ((float)(currenttime - (sunset - 60)) / 120f) * 0.3f;
		}
		//nighttime before datechange
		else if (currenttime >= sunset + 60 && currenttime < 1440) {
			int nightLength = sunrise + 1440 - sunset;
			int pitchBlack = (nightLength / 2) + sunset;
			Debug.Log ("pitchblack time: " + pitchBlack);
			worldBrightness = ((Mathf.Abs ((float)(pitchBlack - currenttime)) / nightLength) * 0.2f) + 0f;
		} else {
			Debug.Log ("world brightness error");
			worldBrightness = 0f;
		}
		return worldBrightness*WorldBrightnessWeatherModifier(pd);
	}

	public float ConvertLongitudeToHue(float longitude){
		//longitude is -180 to 180 in the same spot: the international date line, and 0 in greenwich
		return (longitude + 180)/360;
	}

	public float ConvertTemperatureToSaturation(float temperature){
		/* 210 - 275 K : 0.2 - 0.3 saturation, 
		 * 275 - 310 K : 0.3 - 0.9 saturation, 
		 * 305 - 340 K : 0.9 - 1.00 saturation */
		if (temperature < 210) {
			return 0.2f;
		} else if (temperature >= 210 && temperature < 275) {
			return (temperature - 210) / 65 * 0.1f + 0.2f;
		} else if (temperature >= 275 && temperature < 310) {
			return (temperature - 275) / 35 * 0.6f + 0.3f;
		} else if (temperature >= 310 && temperature < 340) {
			return (temperature - 305) / 35 * 0.1f + 0.9f;
		} else {
			return 1f;
		}
	}

	public float SaturationCut(float saturation, float value){
		if (value > 0.75f) {
			return saturation - ((value - 0.75f) / 0.25f) * saturation;
		} else {
			return saturation;
		}
	}

	public float WorldBrightnessWeatherModifier(PhysicalData pd){
		return 1 - ((pd.cloudiness / 100) * 0.3f);
//		if (pd.weatherName.Equals ("Clear")) {
//			return 1f;
//		} else if (pd.weatherName.Contains ("Cloud") || pd.weatherName.Equals ("Snow") || pd.weatherName.Equals ("Drizzle")) {
//			return 0.9f;
//		} else if (pd.weatherName.Equals ("Rain")) {
//			return 0.75f;
//		} else if (pd.weatherName.Equals ("Thunderstorm")) {
//			return 0.6f;
//		} else {
//			return 1f;
//		}
	}

	public float SaturationWeatherModifer(PhysicalData pd){
		return 1 - ((pd.cloudiness / 100) * 0.6f);
		//		if (pd.weatherName.Equals ("Clear")) {
//			return 1f;
//		} else if (pd.weatherName.Contains ("Cloud") || pd.weatherName.Equals ("Snow") || pd.weatherName.Equals ("Drizzle")) {
//			return 0.8f;
//		} else if (pd.weatherName.Equals ("Rain")) {
//			return 0.6f;
//		} else if (pd.weatherName.Equals ("Thunderstorm")) {
//			return 0.4f;
//		} else {
//			return 1f;
//		}
	}

	public float FogDensityWeatherModifer(PhysicalData pd){
		if (pd.weatherName.Equals ("Clear")) {
			return 0.8f;
		} else if (pd.weatherName.Contains ("Cloud") || pd.weatherName.Equals ("Snow") || pd.weatherName.Equals ("Drizzle")) {
			return 1f;
		} else if (pd.weatherName.Equals ("Rain") || pd.weatherName.Equals ("Haze")) {
			return 1.5f;
		} else if (pd.weatherName.Equals ("Thunderstorm")) {
			return 1f;
		} else {
			return 1f;
		}
	}
}
