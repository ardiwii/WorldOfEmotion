using UnityEngine;
using System.Collections;

public class PhysicalData {

	//weather parameters
	public float latitude; // -90 to 90
	public float longitude; //-180 to 180
	public string weatherName; //Clear, Thunderstorm, Rain, Snow, Cloud, Drizzle
	public float temperature; // 200 to 350
	public float pressure; //in mbar (hPa) averages 1013.25, ranging from 930 to 1060
	public float humidity; //0 to 100
	public float rainIntensity; // 0 to 8
	public float windSpeed; //0 to 5
	public float windDegree;
	public float cloudiness;
	public System.DateTime sunsetTime;
	public System.DateTime sunriseTime;

//	public System.DateTime getSunset(){
//		//System.DateTime.
//	}
}
