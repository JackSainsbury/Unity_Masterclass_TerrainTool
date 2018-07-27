using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerListWidget : Widget {
	// The foreign and local name of the attribute
	public string m_layerListAttributeName;

	// Prefab ui list element
	public GameObject m_visualizeListEntry;

	// The panel to hold the visualizers
	public GameObject m_contentArea;

	// Pixel buffer in the menu visualizer, between layer entries
	public float m_betweenLayersBuffer = 3.0f;

	public Color m_selectedLayerVizColour;
	public Color m_deselectedLayerVizColour;

	// List of prefab ui elements
	private List<GameObject> m_visualizeListEntries;

	// Panel transform which will hold the entry visualizers
	private RectTransform m_rectTransform;

	private Widget m_managerWidget;

	public void Start () {
		m_rectTransform = GetComponent<RectTransform> ();

		m_visualizeListEntries = new List<GameObject>();

		m_managerWidget = GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ();

		// Declare and initialize a new attribute
		AddAttribute <List<Layer>> (m_layerListAttributeName, m_managerWidget.GetAttribute<List<Layer>> (m_layerListAttributeName));
		AddAttribute <int> ("SelectedLayer", m_managerWidget.GetAttribute<int> ("SelectedLayer"));

		// Add all the initial layers to the list
		foreach (Layer layer in GetAttribute<List<Layer>> (m_layerListAttributeName)) {
			AddVisualizer (layer.m_layerID);
		}

		m_visualizeListEntries [GetAttribute <int> ("SelectedLayer")].GetComponent<Image> ().color = m_selectedLayerVizColour;
	}

	// Select a visualizer, thereform a layer
	public void SelectLayer(int layerID){
		// Set the local selected
		SetAttribute<int> ("SelectedLayer", layerID);
		// Set the foreign selected
		m_managerWidget.SetAttribute <int> ("SelectedLayer", layerID);

		for (int i = 0; i < m_visualizeListEntries.Count; ++i) {
			if (m_visualizeListEntries [i].GetComponent<LayerVisualizer>().m_layerID == layerID) {
				m_visualizeListEntries [i].GetComponent<Image> ().color = m_selectedLayerVizColour;
			} else {
				m_visualizeListEntries [i].GetComponent<Image> ().color = m_deselectedLayerVizColour;
			}
		}

		AllMethodMessages ();
	}

	// From button, add layer
	public void AddLayer () {
		List<Layer> m_layers = GetAttribute<List<Layer>> (m_layerListAttributeName);

		// Adds a new layer, with a new id
		GetAttribute<List<Layer>> (m_layerListAttributeName).Add(new Layer(LayerFunctionType.Normal, m_layers.Count));

		m_layers [m_layers.Count - 1].m_layerID = m_layers [m_layers.Count - 2].m_layerID + 1;
		AddVisualizer (m_layers [m_layers.Count - 1].m_layerID);
	}

	// Add a new visualizer to the list
	void AddVisualizer (int layerID) {
		// Instance a new layer visualizer
		GameObject newEntry = Instantiate (m_visualizeListEntry, transform.position, Quaternion.identity) as GameObject;

		// Scale appropriately
		newEntry.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_contentArea.GetComponent<RectTransform>().rect.width - m_betweenLayersBuffer, 20);

		// Set parent to the content panel
		newEntry.transform.SetParent (m_contentArea.transform, true);

		// Reference this script on the visualizer for layer selection clicks
		newEntry.GetComponent<LayerVisualizer> ().m_myListWidget = this;

		// Set the id to be the same as this layer
		newEntry.GetComponent<LayerVisualizer> ().m_layerID = layerID;

		// Add to the visualizers stack
		m_visualizeListEntries.Add(newEntry);

		// Position visualizers
		PositionVisualizers ();
	}

	// For all the visualizers in the active list
	void PositionVisualizers () {
		for(int i = 0; i < m_visualizeListEntries.Count; ++i){
			// Position
			m_visualizeListEntries[i].GetComponent<RectTransform> ().position = m_rectTransform.position + new Vector3 (
				-m_betweenLayersBuffer * 1.5f, 
				((i * m_visualizeListEntries[i].GetComponent<RectTransform> ().rect.height) + (i * m_betweenLayersBuffer) - ((GetComponent<RectTransform> ().rect.height / 2) - (m_visualizeListEntries[i].GetComponent<RectTransform> ().rect.height / 2) - m_betweenLayersBuffer)), 
				0);
		}
	}

	public void ClearLayer(){
		GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<LayerManager> ().ClearSelectedLayer ();
	}

	public void MergeLayers(){
		GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<LayerManager> ().MergeAllLayers ();

		int layerCount = GetAttribute <List<Layer>> (m_layerListAttributeName).Count;

		int[] ids = new int[layerCount];

		for(int i = 0; i < layerCount; ++i) {
			ids [i] = GetAttribute <List<Layer>> (m_layerListAttributeName) [i].m_layerID;
		}

		for(int i = 0; i < ids.Length; ++i) {
			SelectLayer(ids[i]);
			RemoveLayer ();
		}
	}

	// From button, remove layer
	public void RemoveLayer () {
		if (GetAttribute <int> ("SelectedLayer") != 0) {

			int deleteIndex = 1;
			bool topStack = false;

			for (int i = 0; i < GetAttribute<List<Layer>> (m_layerListAttributeName).Count; ++i) {
				if (GetAttribute<List<Layer>> (m_layerListAttributeName) [i].m_layerID == GetAttribute <int> ("SelectedLayer")) {
					deleteIndex = i;
					if(deleteIndex == GetAttribute<List<Layer>> (m_layerListAttributeName).Count -1){
						topStack = true;
					}
				}
			}

			Destroy (m_visualizeListEntries [deleteIndex]);
			m_visualizeListEntries.RemoveAt (deleteIndex);
			GetAttribute<List<Layer>> (m_layerListAttributeName).RemoveAt (deleteIndex);

			SelectLayer (GetAttribute<List<Layer>> (m_layerListAttributeName)[(topStack) ? deleteIndex - 1 : deleteIndex].m_layerID);

			PositionVisualizers();

			List<Vector2> ModifiedChunks = new List<Vector2>();

			for(int i = 0; i < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++i){
				for(int j = 0; j < GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<Widget>().GetAttribute<int>("ChunksToLoadDimensions"); ++ j){
					ModifiedChunks.Add (new Vector2(i, j));
				}
			}

			GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().PropogateChunks (ModifiedChunks);
		}
	}
}
