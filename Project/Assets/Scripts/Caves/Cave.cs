using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cave consisting of a minimum 1 bezier curve
public class Cave : MonoBehaviour {

	// List of cave curves
	public List<GameObject> m_curveLists;

	// Cave bezier with default settings
	public GameObject m_bezierPrefab;


	// Constructor
	public void Start () {
		// Initialize a new bezier list
		m_curveLists = new List<GameObject> ();

		// Add 1 curve by default
		NewCurveBranch (new Vector3(-20, -10, 0), new Vector3(-20, 10, 0), new Vector3(20, 10, 0), new Vector3(20, -10, 0));

		// Position the cave to the grid
		SetCaveOffset ();

		m_curveLists [0].GetComponent<CaveCubicBezier> ().SelectCurve ();
	}

	// Create a new bezier controller curve for the voxel cave interpretation
	public void NewCurveBranch (Vector3 a, Vector3 ac, Vector3 bc, Vector3 b) {
		
		// Add the new curve to the cave curve list
		GameObject newCurveObject = Instantiate (m_bezierPrefab, transform.position, Quaternion.identity);

		int id = m_curveLists.Count;

		// Name it
		newCurveObject.name = "CaveCurve " + id.ToString();

		//Parent it to cave at the very least
		newCurveObject.transform.SetParent (transform);

		//Init to defaults
		newCurveObject.GetComponent<CaveCubicBezier> ().InitializerBezier (a, ac, bc, b, id);

		// Create the cave
		newCurveObject.GetComponent<CaveCubicBezier>().GenerateMesh ();

		// Add new curve to the list
		m_curveLists.Add (newCurveObject);
	}

	// Set the position of the cave to match the grid
	public void SetCaveOffset () {
		// Determine if the cave is currently offset
		bool isCurrentlyHalfOffset = (transform.position.x - Mathf.Floor (transform.position.x) == 0) ? false : true;
		// Determine if the cave should be offset
		bool shouldBeCurrentlyOffset = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ().m_halfOffset;

		// Process accordingly
		if (shouldBeCurrentlyOffset && !isCurrentlyHalfOffset) {
			// Add a half grid space to the whole cave system
			transform.position += new Vector3(0.5f, 0, 0.5f);
		} else if (isCurrentlyHalfOffset) {
			// subtract a half grid space from the whole cave system
			transform.position -= new Vector3(0.5f, 0, 0.5f);
		}
	}
}