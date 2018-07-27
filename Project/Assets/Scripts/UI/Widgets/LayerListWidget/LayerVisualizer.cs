using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerVisualizer : MonoBehaviour {

	public LayerListWidget m_myListWidget;

	public int m_layerID;

	public Text m_layerText;

	// This visualizer was clicked, selection code
	public void SelectLayer(){
		m_myListWidget.SelectLayer (m_layerID);
	}

	// Use this for initialization
	void Start () {
		m_layerText.text = "Layer " + m_layerID.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
