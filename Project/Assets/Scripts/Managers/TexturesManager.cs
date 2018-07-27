using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TextureSlot{
	private int m_textureID;
	public Texture2D m_texture;

	// Constructor
	public TextureSlot (int textureID) {
		m_textureID = textureID;
	}

	// Overload Constructor
	public TextureSlot (int textureID, Texture2D texture) {
		m_textureID = textureID;
		m_texture = texture;
	}

	// Assign a texture to the "new" layer
	public void AssignTexture (Texture2D texture) {
		m_texture = texture;
	}

	// Get this texture slot's id
	public Texture2D GetTexture () {
		return m_texture;
	}

	// Get this texture slot's assigned texture
	public int GetTextureID () {
		return m_textureID;
	}
}


public class TexturesManager : MonoBehaviour {
	//----------------------------------- MANAGER -----------------------------------
	// I am a manager, widget, attribute container
	private Widget m_allLinkedAttributes;


	// Use this for initialization
	void Start () {
		// New attribute list (widget)
		m_allLinkedAttributes = gameObject.AddComponent<Widget>();

		// Add all used attributes, Add 1 value to the textures set by default
		m_allLinkedAttributes.AddAttribute<List<TextureSlot>> ("Textures", new List<TextureSlot>(){ new TextureSlot(0, new Texture2D(64, 64)) });
		m_allLinkedAttributes.AddAttribute<int> ("SelectedTexture", 0);

		ModSelectedLayerSettings ();
	}

	// The "selected" texture has changed
	void SelectedLayer(){
		for(int i = 0; i < m_allLinkedAttributes.GetAttribute <List<TextureSlot>> ("Textures").Count; ++i){ 
			if(m_allLinkedAttributes.GetAttribute <List<TextureSlot>> ("Textures")[i].GetTextureID() == m_allLinkedAttributes.GetAttribute<int>("SelectedTexture")){

				//m_allLinkedAttributes.SetAttribute <LayerFunctionType> ("LayerMode", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerFunctionType);
				//m_allLinkedAttributes.SetAttribute <float> ("LayerOpacity", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerOpacity);
				//m_allLinkedAttributes.SetAttribute <Vector2> ("LayerOffset", m_allLinkedAttributes.GetAttribute <List<Layer>> ("Layers")[i].m_layerOffset);
			}
		}
	}


	public void ModSelectedLayerSettings(){
		List<TextureSlot> m_textures = m_allLinkedAttributes.GetAttribute <List<TextureSlot>> ("Textures");
		int selectedIndex = 0;

		for(int i = 0; i < m_textures.Count; ++i){
			if(m_allLinkedAttributes.GetAttribute <int> ("SelectedTexture") == m_textures[i].GetTextureID()){
				//selectedIndex = i;

				// Settings have been updated, propogate to SELECTED LAYER
				//m_layers[i].m_layerFunctionType = m_allLinkedAttributes.GetAttribute <LayerFunctionType> ("LayerMode");
				//m_layers[i].m_layerOpacity = m_allLinkedAttributes.GetAttribute <float> ("LayerOpacity");
				//m_layers[i].m_layerOffset = m_allLinkedAttributes.GetAttribute <Vector2> ("LayerOffset");
			}
		}
	}

	/*
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
	*/

	/*
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
	*/

	/*
	// -------------------- Manager side layer logic --------------------

	public Layer GetSelectedLayer(){
		List<Texture2D> m_layers = m_allLinkedAttributes.GetAttribute <List<Texture2D>> ("Textures");

		for(int i = 0; i < m_layers.Count; ++i){
			if(m_allLinkedAttributes.GetAttribute <int> ("SelectedLayer") == m_layers[i].m_textureID){
				return m_layers [i];
			}
		}

		return m_layers [0];
	}

	// Update is called once per frame
	void Update () {
	}
	*/
}
