using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour {

	public DataController dataController;
	public TerrainManager terrainController;
	public LightingScript lightController;
	public WindScript windController;
	public GameObject PlayerAvatar;
	public ParticleScript rainEffect;
	public GameObject InfoText;
	public bool dataReady;
	private float timeElapsedGenerate;

	// Use this for initialization
	void Start () {
		Debug.Log (System.DateTime.Now);
		//dataController.LoadWeatherData ();
	}

	public void GenerateWorld(){
		InfoText.GetComponent<Text>().text = "Generating World";
		StartCoroutine (WaitLoadAndGenerate ());
		dataController.QuickFetchData ();
	}

	IEnumerator WaitLoadAndGenerate(){
		float waitTime = 0f;
		while (!dataController.isLoaded) {
			yield return new WaitForSeconds (0.1f);
			waitTime += 0.1f;
		}
		if (dataController.isSuccess) {
			System.DateTime startTime = System.DateTime.Now;
			GenerateTerrain ();
			InitSkyAndLight ();
			GenerateWind ();
			timeElapsedGenerate = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond) - (startTime.Second * 1000 + startTime.Millisecond);
			Debug.Log ("Time elapsed for generating world: " + timeElapsedGenerate);
			System.DateTime curtime = System.DateTime.Now.AddHours(dataController.GetTimeShift());
			InfoText.GetComponent<Text> ().text = "World finished generated for \n" + dataController.cityName + "\nat " + curtime;
		}
	}

	public void AccessWeatherData(){
		dataController.LoadWeatherData ();
	}

	public void GenerateTerrain(){
		terrainController.GenerateTerrain(dataController.physicalData);
		PlayerAvatar.transform.position = new Vector3 (200f, terrainController.startingAltitude, 200f);
	}

	public void InitSkyAndLight(){
		lightController.timezone = dataController.GetTimeZone();
		lightController.timeshift = dataController.GetTimeShift();
		lightController.GenerateLight (dataController.physicalData);
		//Light.GetComponent<LightingScript> ().RandomizeLight ();
	}

	public void GenerateParticles(){
		rainEffect.GenerateParticleEffect (dataController.physicalData);
	}

	public void GenerateWind(){
		windController.GenerateWind (dataController.physicalData);
	}

	public void ExitApp(){
		Application.Quit ();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
