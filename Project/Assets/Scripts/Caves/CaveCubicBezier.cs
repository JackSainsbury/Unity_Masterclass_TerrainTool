using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CaveCubicBezier : MonoBehaviour {
	// The curve id
	public int m_id;

	public GameObject m_gizmo;

	public GameObject m_bezierScaleGizmo;

	// A list of the 4 gizmos if this curve has been selected
	public List<Transform> m_gizmos;


	//Scales of the bezier along its length
	private List <float> m_DivScales;

	// A size t = 0
	private Vector3 a_anchor;
	// A Control node, shaping left hand bezier
	private Vector3 a_control;

	// B size t = 1
	private Vector3 b_anchor;
	// B Control node, shaping right hand bezier
	private Vector3 b_control;

	public void InitializerBezier ( Vector3 a, Vector3 ac, Vector3 bc, Vector3 b, int id) {
		// Left side of bezier
		a_anchor = a;
		a_control = ac;

		// Right side of bezier
		b_control = bc;
		b_anchor = b;

		// Get the curve id
		m_id = id;
	}

	public void DestroyGizmos(){
		m_gizmos.Clear ();
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("GizmoObject")){
			Destroy(go);
		}

		foreach(GameObject go in GameObject.FindGameObjectsWithTag("GizmoRing")){
			Destroy(go.transform.root.gameObject);
		}
	}

	// I have clicked on this curve section of cave within the hierarchy
	public void SelectCurve() {
		CaveManager cmanager = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ();

		cmanager.DeSelectCaveBezier();

		m_gizmos.Add(Instantiate (m_gizmo, a_anchor, Quaternion.identity).transform);
		m_gizmos.Add(Instantiate (m_gizmo, a_control, Quaternion.identity).transform);

		//Make handle (1) a child of anchor (0)
		m_gizmos [0].GetComponent<Gizmo> ().m_childGizmo = m_gizmos [1];

		m_gizmos.Add(Instantiate (m_gizmo, b_control, Quaternion.identity).transform);
		m_gizmos.Add(Instantiate (m_gizmo, b_anchor, Quaternion.identity).transform);

		//Make handle (2) a child of anchor (3)
		m_gizmos [3].GetComponent<Gizmo> ().m_childGizmo = m_gizmos [2];

		int m_lengthRes = cmanager.GetDivRes ();

		// Add the scaling rings
		for (int i = 0; i < m_lengthRes; ++i) {
			//Add a new ring
			m_gizmos.Add (Instantiate (m_bezierScaleGizmo, transform.position, Quaternion.identity).transform);

			// Set the t value for this ring
			m_gizmos [m_gizmos.Count - 1].GetComponent<GizmoRing> ().m_percentageT = ((float)1 / (m_lengthRes - 1)) * i;
			m_gizmos [m_gizmos.Count - 1].GetComponent<GizmoRing> ().m_bezier = this;
		}

		PositionRingGizmos ();

		cmanager.SelectCaveBezier (this);
	}

	// Linear bezier Point
	int getPt ( int n1 , int n2 , float perc ) {
		int diff = n2 - n1;
		return n1 + Mathf.RoundToInt( diff * perc );
	}

	// Get value based on interpolant
	public Vector3 PositionFromInterpolant ( float t ) {
		// The Green Lines
		int xa = getPt( (int)a_anchor.x, (int)a_control.x, t );
		int ya = getPt( (int)a_anchor.y, (int)a_control.y, t );
		int za = getPt( (int)a_anchor.z, (int)a_control.z, t );

		int xb = getPt( (int)a_control.x, (int)b_control.x, t );
		int yb = getPt( (int)a_control.y, (int)b_control.y, t );
		int zb = getPt( (int)a_control.z, (int)b_control.z, t );

		int xc = getPt( (int)b_control.x, (int)b_anchor.x, t );
		int yc = getPt( (int)b_control.y, (int)b_anchor.y, t );
		int zc = getPt( (int)b_control.z, (int)b_anchor.z, t );

		// The Blue Line
		int xm = getPt( (int)xa, (int)xb, t );
		int ym = getPt( (int)ya, (int)yb, t );
		int zm = getPt( (int)za, (int)zb, t );

		int xn = getPt( (int)xb, (int)xc, t );
		int yn = getPt( (int)yb, (int)yc, t );
		int zn = getPt( (int)zb, (int)zc, t );

		// Return the position at interpolant t (0.0f ... 1.0f)
		return new Vector3 (getPt( xm , xn , t ), getPt( ym , yn , t ), getPt( zm , zn , t ));
	}

	public void GenerateMesh(){

		CaveManager cmanager = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ();

		int m_lengthRes = cmanager.GetDivRes();
		int m_ribRes = cmanager.GetRoundRes();

		m_DivScales = new List<float> ();

		for (int i = 0; i < m_lengthRes; ++i) {
			m_DivScales.Add (3);
		}

		// Add the now set mesh to the game object as a meshfilter
		gameObject.AddComponent<MeshFilter> ().mesh = new Mesh ();

		// Add and set up the new mesh renderer
		gameObject.AddComponent<MeshRenderer>().material = cmanager.m_caveMaterial;

		// Add a mesh collider used for raycast testing
		gameObject.AddComponent<MeshCollider>();

		UpdateMesh (m_lengthRes, m_ribRes);

		GenerateTriangles (m_lengthRes, m_ribRes);

		// Add a mesh collider used for raycast testing
		GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter> ().mesh;
	}

	// Structure of mesh has been changed, recreate
	public void ChangeMesh(int m_lengthRes, int m_ribRes){

		if (m_lengthRes > m_DivScales.Count) {
			int countCache = m_DivScales.Count;
			for(int i = 0; i < m_lengthRes - countCache; ++ i){
				m_DivScales.Add (3.25f);
			}
		}

		if (m_gizmos.Count > 0) {
			DestroyGizmos ();
			SelectCurve ();
		}
			
		// Add the now set mesh to the game object as a meshfilter
		gameObject.GetComponent<MeshFilter> ().mesh = new Mesh ();

		UpdateMesh (m_lengthRes, m_ribRes);
		GenerateTriangles (m_lengthRes, m_ribRes);

		// Add a mesh collider used for raycast testing
		GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter> ().mesh;
	}

	private void PositionRingGizmos () {
		CaveManager cmanager = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ();

		int m_lengthRes = cmanager.GetDivRes ();

		// Add the scaling rings
		for (int i = 0; i < m_lengthRes; ++i) {
			// Get the position CURRENTLY along the bezier (0 .. 1) t -> vec3
			Vector3 bezierTPos = PositionFromInterpolant(((float)1 / (m_lengthRes - 1)) * i);

			// Get t (interpolant) for next (0 .. 1+(eot)) (last spill)
			float tNext = ((float)1 / (m_lengthRes - 1)) * (i + 1);

			Vector3 dir = Vector3.zero;
			Quaternion rot = Quaternion.identity;

			// if !last
			if (tNext <= 1) {
				// safe internal
				dir = Vector3.Normalize (PositionFromInterpolant (tNext) - bezierTPos);
				rot = Quaternion.LookRotation (dir);
			} else {
				// collect spilled value
				dir = Vector3.Normalize (bezierTPos - PositionFromInterpolant (((float)1 / (m_lengthRes - 1)) * (i-1)));

				if (dir != Vector3.zero) {
					rot = Quaternion.LookRotation (dir);
				}
			}

			// Rotate to the bezier orientation
			m_gizmos [i + 4].position = bezierTPos;

			//Position along bezier
			m_gizmos [i + 4].rotation = rot;

			if (i < m_DivScales.Count) {
				//Scale to size
				m_gizmos [i + 4].localScale = new Vector3 (m_DivScales[i], m_DivScales[i], m_DivScales[i]);
			} else {
				//Scale to size
				m_gizmos [i + 4].localScale = new Vector3 (3.25f, 3.25f, 3.25f);
			}
		}
	}

	// Call at the end of a mod tick, now array values have been changed
	public void UpdateMesh (int m_lengthRes, int m_ribRes) {

		// The new array of verts we are generating
		Vector3[] vertices = new Vector3[m_lengthRes * m_ribRes];
		// The new array of verts we are generating
		Vector3[] normals = new Vector3[m_lengthRes * m_ribRes];

		if (m_gizmos.Count > 0) {
			m_DivScales.Clear ();
		}

		// Populate verts array
		for (int i = 0; i < m_lengthRes; ++i) {
			if (m_gizmos.Count > 0) {
				m_DivScales.Add (m_gizmos [i + 4].localScale.x);
			}

			for (int j = 0; j < m_ribRes; ++j) {

				Vector3 vertPos = new Vector3 (0, m_DivScales[i], 0); //3 radius, extract for curve scales

				// Rotation
				Quaternion angle = Quaternion.Euler (new Vector3 (0, 0, (360 / m_ribRes) * j));

				// Rotate into the cylinder
				vertPos = angle * vertPos;
				Vector3 normal = angle * Vector3.up;

				// Get the position CURRENTLY along the bezier (0 .. 1) t -> vec3
				Vector3 bezierTPos = PositionFromInterpolant(((float)1 / (m_lengthRes - 1)) * i);

				// Get t (interpolant) for next (0 .. 1+(eot)) (last spill)
				float tNext = ((float)1 / (m_lengthRes - 1)) * (i + 1);

				Vector3 dir = Vector3.zero;
				Quaternion rot = Quaternion.identity;

				// if !last
				if (tNext <= 1) {
					// safe internal
					dir = Vector3.Normalize (PositionFromInterpolant (tNext) - bezierTPos);
					rot = Quaternion.LookRotation (dir);
				} else {
					// collect spilled value
					dir = Vector3.Normalize (bezierTPos - PositionFromInterpolant (((float)1 / (m_lengthRes - 1)) * (i-1)));

					if (dir != Vector3.zero) {
						rot = Quaternion.LookRotation (dir);
					}
				}

				// Rotate to the bezier orientation
				vertPos = rot * vertPos;

				//Position along bezier
				vertPos += bezierTPos;


				vertices [(i * m_ribRes) + j] = vertPos;
				normals  [(i * m_ribRes) + j] = normal;
			}
		}

		Vector2[] uvs = new Vector2[vertices.Length];

		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x / 10, vertices[i].z / 10);
		}

		// Set the verts on our new mesh
		GetComponent<MeshFilter> ().mesh.vertices = vertices;
		// Set the uvs on our new mesh
		GetComponent<MeshFilter> ().mesh.uv = uvs;
		// Set the verts on our new mesh
		GetComponent<MeshFilter> ().mesh.normals = normals;
		// Update the mesh collider at tick time
		GetComponent<MeshCollider> ().sharedMesh = GetComponent<MeshFilter> ().mesh;
		// Recalculate the chunk bounds, for culling
		GetComponent<MeshFilter> ().mesh.RecalculateBounds ();

		if (m_gizmos.Count > 0) {
			PositionRingGizmos ();
		}
	}

	void GenerateTriangles (int m_lengthRes, int m_ribRes) {
		// The new array of verts we are generating
		int[] triangles = new int[(m_lengthRes - 1) * m_ribRes * 6];

		// Populate verts array
		for (int i = 0; i < m_lengthRes; i++) {
			for (int j = 0; j < m_ribRes; j++) {
				if (i > 0) {
					if (j > 0) {
						int count = ((((i - 1) * (m_ribRes - 1)) + (j - 1)) * 6) + ((m_lengthRes - 1) * 6);
						int countRaw = (i * m_ribRes) + j;

						// Tri 1
						triangles [count] = countRaw - 1;
						triangles [count + 1] = countRaw;
						triangles [count + 2] = countRaw - (m_ribRes + 1);

						// Tri 2
						triangles [count + 3] = countRaw;
						triangles [count + 4] = countRaw - m_ribRes;
						triangles [count + 5] = countRaw - (m_ribRes + 1);
					} else {
						int count = ((i - 1) * 6);
						int countRaw = (i * m_ribRes) + j;

						//Debug.Log (countRaw);

						// Tri 1
						triangles [count    ] = countRaw - 1;
						triangles [count + 1] = countRaw + (m_ribRes - 1);
						triangles [count + 2] = countRaw;

						// Tri 2
						triangles [count + 3] = countRaw - m_ribRes;
						triangles [count + 5] = countRaw;
						triangles [count + 4] = countRaw - 1;
					}
				}
			}
		}

		// Set the triangles on our new mesh
		GetComponent<MeshFilter> ().mesh.triangles = triangles;
	}

	//GET

	// Start of bezier curve
	public Vector3 GetStart(){
		return a_anchor;
	}

	// Start handle
	public Vector3 GetStartControl(){
		return a_control;
	}

	// End of bezier curve
	public Vector3 GetEndControl(){
		return b_control;
	}

	// End handle
	public Vector3 GetEnd(){
		return b_anchor;
	}

	//SET

	// Set Start of bezier curve
	public void SetStart(Vector3 aAnchor){
		a_anchor = aAnchor;
	}

	// Set Start handle
	public void SetStartControl(Vector3 aControl){
		a_control = aControl;
	}

	// Set End of bezier curve
	public void SetEndControl(Vector3 bControl){
		b_control = bControl;
	}

	// Set End handle
	public void SetEnd(Vector3 bAnchor){
		b_anchor = bAnchor;
	}
}
