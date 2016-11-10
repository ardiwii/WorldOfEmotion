using UnityEngine;
using System.Collections;
//using System.Net.NetworkInformation;
using SimpleJSON;
using UnityEngine.UI;

public class DataController : MonoBehaviour {

	//IP-API
	private string IP_APIRequest = "http://ip-api.com/json";

	//OpenWeatherMap
	private string OWMKey = "dcc6a9b6ce906b1ce9318b40be976358";
	private string OWMURL = "api.openweathermap.org/data/2.5/weather?";

	//weather parameters
	public PhysicalData physicalData;
//	public float latitude{get;private set;} // -90 to 90
//	public float longitude{get;private set;} //-180 to 180
//	public string weatherName{get;private set;} //Clear, Thunderstorm, Rain, Snow, Cloud
//	public float temperature{get;private set;} // 200 to 350
//	public float pressure{get;private set;} //
//	public float humidity{get;private set;} //0 to 100
//	public float rainIntensity{get;private set;} // 0 to 8
//	public float windSpeed{get;private set;} //0 to 5
//	public float windDegree{get;private set;}
//
	//Google Geolocation
	private string APIKey = "AIzaSyC9E4VFoGUit_noWJkxMOehF1Bz2o579yI";
	private string geolocationAddress = "https://www.googleapis.com/geolocation/v1/geolocate?key=";
	private string macAddress = "";

	public GameObject InfoText;
	public GameObject customLatText;
	public GameObject customLonText;
	public GameObject CustomFields;
	public string cityName;
	public bool isLoaded;
	public bool isSuccess;
	private bool readyToLoadAPI;
	private string realWeatherName;
	private float timeElapsedIP;
	private float timeElapsedWeather;

	// Use this for initialization
	void Start () {
		physicalData = new PhysicalData ();
		//InitMacAddress ();
		//Debug.Log (Network.player.ipAddress);
		readyToLoadAPI = true;
		isLoaded = false;
		GetIPLocation ();
		//LoadWeatherData ();
	}

	public void LoadWeatherData(){
		if (readyToLoadAPI) {
			readyToLoadAPI = false;
			FetchData ();
		}
	}

	public void GetIPLocation(){
		if (readyToLoadAPI) {
			timeElapsedIP = 0f;
			readyToLoadAPI = false;
			StartCoroutine (LoadIPLocation ());
		}
	}


	IEnumerator LoadIPLocation(){
		//byte[] postData = 
		System.DateTime startTime = System.DateTime.Now;
		InfoText.GetComponent<Text> ().text = "Fetching coordinate from IP";
		WWW ip_apiRequest = new WWW (IP_APIRequest);
		yield return ip_apiRequest;
		if (ip_apiRequest.error != null) {
			Debug.Log (ip_apiRequest.error);
			InfoText.GetComponent<Text> ().text = "Failed to get IP data, check your internet connection";
		} else {
			Debug.Log ("reply message: " + ip_apiRequest.text);
			JSONNode arrReply = JSON.Parse (ip_apiRequest.text);
			string ip = arrReply ["query"].Value;
			physicalData.latitude = arrReply ["lat"].AsFloat;
			physicalData.longitude = arrReply ["lon"].AsFloat;
			CustomFields.transform.FindChild ("Latitude").GetComponent<InputField> ().text = physicalData.latitude.ToString ();
			CustomFields.transform.FindChild ("Longitude").GetComponent<InputField> ().text = physicalData.longitude.ToString ();
			InfoText.GetComponent<Text>().text = "Coordinate loaded from ip, ready to generate world";
			CustomFields.transform.FindChild ("Timezone").GetComponent<InputField> ().text = "";
			timeElapsedIP = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond) - (startTime.Second * 1000 + startTime.Millisecond);
			Debug.Log ("Time elapsed for loading ip location: " + timeElapsedIP);
		}
		isLoaded = false;
		readyToLoadAPI = true;
	}

	public void CoordinateChange(){
		isLoaded = false;
	}

	public void FetchData(){
		if (customLatText.GetComponent<Text> ().text.Length == 0 || customLonText.GetComponent<Text> ().text.Length == 0) {
			InfoText.GetComponent<Text> ().text = "enter latitude and longitude for custom coordinate!";
		} else {
			float custlat = float.Parse (CustomFields.transform.FindChild ("Latitude").FindChild ("Text").GetComponent<Text> ().text);
			float custlon = float.Parse (CustomFields.transform.FindChild ("Longitude").FindChild ("Text").GetComponent<Text> ().text);
			StartCoroutine (InitWeatherDataCustom (custlat, custlon));
		}
	}

	public void QuickFetchData(){
		if (!isLoaded) {
			FetchData ();
		} else {
		}
	}

	public void UseRandomCoordinate(){
		float randomlat = Random.Range (-65f, 75f);
		float randomlon = Random.Range (-180f, 180f);
		CustomFields.transform.FindChild ("Latitude").GetComponent<InputField>().text = randomlat.ToString();
		CustomFields.transform.FindChild ("Longitude").GetComponent<InputField>().text = randomlon.ToString();
		physicalData.latitude = randomlat;
		physicalData.longitude = randomlon;
		CustomFields.transform.FindChild ("Timezone").GetComponent<InputField> ().text = Mathf.FloorToInt (randomlon / 15f).ToString();
		isLoaded = false;
		InfoText.GetComponent<Text>().text = "Coordinate randomized, ready to generate world";
	}

	public void LoadToInputFields(){
		CustomFields.transform.FindChild ("Temperature").GetComponent<InputField>().text = physicalData.temperature.ToString ();
		CustomFields.transform.FindChild ("Humidity").GetComponent<InputField>().text = physicalData.humidity.ToString ();
		CustomFields.transform.FindChild ("Pressure").GetComponent<InputField>().text = physicalData.pressure.ToString ();
		CustomFields.transform.FindChild ("WindSpeed").GetComponent<InputField>().text = physicalData.windSpeed.ToString ();
		CustomFields.transform.FindChild ("Cloudiness").GetComponent<InputField>().text = physicalData.cloudiness.ToString ();
		if (physicalData.weatherName.Equals ("Rain")) {
			CustomFields.transform.FindChild ("Rain").GetComponent<Toggle> ().isOn = true;
			CustomFields.transform.FindChild ("RainIntensity").GetComponent<InputField> ().text = physicalData.rainIntensity.ToString ();
		} else {
			CustomFields.transform.FindChild ("Rain").GetComponent<Toggle> ().isOn = false;
		}
	}

	public void SaveInput(){
		float custlat = float.Parse (CustomFields.transform.FindChild ("Latitude").FindChild ("Text").GetComponent<Text> ().text);
		float custlon = float.Parse (CustomFields.transform.FindChild ("Longitude").FindChild ("Text").GetComponent<Text> ().text);
		float custtemp = float.Parse (CustomFields.transform.FindChild ("Temperature").FindChild ("Text").GetComponent<Text> ().text);
		float custhumid = float.Parse (CustomFields.transform.FindChild ("Humidity").FindChild ("Text").GetComponent<Text> ().text);
		float custpres = float.Parse (CustomFields.transform.FindChild ("Pressure").FindChild ("Text").GetComponent<Text> ().text);
		float custspeed = float.Parse (CustomFields.transform.FindChild ("WindSpeed").FindChild ("Text").GetComponent<Text> ().text);
		float custcloud = float.Parse (CustomFields.transform.FindChild ("Cloudiness").FindChild ("Text").GetComponent<Text> ().text);
		bool isRaining = CustomFields.transform.FindChild ("Rain").GetComponent<Toggle> ().isOn;
		float custintensity = 0f;
		if (isRaining) {
			custintensity = float.Parse (CustomFields.transform.FindChild ("RainIntensity").FindChild ("Text").GetComponent<Text> ().text);
		} else {
			if (!realWeatherName.Contains ("Rain")) {
				physicalData.weatherName = realWeatherName;
			} else {
				physicalData.weatherName = "Clear";
				realWeatherName = "Clear";
			}
		}
		if (custlat >= -85f && custlon <= 85f) {
			physicalData.latitude = custlat;
		} else {
		}
		if (custlon >= -180f && custlon <= 180f) {
			physicalData.longitude = custlon;
		} else {
		}
		if (custtemp >= 200f && custlon <= 350f) {
			physicalData.temperature = custtemp;
		} else {
		} 
		if (custhumid >= 0f && custhumid <= 100f) {
			physicalData.humidity = custhumid;
		} else {
		}
		if (custpres >= 400f && custpres <= 1600f) {
			physicalData.pressure = custpres;
		} else {
		}
		if (custspeed >= 0f && custspeed <= 20f) {
			physicalData.windSpeed = custspeed;
		} else {
		}
		if (custcloud >= 0f && custcloud <= 100f) {
			physicalData.cloudiness = custcloud;
		}
		if (isRaining && custintensity > 0f && custintensity <= 8f) {
			physicalData.weatherName = "Rain";
			physicalData.rainIntensity = custintensity;
		} else {
		}
		InfoText.GetComponent<Text> ().text = "Custom data saved";
		isLoaded = true;
		isSuccess = true;
	}

	public int GetTimeZone(){
		if (!CustomFields.transform.FindChild ("Timezone").FindChild ("Text").GetComponent<Text> ().text.Equals ("")) {
			return int.Parse (CustomFields.transform.FindChild ("Timezone").FindChild ("Text").GetComponent<Text> ().text);
		} else {
			return 99;
		}
	}

	public int GetTimeShift(){
		if (!CustomFields.transform.FindChild ("Timeshift").FindChild ("Text").GetComponent<Text> ().text.Equals ("")) {
			return int.Parse (CustomFields.transform.FindChild ("Timeshift").FindChild ("Text").GetComponent<Text> ().text);
		} else {
			return 0;
		}
	}

	IEnumerator InitWeatherDataCustom(float lat, float lon) {
		System.DateTime startTime = System.DateTime.Now;
		InfoText.GetComponent<Text> ().text = "Fetching weather data, please wait...";
		isSuccess = false;
		physicalData.latitude = lat;
		physicalData.longitude = lon;
		Debug.Log ("latitude: " + physicalData.latitude);
		Debug.Log ("longitude: " + physicalData.longitude);
		string owmFullURL = OWMURL + "APPID=" + OWMKey + "&lat=" + physicalData.latitude + "&lon=" + physicalData.longitude;
		WWW owmRequest = new WWW (owmFullURL);
		yield return owmRequest;
		if (owmRequest.error != null) {
			Debug.Log (owmRequest.error);
			InfoText.GetComponent<Text> ().text = "Failed to get weather data, check your internet connection or service is unavailable";
		} else {
			Debug.Log ("reply message: " + owmRequest.text);
			JSONNode owmReply = JSON.Parse (owmRequest.text);
			physicalData.temperature = owmReply ["main"] ["temp"].AsFloat;
			physicalData.pressure = owmReply ["main"] ["pressure"].AsFloat;
			physicalData.humidity = owmReply ["main"] ["humidity"].AsFloat;
			physicalData.windSpeed = owmReply ["wind"] ["speed"].AsFloat;
			physicalData.windDegree = owmReply ["wind"] ["deg"].AsFloat;
			physicalData.weatherName = owmReply ["weather"] [0] ["main"].Value;
			realWeatherName = physicalData.weatherName;
			physicalData.rainIntensity = owmReply ["rain"] ["3h"].AsFloat;
			physicalData.cloudiness = owmReply ["clouds"] ["all"].AsFloat;
			System.DateTime dtDateTime = new System.DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			physicalData.sunsetTime = dtDateTime.AddSeconds (owmReply ["sys"] ["sunset"].AsDouble);
			physicalData.sunriseTime = dtDateTime.AddSeconds (owmReply ["sys"] ["sunrise"].AsDouble);
			Debug.Log ("sunset time: " + physicalData.sunsetTime);
			cityName = owmReply ["name"].Value;
			InfoText.GetComponent<Text> ().text = "Weather data succesfully fetched for \n" + physicalData.latitude + "," + physicalData.longitude + "\n" + cityName;
			LoadToInputFields ();
			isSuccess = true;
			timeElapsedWeather = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond) - (startTime.Second * 1000 + startTime.Millisecond);
			Debug.Log ("Time elapsed for loading weather data: " + timeElapsedWeather);
		}
		isLoaded = true;
		readyToLoadAPI = true;
	}

	// Update is called once per frame
	void Update () {

	}

	//END

	IEnumerator InitWeatherData() {
		WWW ip_apiRequest = new WWW (IP_APIRequest);
		yield return ip_apiRequest;
		if (ip_apiRequest.error != null) {
			Debug.Log (ip_apiRequest.error);
		}
		Debug.Log("reply message: " + ip_apiRequest.text);
		JSONNode arrReply = JSON.Parse (ip_apiRequest.text);
		string ip = arrReply ["query"].Value;
		physicalData.latitude = arrReply ["lat"].AsFloat;
		physicalData.longitude = arrReply ["lon"].AsFloat;
		Debug.Log ("latitude: " + physicalData.latitude);
		Debug.Log ("longitude: " + physicalData.longitude);
		string owmFullURL = OWMURL + "APPID=" + OWMKey + "&lat=" + physicalData.latitude + "&lon=" + physicalData.longitude;
		WWW owmRequest = new WWW (owmFullURL);
		yield return owmRequest;
		if (owmRequest.error != null) {
			Debug.Log (owmRequest.error);
		}
		Debug.Log("reply message: " + owmRequest.text);
		JSONNode owmReply = JSON.Parse (owmRequest.text);
		physicalData.temperature = owmReply ["main"] ["temp"].AsFloat;
		physicalData.pressure = owmReply ["main"] ["pressure"].AsFloat;
		physicalData.humidity = owmReply ["main"] ["humidity"].AsFloat;
		physicalData.windSpeed = owmReply ["wind"] ["speed"].AsFloat;
		physicalData.windDegree = owmReply ["wind"] ["deg"].AsFloat;
		physicalData.weatherName = owmReply ["weather"][0]["main"].Value;
		physicalData.rainIntensity = owmReply ["rain"] ["3h"].AsFloat;
		physicalData.cloudiness = owmReply ["clouds"] ["all"].AsFloat;
		string cityName = owmReply ["name"].Value;
		InfoText.GetComponent<Text> ().text = "your IP: " + ip + "\n" + physicalData.latitude + "," + physicalData.longitude + "\n" + cityName ;
		Debug.Log ("weather data fetched");
		readyToLoadAPI = true;
	}
		
	IEnumerator LoadIPLocationGoogle(){
		string ourPostData = "{}";
		System.Collections.Generic.Dictionary<string,string> headers = new System.Collections.Generic.Dictionary<string,string>();
		headers.Add("Content-Type", "application/json");
		//headers.Add("Cookie", "Our session cookie");
		byte[] postData = System.Text.Encoding.ASCII.GetBytes (ourPostData);
		WWW googleGeoAPI = new WWW (geolocationAddress + APIKey, postData);
		yield return googleGeoAPI;
		if (googleGeoAPI.error != null) {
			Debug.Log (googleGeoAPI);
		}
		Debug.Log("reply message: " + googleGeoAPI.text);
		JSONNode arrReply = JSON.Parse (googleGeoAPI.text);
		//string ip = arrReply ["query"].Value
		physicalData.latitude = arrReply ["location"]["lat"].AsFloat;
		physicalData.longitude = arrReply ["location"]["lon"].AsFloat;
		CustomFields.transform.FindChild ("Latitude").GetComponent<InputField>().text = physicalData.latitude.ToString ();
		CustomFields.transform.FindChild ("Longitude").GetComponent<InputField>().text = physicalData.longitude.ToString ();
		readyToLoadAPI = true;
	}

//	//Get MAC address for google geolocation api
//	public void InitMacAddress()
//	{
//		IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
//		NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
//		Debug.Log("Interface information for " + 
//			computerProperties.HostName +"."+ computerProperties.DomainName);
//		foreach (NetworkInterface adapter in nics)
//		{
//			IPInterfaceProperties properties = adapter.GetIPProperties();
//			Debug.Log(adapter.Description);
//			Debug.Log(string.Empty.PadLeft(adapter.Description.Length,'='));
//			Debug.Log(" Interface type .......................... : "+ adapter.NetworkInterfaceType);
//			Debug.Log(" Physical Address ........................ : "+ 
//				adapter.GetPhysicalAddress().ToString());
//			Debug.Log(" Is receive only.......................... : "+ adapter.IsReceiveOnly);
//			Debug.Log(" Multicast................................ : "+ adapter.SupportsMulticast);
//		}
//	}
}
