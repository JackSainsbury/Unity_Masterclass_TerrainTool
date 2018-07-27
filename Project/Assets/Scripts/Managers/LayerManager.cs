using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// How the heightmap should affect the layer below it
[System.Serializable]
public enum LayerFunctionType { Normal, Subtract, Multiply, Divide, Average, NUMFUNCTYPE }

public class Layer {

	// Id of the layer for history references
	public int m_layerID;

	// The type of logical function this layer will perform (normal(add), subtract, mult, divide, average ... etc)
	public LayerFunctionType m_layerFunctionType;
	// The corresponding layer function script
	public LayerFunction m_layerFuntion;

	// How many coordinate terrain values is the layer offset
	public Vector2 m_layerOffset;

	// The opactity (0->1) of the layer
	public float m_layerOpacity = 1;

	//----------------------------------- Public Members -----------------------------------
	public LayerChunk[,] m_layerChunks;

	// List of active history elements, pre bake to layer array (once bakes, can't be undone).
	public List<MapModifier> m_activeModifierStack;

	private int oddModChunk;
	private int oddMod;

	// Constructor
	public Layer(LayerFunctionType layerFunction, int layerID){
		//Layer = the new id (should be unique)
		m_layerID = layerID;

		ChangeLayerType ();

		ClearLayer ();
	}

	public void ChangeLayerType () {
		// Based on the enum, add the correct script (dangerous - makes sure these are correct)
		switch (m_layerFunctionType) {
		case LayerFunctionType.Normal:
			m_layerFuntion = new NormalLayer ();
			break;
		case LayerFunctionType.Subtract:
			m_layerFuntion = new SubtractLayer ();
			break;
		case LayerFunctionType.Multiply:
			m_layerFuntion = new MultiplyLayer ();
			break;
		case LayerFunctionType.Divide:
			m_layerFuntion = new DivideLayer ();
			break;
		case LayerFunctionType.Average:
			m_layerFuntion = new AverageLayer ();
			break;
		}
	}

	// Blanks the array and modifiers
	public void ClearLayer(){
		if (m_activeModifierStack != null) {
			m_activeModifierStack.Clear ();
		}
		// Chunks to load dimensions
		int cd = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget> ().GetAttribute <int> ("ChunkDimensions");
		int ctld = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget> ().GetAttribute <int> ("ChunksToLoadDimensions");

		// Initialize chunk array
		m_layerChunks = new LayerChunk[ctld, ctld];

		for (int i = 0; i < ctld; ++i) {
			for (int j = 0; j < ctld; ++j) {
				m_layerChunks [i, j] = new LayerChunk (cd);
			}
		}

		oddModChunk = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().oddModChunk;
		oddMod = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().oddMod;
	}

	bool ClampToTerrain(int x, int y){
		bool qualifier = false;
		float halfDim = ((float)GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler>().GetCombinedMapDimensions () / 2);

		if (x >= -halfDim && x <= halfDim && y >= -halfDim && y <= halfDim) {
			qualifier = true;
		}

		return qualifier;
	}

	// World space coordinates will come in, these need to be processed to modify the correct map->chunk->height values
	public List<Vector2> SetHeightFromWorld(int x, int y, float value){
		List<Vector2> ModifiedChunks = new List<Vector2> ();

		if (ClampToTerrain (x, y)) {
			Widget m_mapWidget = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>();

			int cd = m_mapWidget.GetAttribute <int> ("ChunkDimensions");
			int ctld = m_mapWidget.GetAttribute <int> ("ChunksToLoadDimensions");

			int m_modx = (int)(x - (Mathf.Floor ((x + Mathf.Round ((oddModChunk * (cd / 2))) - ((oddMod * oddModChunk) * .5f)) / (cd - 1)) * (cd - 1)) + (oddModChunk * (cd / 2))) - (oddMod * oddModChunk);
			int m_mody = (int)(y - (Mathf.Floor ((y + Mathf.Round ((oddModChunk * (cd / 2))) - ((oddMod * oddModChunk) * .5f)) / (cd - 1)) * (cd - 1)) + (oddModChunk * (cd / 2))) - (oddMod * oddModChunk);

			int m_chunkx = (int)Mathf.Floor (((float)x / (cd - 1)) + ((float)ctld / 2));
			int m_chunky = (int)Mathf.Floor (((float)y / (cd - 1)) + ((float)ctld / 2));

			if (m_chunkx == ctld) { 
				m_chunkx--;
				m_modx = cd - 1;
			}
			if (m_chunky == ctld) {
				m_chunky--;
				m_mody = cd - 1;
			}

			m_layerChunks [m_chunkx, m_chunky].SetHeight (m_modx, m_mody, value);


			// Check if the coordinates need to be stitched to adjacent chunks
			// Just x
			if (m_modx == 0 && m_chunkx != 0) {
				m_layerChunks [m_chunkx - 1, m_chunky].SetHeight (cd - 1, m_mody, value);

				// Add this chunk to the update stack
				ModifiedChunks.Add (new Vector2 (m_chunkx - 1, m_chunky));
			}
			// Just y
			if (m_mody == 0 && m_chunky != 0) {
				m_layerChunks [m_chunkx, m_chunky - 1].SetHeight (m_modx, cd - 1, value);

				// Add this chunk to the update stack
				ModifiedChunks.Add (new Vector2 (m_chunkx, m_chunky - 1));
			}
			// Both (Corner click
			if (m_modx == 0 && m_mody == 0 && m_chunky != 0 && m_chunkx != 0) {

				m_layerChunks [m_chunkx - 1, m_chunky - 1].SetHeight (cd - 1, cd - 1, value);

				// Add this chunk to the update stack
				ModifiedChunks.Add (new Vector2 (m_chunkx - 1, m_chunky - 1));
			}

			// Only ever add internal chunks
			if (m_chunkx < ctld && m_chunky < ctld) {
				// Add this chunk to the update stack
				ModifiedChunks.Add (new Vector2 (m_chunkx, m_chunky));
			}
		}

		return ModifiedChunks;
	}

	// World space coordinates will come in, these need to be processed to return the correct map->chunk->height values
	public float GetHeightFromWorld(int x, int y){
		float output = 0;

		if (ClampToTerrain (x, y)) {
			Widget m_mapWidget = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>();

			int cd = m_mapWidget.GetAttribute <int> ("ChunkDimensions");
			int ctld = m_mapWidget.GetAttribute <int> ("ChunksToLoadDimensions");

			int m_modx = (int)(x - (Mathf.Floor ((x + Mathf.Round ((oddModChunk * (cd / 2))) - ((oddMod * oddModChunk) * .5f)) / (cd - 1)) * (cd - 1)) + (oddModChunk * (cd / 2))) - (oddMod * oddModChunk);
			int m_mody = (int)(y - (Mathf.Floor ((y + Mathf.Round ((oddModChunk * (cd / 2))) - ((oddMod * oddModChunk) * .5f)) / (cd - 1)) * (cd - 1)) + (oddModChunk * (cd / 2))) - (oddMod * oddModChunk);

			int m_chunkx = (int)Mathf.Floor (((float)x / (cd - 1)) + ((float)ctld / 2));
			int m_chunky = (int)Mathf.Floor (((float)y / (cd - 1)) + ((float)ctld / 2));

			if (m_chunkx == ctld) { 
				m_chunkx--;
				m_modx = cd - 1;
			}
			if (m_chunky == ctld) {
				m_chunky--;
				m_mody = cd - 1;
			}
			output = m_layerChunks [m_chunkx, m_chunky].GetHeight (m_modx, m_mody);
		}

		return output;
	}
}

public class LayerManager : MonoBehaviour {
	//----------------------------------- MANAGER -----------------------------------
	// I am a manager, widget, attribute container
	private Widget m_allLinkedAttributes;


	// Use this for initialization
	void Start () {
		// New attribute list (widget)
		m_allLinkedAttributes = gameObject.AddComponent<Widget>();

		// Add all used attributes, Add 1 value to the layers set by default
		m_allLinkedAttributes.AddAttribute<List<Layer>> ("Layers", new List<Layer>(){ new Layer (LayerFunctionType.Normal, 0) });
		m_allLinkedAttributes.AddAttribute<int> ("SelectedLayer", 0);

		m_allLinkedAttributes.AddAttribute<LayerFunctionType> ("LayerMode", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[0].m_layerFunctionType);
		m_allLinkedAttributes.AddAttribute<float> ("LayerOpacity", 1);
		m_allLinkedAttributes.AddAttribute<Vector2> ("LayerOffset", Vector2.zero);

		ModSelectedLayerSettings ();

	}

	// The "selected" layer has changed
	void SelectedLayer(){
		for(int i = 0; i < m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers").Count; ++i){ 
			if(m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerID == m_allLinkedAttributes.GetAttribute<int>("SelectedLayer")){
				m_allLinkedAttributes.SetAttribute <LayerFunctionType> ("LayerMode", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerFunctionType);
				m_allLinkedAttributes.SetAttribute <float> ("LayerOpacity", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerOpacity);
				m_allLinkedAttributes.SetAttribute <Vector2> ("LayerOffset", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerOffset);
			}
		}

		ProjectSelectedLayer ();
	}
		
	public void ModSelectedLayerSettings(){
		List<Layer> m_layers = m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers");
		int selectedIndex = 0;

		for(int i = 0; i < m_layers.Count; ++i){
			if(m_allLinkedAttributes.GetAttribute <int> ("SelectedLayer") == m_layers[i].m_layerID){
				selectedIndex = i;

				// Settings have been updated, propogate to SELECTED LAYER
				m_layers[i].m_layerFunctionType = m_allLinkedAttributes.GetAttribute <LayerFunctionType> ("LayerMode");
				m_layers[i].m_layerOpacity = m_allLinkedAttributes.GetAttribute <float> ("LayerOpacity");
				m_layers[i].m_layerOffset = m_allLinkedAttributes.GetAttribute <Vector2> ("LayerOffset");
			}
		}

		m_layers[selectedIndex].ChangeLayerType ();

		List<Vector2> ModifiedChunks = new List<Vector2>();

		for(int i = 0; i < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++i){
			for(int j = 0; j < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++ j){
				ModifiedChunks.Add (new Vector2(i, j));
			}
		}
			
		GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().PropogateChunks (ModifiedChunks);

		ProjectSelectedLayer ();
	}

	public void MergeAllLayers(){
		List<Vector2> ModifiedChunks = new List<Vector2>();

		for(int i = 0; i < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++i){
			for(int j = 0; j < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++ j){
				ModifiedChunks.Add (new Vector2(i, j));
			}
		}

		Layer finalLayer = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().PropogateChunks (ModifiedChunks);

		m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers") [0] = finalLayer;

		ModSelectedLayerSettings ();
	}

	public void ClearSelectedLayer(){
		GetSelectedLayer ().ClearLayer ();

		List<Vector2> ModifiedChunks = new List<Vector2>();

		for(int i = 0; i < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++i){
			for(int j = 0; j < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++ j){
				ModifiedChunks.Add (new Vector2(i, j));
			}
		}

		GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().PropogateChunks (ModifiedChunks);
	}

	// access the projector and project selection
	public void ProjectSelectedLayer(){
		int texDim = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().GetCombinedMapDimensions ();

		Texture2D newTexture = new Texture2D (texDim, texDim);

		Layer selLayer = GetSelectedLayer ();

		for (int i = 0; i < texDim; ++i) {
			for (int j = 0; j < texDim; ++j) {
				float height = selLayer.GetHeightFromWorld (i - Mathf.RoundToInt(texDim/2), j - Mathf.RoundToInt(texDim/2));


				float r = .5f;
				float g = .5f;
				float b = .5f;

				if (height != 0) {
					float falloff = (Mathf.Abs(height) < 10) ? Mathf.Abs(height)/10 : 1;

					r = (height < 0) ? falloff : 0;
					b = (height > 0) ? falloff : 0;
				}

				newTexture.SetPixel (i, j, new Color(r, g, b));
			}
		}

		newTexture.Apply ();

		GameObject.FindGameObjectWithTag ("LayerProjector").GetComponent<LayerProjector> ().ProjectMapImage (newTexture);
	}

	// -------------------- Manager side layer logic --------------------

	public Layer GetSelectedLayer(){
		List<Layer> m_layers = m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers");

		for(int i = 0; i < m_layers.Count; ++i){
			if(m_allLinkedAttributes.GetAttribute <int> ("SelectedLayer") == m_layers[i].m_layerID){
				return m_layers [i];
			}
		}

		return m_layers [0];
	}

	public void ResizeLayers() {
		foreach (Layer layer in m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")) {
			layer.ClearLayer ();
		}
	}

	// Update is called once per frame
	void Update () {
	}
}
