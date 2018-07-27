using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePerlin : MonoBehaviour {

	private MapHandler m_mapHandlerScript;

	public List<Vector2> PaintPerlinNoise(int x, int y, float weight){
		m_mapHandlerScript = GameObject.FindGameObjectWithTag("MapHandler").GetComponent<MapHandler>();

		Vector2 offset = GetComponent<Widget>().GetAttribute <Vector2> ("NoiseOffset");
		float amplitude = GetComponent<Widget>().GetAttribute <float> ("NoiseScale");


		LayerManager layerManager = GameObject.FindGameObjectWithTag ("LayerManager").GetComponent<LayerManager> ();
	
		float p1 = Mathf.PerlinNoise (((float)(x + 1) / 5) + (offset.x / 10), ((float)(y + 1) / 5) + (offset.y / 10)) * ((3 * amplitude) * weight);
		float p2 = Mathf.PerlinNoise (((float)(x + 1) / 20) + (offset.x / 10), ((float)(y + 1) / 20) + (offset.y / 10)) * ((15 * amplitude) * weight);

		return layerManager.GetSelectedLayer().SetHeightFromWorld (x, y, p1 + p2);
	}

	// Whole layer
	public void GeneratePerlinNoise(){
		m_mapHandlerScript = GameObject.FindGameObjectWithTag("MapHandler").GetComponent<MapHandler>();

		Vector2 offset = GetComponent<Widget>().GetAttribute <Vector2> ("NoiseOffset");
		float amplitude = GetComponent<Widget>().GetAttribute <float> ("NoiseScale");

		int mapDimension = m_mapHandlerScript.GetCombinedMapDimensions ();
		int halfMap = Mathf.RoundToInt (mapDimension);

		LayerManager layerManager = GameObject.FindGameObjectWithTag ("LayerManager").GetComponent<LayerManager> ();

		for (int i = 0; i <= mapDimension; ++i) {
			for (int j = 0; j <= mapDimension; ++j) {
				float p1 = Mathf.PerlinNoise (((float)(i + 1) / 5) + (offset.x / 10), ((float)(j + 1) / 5) + (offset.y / 10)) * (3 * amplitude);
				float p2 = Mathf.PerlinNoise (((float)(i + 1) / 20) + (offset.x / 10), ((float)(j + 1) / 20) + (offset.y / 10)) * (15 * amplitude);

				layerManager.GetSelectedLayer().SetHeightFromWorld (i - (halfMap/2), j - (halfMap/2), layerManager.GetSelectedLayer().GetHeightFromWorld (i - (halfMap/2), j - (halfMap/2)) + p1 + p2);
			}
		}

		// In the current mouse step, which chunks were modified?
		List<Vector2> ModifiedChunks = new List<Vector2> ();

		for (int i = 0; i < m_mapHandlerScript.GetComponent<Widget> ().GetAttribute <int> ("ChunksToLoadDimensions"); ++i) {
			for (int j = 0; j < m_mapHandlerScript.GetComponent<Widget> ().GetAttribute <int> ("ChunksToLoadDimensions"); ++j) {
				ModifiedChunks.Add(new Vector2(i, j));
			}
		}

		m_mapHandlerScript.PropogateChunks (ModifiedChunks);
	}

	// Update is called once per frame
	void Update () {
	}
}
