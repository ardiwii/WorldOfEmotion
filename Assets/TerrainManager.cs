using UnityEngine;
using System.Collections;
using RobinTheilade.MassTreePlacement;

public class TerrainManager : MonoBehaviour {

	public Texture2D[] baseTextures;
	public Texture2D[] highAltVariantTextures;
	public Texture2D[] flatGrassyTerrain;
	public Texture2D[] cliffTerrain;

	private Texture2D[] selectedTextures;

	public GameObject[] TropicalTrees;
	public TreePrototype[] Tropical_Trees;
	public GameObject[] TemperateTrees;

	public float startingAltitude;
	public float cliffGradient;
	public float meadowGradientMax;
	public float maxTerrainHeight;

	private float climateScore;
	float minHeight;
	float maxHeight;

	public void GenerateTerrain(PhysicalData pd){
		Terrain terrain = GetComponent<Terrain> ();
		float latitude = pd.latitude;
		float longitude = pd.longitude;
//		GetComponent<TerrainToolkit> ().FractalGenerator(0.4f, 1f);

		CalculateClimate (pd);
		GenerateTrees (pd);

		//Generate Terrain Contour
		GetComponent<TerrainToolkit> ().CustomFractalGenerator(0.4f, 1f, CoordinateToSeed(latitude, longitude));
		GetComponent<TerrainToolkit> ().FractalGenerator (0.6f, 0.025f);
		GetComponent<TerrainToolkit> ().SmoothTerrain (1, 1f);

		//Generate Terrain Texture
		minHeight = GetComponent<TerrainToolkit> ().minHeight;
		Debug.Log ("minimum height:" + minHeight);
		maxHeight = GetComponent<TerrainToolkit> ().maxHeight;
		Debug.Log ("maximum height:" + maxHeight);
		float delta = (maxHeight - minHeight)/5f;
		float[] hstops = new float[4];
		for (int i = 0; i < 4; i++) {
			hstops [i] = minHeight + delta * (i+1);
		}
		float[] sstops = new float[2];
		sstops [0] = 15.0f;
		sstops [1] = 35.0f;
		selectedTextures = new Texture2D[4];
		SelectTextures (pd);
		GetComponent<TerrainToolkit> ().CustomAssignTerrainTexture (selectedTextures);
		GenerateTexture();
		startingAltitude = GetComponent<Terrain> ().terrainData.GetHeight (200, 200) + 10;
	}

	public void CalculateClimate(PhysicalData pd){
		climateScore = 0f; //lower score for colder and more humid region and higher for hotter, dryer region
		//temperature ranging 270-320, outside of that will give more than 1 or negative score
		climateScore += 1 - ((pd.temperature - 270) / 50);
		//humidity give tinier effect to the terrain climate
		climateScore += pd.humidity/250 - 0.2f;
		Debug.Log ("climate Score: " + climateScore);
	}

	public void SelectTextures(PhysicalData pd){
		int tx0, tx1, tx2, tx3;
		float score0, score1, score2, score3;
		//selecting texture[0], base texture
		score0 = climateScore * (baseTextures.Length - 2);
		if (score0 < 0.5f) {
			tx0 = 0;
		} else if (score0 > baseTextures.Length - 1.5f) {
			tx0 = baseTextures.Length - 1;
		} else {
			tx0 = Mathf.RoundToInt (score0);
		}
		selectedTextures [0] = baseTextures[tx0];
		//selecting texture[1], alternate
		score1 = climateScore * highAltVariantTextures.Length;
		if (score1 < 0) {
			tx1 = 0;
		} else if (score1 >= highAltVariantTextures.Length) {
			tx1 = highAltVariantTextures.Length - 1;
		} else {
			tx1 = Mathf.FloorToInt (score1);
		}
		selectedTextures [1] = highAltVariantTextures [tx1];
		//selecting texture[2], flat ground/grass, negative=sand, over max = snow
		score2 = climateScore * (flatGrassyTerrain.Length-2);
		if (score2 < 0) {
			tx2 = 0; //snow
		} else if (score2 >= flatGrassyTerrain.Length-2) {
			tx2 = flatGrassyTerrain.Length - 1;
		} else {
			tx2 = Mathf.CeilToInt (score2);
		}
		selectedTextures [2] = flatGrassyTerrain [tx2];
		//selecting texture[3], cliff
		score3 = climateScore * (cliffTerrain.Length - 1);
		if (score3 < 0.5f) {
			tx3 = 0;
		} else if (score3 > cliffTerrain.Length - 1.5f) {
			tx3 = cliffTerrain.Length - 1;
		} else {
			tx3 = Mathf.RoundToInt (score3);
		}
		selectedTextures [3] = cliffTerrain[tx3];
		Debug.Log ("texture score: " + score0 + ", " + score1 + ", " + score2 + ", " + score3);
		Debug.Log ("texture selected: " + tx0 + ", " + tx1 + ", " + tx2 + ", " + tx3);
	}

	public int CoordinateToSeed(float lat, float lon){
		int roundedlon = Mathf.RoundToInt(lon * 1000);
		int roundedlat = Mathf.RoundToInt(lat * 1000);
		return (roundedlon * 100) + roundedlat;
	}

	public void GenerateTrees(PhysicalData pd){
		Terrain terrain = GetComponent<Terrain> ();
		float negTreeScore;
		int treeCount;
		if (climateScore >= 0.5f) {
			negTreeScore = (climateScore - 0.5f)/0.5f;
		} else {
			negTreeScore = (0.5f - climateScore)/0.5f;
		}
		//climate score: max tree count at 0.5, tree count (150-800)
		treeCount = Mathf.Clamp (Mathf.RoundToInt (800 - (negTreeScore * 600)), 0, 800);
		//desert trees
		if (climateScore < 0) {
			int treeVar = 2;
			//worse climate, only dead trees
			if (climateScore < -0.8f) {
				treeVar = 1;
			}
			TreePrototype[] newTreeProts = new TreePrototype[treeVar];
			int j = 0;
			for (int i = 0; i < treeVar; i++) {
				newTreeProts [i] = new TreePrototype ();
				newTreeProts [i].prefab = TropicalTrees [j];
				newTreeProts [i].bendFactor = 1f;
				j++;
			}
			terrain.terrainData.treePrototypes = newTreeProts;
			terrain.terrainData.RefreshPrototypes ();
		} else {
			int treeVar = 4;
			//tropical plants
			if (pd.latitude < 30f && pd.latitude > -30f) {
				TreePrototype[] newTreeProts = new TreePrototype[treeVar];
				float variantScore = climateScore * (TropicalTrees.Length - treeVar);
				int startidx = 0;
				if (variantScore < 0) {
					startidx = 0;
				} else if (variantScore >= TropicalTrees.Length - treeVar) {
					startidx = TropicalTrees.Length - treeVar;
				} else {
					startidx = Mathf.RoundToInt (variantScore);
				}
				Debug.Log ("Start index: " + startidx);
				int j = startidx;
				for (int i = 0; i < treeVar; i++) {
					newTreeProts [i] = new TreePrototype ();
					newTreeProts [i].prefab = TropicalTrees [j];
					newTreeProts [i].bendFactor = 1f;
					j++;
				}
				terrain.terrainData.treePrototypes = newTreeProts;
				terrain.terrainData.RefreshPrototypes ();
			} 
			//temperate
			else {
				TreePrototype[] newTreeProts = new TreePrototype[treeVar];
				float variantScore = climateScore * TemperateTrees.Length - treeVar;
				int startidx = 0;
				if (variantScore < 0) {
					startidx = 0;
				} else if (variantScore >= TemperateTrees.Length - treeVar) {
					startidx = TemperateTrees.Length - treeVar;
				} else {
					startidx = Mathf.RoundToInt (variantScore);
				}
				Debug.Log ("Start index: " + startidx);
				int j = startidx;
				for (int i = 0; i < treeVar; i++) {
					newTreeProts [i] = new TreePrototype ();
					newTreeProts [i].prefab = TemperateTrees [j];
					newTreeProts [i].bendFactor = 1f;
					j++;
				}
				terrain.terrainData.treePrototypes = newTreeProts;
				terrain.terrainData.RefreshPrototypes ();
			}
		}
		GetComponent<MassTreePlacement> ().count = treeCount;
		GetComponent<MassTreePlacement>().GenerateTrees (terrain);
	}

	public void GenerateTexture(){
		// Get the attached terrain component
		Terrain terrain = GetComponent<Terrain>();

		// Get a reference to the terrain data
		TerrainData terrainData = terrain.terrainData;

		float hmax = maxHeight * maxTerrainHeight;
		float hmin = minHeight * maxTerrainHeight;

		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
		float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{
				// Normalise x/y coordinates to range 0-1 
				float y_01 = (float)y/(float)terrainData.alphamapHeight;
				float x_01 = (float)x/(float)terrainData.alphamapWidth;

				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
				float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );

				// Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
				Vector3 normal = terrainData.GetInterpolatedNormal(y_01,x_01);

				// Calculate the steepness of the terrain
				float steepness = terrainData.GetSteepness(y_01,x_01);

				// Setup an array to record the mix of texture weights at this point
				float[] splatWeights = new float[terrainData.alphamapLayers];
				float[] textureProbability = new float[splatWeights.Length];

				// CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

				// Texture[1] is the rocky/dirt variation, stronger in higher altitude
				splatWeights[1] = (height-hmin)/(hmax-hmin);

				// Texture[0] is the dirt/grass variation, spread evenly
				splatWeights [0] = 1f - splatWeights[1];

				// Texture[2] is the full grass texture, stronger on flatter terrain
				if (steepness < meadowGradientMax) {
					splatWeights [2] = Mathf.Clamp((meadowGradientMax*1.5f - steepness) / meadowGradientMax,0.7f,2f);
				} else {
					splatWeights [2] = 0f;
				}

				// Texture[3] is the cliff texture, stronger and prioritized in steeper terrain 
				if (steepness > cliffGradient) {
					splatWeights [3] = steepness / cliffGradient;
//					if (steepness > cliffGradient * 1.5f) {
//						splatWeights [0] *= 0.5f;
//						splatWeights [1] *= 0.5f;
//					}
				} else {
					splatWeights [3] = 0f;
				}

				// Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
				float z = 0;
				for(int i=0;i<splatWeights.Length;i++){
					z += splatWeights[i];
				}
					
				// Loop through each terrain texture
				for(int i = 0; i<terrainData.alphamapLayers; i++){

					// Normalize so that sum of all texture weights = 1
					splatWeights[i] /= z;

					// Assign this point to the splatmap array
					splatmapData[x, y, i] = splatWeights[i];
				}
			}
		}

		// Finally assign the new splatmap to the terrainData:
		terrainData.SetAlphamaps(0, 0, splatmapData);
	}

	float calculateStrength(float probability){
		float diceRoll = Random.Range (0.0f, 1.0f);
		if (diceRoll > 1 - probability) {
			return (Mathf.Clamp01 (0.2f + (diceRoll - (1 - probability))/probability));
		} else {
			return 0f;
		}
	}
}