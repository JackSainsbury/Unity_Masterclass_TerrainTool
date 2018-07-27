using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerProjector : MonoBehaviour {

	// My projector component
	private Projector m_projector;

	public Texture2D m_tex;

	// Use this for initialization
	void Start () {
		m_projector = GetComponent<Projector> ();
	}

	public void ProjectMapImage(Texture2D m_projection){
		m_projector.material.SetTexture("_ShadowTex", m_projection);

		m_tex = m_projection;


		m_projector.orthographicSize = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ().GetCombinedMapDimensions () / 2;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
