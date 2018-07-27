using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Manager for active caves
public class CaveManager : MonoBehaviour {
	//----------------------------------- MANAGER -----------------------------------
	// I am a manager, widget, attribute container
	private Widget m_allLinkedAttributes;


	//----------------------------------- Public Members -----------------------------------
	// Should the cave be represented by a voxel grid?
	public bool m_voxelVizualisation = false;

	// Default cave material
	public Material m_caveMaterial;

	// Create a new cave
	public GameObject m_cavePrefab;

	// Should the cave be offset by half a world space unit
	public bool m_halfOffset = false;


	//----------------------------------- Private Members -----------------------------------
	//List of the active caves
	private List<GameObject> m_caveObjects;

	// The currently (if) selected cave curve
	private CaveCubicBezier m_selectedCaveCurve;

	private MapHandler m_mapHandler;

	// Use this for initialization
	void Start () {
		// New attribute list (widget)
		m_allLinkedAttributes = gameObject.AddComponent<Widget>();

		// Add all used attributes
		m_allLinkedAttributes.AddAttribute <int> ("CaveDivRes", 10); // 0 Rect Brush, 1 Gradient Brush, 2 Circle Brush, 3 Image Brush
		m_allLinkedAttributes.AddAttribute <int> ("CaveRoundRes", 12);   // Gradient Brush falloff

		m_caveObjects = new List<GameObject> ();

		m_mapHandler = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ();
	}


	public Transform[] GetCaveTransforms () {
		Transform[] transforms = new Transform[m_caveObjects.Count];

		for (int i = 0; i < m_caveObjects.Count; ++i) {
			transforms [i] = m_caveObjects[i].transform;
		}

		return transforms;
	}

	public CaveCubicBezier GetSelectedBezier(){
		return m_selectedCaveCurve;
	}

	// Select a cave section
	public void SelectCaveBezier (CaveCubicBezier curveSection){
		m_selectedCaveCurve = curveSection;
	}

	// De select the selected cave bezier
	public void DeSelectCaveBezier (){
		if (m_selectedCaveCurve != null) {
			m_selectedCaveCurve.DestroyGizmos ();
		}

		m_selectedCaveCurve = null;
	}

	// Access the resolutions (cave inits)
	public int GetDivRes () {
		return m_allLinkedAttributes.GetAttribute <int> ("CaveDivRes");
	}
	public int GetRoundRes () {
		return m_allLinkedAttributes.GetAttribute <int> ("CaveRoundRes");
	}

	// From the ui, update the cave resolutions
	public void UpdateResolutions(){
		foreach (GameObject cave in m_caveObjects) {
			foreach (GameObject curve in cave.GetComponent<Cave>().m_curveLists) {
				curve.GetComponent<CaveCubicBezier> ().ChangeMesh (m_allLinkedAttributes.GetAttribute <int> ("CaveDivRes"), m_allLinkedAttributes.GetAttribute <int> ("CaveRoundRes"));
			}
		}
	}

	// From the ui, create a new cave piece
	public void CreateNewCaveSection() {
		// If selection is filled, add to cave hierarchy reate a new cave piece
		if (m_selectedCaveCurve != null) {
			Vector3 lastEnd = m_selectedCaveCurve.GetEnd ();
			Vector3 lastEndControl = m_selectedCaveCurve.GetEndControl ();

			m_selectedCaveCurve.transform.root.GetComponent<Cave> ().NewCurveBranch (lastEnd, lastEnd + (lastEnd - lastEndControl), lastEnd + ((lastEnd - lastEndControl) * 2), lastEnd + (lastEnd - lastEndControl) + new Vector3 (10, 0, 0));

			int lastIndex = m_selectedCaveCurve.transform.root.GetComponent<Cave> ().m_curveLists.Count - 1;

			m_selectedCaveCurve.transform.root.GetComponent<Cave> ().m_curveLists [lastIndex].transform.SetParent (m_selectedCaveCurve.transform);

			m_selectedCaveCurve.transform.root.GetComponent<Cave> ().m_curveLists [lastIndex].GetComponent<CaveCubicBezier> ().SelectCurve ();

		} else {
			// Else new cave hierarchy
			m_caveObjects.Add (Instantiate (m_cavePrefab, Vector3.zero, Quaternion.identity) as GameObject);
		}
	}

	// Update is called once per frame
	void Update () {


		// Checking for changes in the selected curve's gizmos
		if (m_selectedCaveCurve != null) {
			List<CaveCubicBezier> updates = new List<CaveCubicBezier> ();

			for(int i = 0; i < 4; ++ i) {
				if (m_selectedCaveCurve.m_gizmos[i].transform.hasChanged) {
					switch (i) {
					case 0:
						if (m_selectedCaveCurve.transform.parent.GetComponent<CaveCubicBezier> () != null) {
							// Set parent end to new node pos
							m_selectedCaveCurve.transform.parent.GetComponent<CaveCubicBezier> ().SetEnd (m_selectedCaveCurve.m_gizmos [i].position);

							// Add parent to update
							updates.Add (m_selectedCaveCurve.transform.parent.GetComponent<CaveCubicBezier> ());

							// Set all children of parent to node pos (including this node start)
							foreach (Transform child in m_selectedCaveCurve.transform.parent.transform) {
								child.GetComponent<CaveCubicBezier> ().SetStart (m_selectedCaveCurve.m_gizmos [i].position);
								// Add all children to update
								updates.Add (child.GetComponent<CaveCubicBezier> ());
							}
						}
							
						break;
					case 1:
						m_selectedCaveCurve.SetStartControl (m_selectedCaveCurve.m_gizmos[i].position);
						break;
					case 2:
						m_selectedCaveCurve.SetEndControl (m_selectedCaveCurve.m_gizmos[i].position);
						break;
					case 3:
						m_selectedCaveCurve.SetEnd (m_selectedCaveCurve.m_gizmos [i].position);

						foreach (Transform child in m_selectedCaveCurve.transform) {
							child.GetComponent<CaveCubicBezier> ().SetStart (m_selectedCaveCurve.m_gizmos [i].position);
							updates.Add (child.GetComponent<CaveCubicBezier> ());
						}

						break;
					}

					updates.Add (m_selectedCaveCurve);
					m_selectedCaveCurve.m_gizmos[i].transform.hasChanged = false;
				}
			}

			foreach(CaveCubicBezier up in updates){
				up.UpdateMesh (m_allLinkedAttributes.GetAttribute <int> ("CaveDivRes"), m_allLinkedAttributes.GetAttribute <int> ("CaveRoundRes"));
			}
		}


		// Deleting caves with the delete key
		if (Input.GetKeyDown (KeyCode.Delete)) {
			if (m_selectedCaveCurve != null) {
				Cave cave = m_selectedCaveCurve.transform.root.GetComponent<Cave> ().GetComponent<Cave> ();

				if (m_selectedCaveCurve.transform.root != m_selectedCaveCurve.transform.parent) {
					cave.m_curveLists.Remove (m_selectedCaveCurve.gameObject);

					// If that was the last cave in the hierarchy, destroy the cave container
					if (cave.GetComponent<Cave> ().m_curveLists.Count == 0) {
						m_caveObjects.Remove (cave.gameObject);

						Destroy (cave.gameObject);
					}


					// Collect the children
					List<Transform> moves = new List<Transform> ();
					foreach (Transform child in m_selectedCaveCurve.transform) {
						moves.Add (child);
						child.GetComponent<CaveCubicBezier> ().SetStart (m_selectedCaveCurve.transform.parent.GetComponent<CaveCubicBezier> ().GetEnd ());
						child.GetComponent<CaveCubicBezier> ().UpdateMesh (m_allLinkedAttributes.GetAttribute <int> ("CaveDivRes"), m_allLinkedAttributes.GetAttribute <int> ("CaveRoundRes"));
					}

					// Move the children, non destructively
					foreach (Transform move in moves) {
						move.SetParent (m_selectedCaveCurve.transform.parent);
					}

					// Destroy the selected game object
					Destroy (m_selectedCaveCurve.gameObject);
					// Make certain the selected is null
					DeSelectCaveBezier ();
				} else {
					m_caveObjects.Remove (cave.gameObject);
					DeSelectCaveBezier ();
					Destroy (cave.gameObject);
				}
			}
		}

		// Should be offset currently
		if (m_mapHandler.oddMod == 1 && m_mapHandler.oddModChunk == 1) {
			m_halfOffset = true;
		}
	}
}
