using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;


public class ToolManager : MonoBehaviour {
	//----------------------------------- MANAGER -----------------------------------
	// I am a manager, widget, attribute container
	private Widget m_allLinkedAttributes;

	//----------------------------------- Public Members -----------------------------------

	// The base weight of change the brush will mod
	public float m_brushWeight = 1.0f;

	// The visualizer for the brush projection
	public GameObject m_brushProjector;

	// Texture to load and paint with
	public Texture2D m_loadedTexture;

	//----------------------------------- Private Members -----------------------------------

	// Cache of map handler
	private MapHandler m_mapHandlerScript;

	// Cache the last 
	private Vector3 m_lastMousePos;

	// Instance of the brush light on the terrain
	private GameObject m_mouseLightInstance;

	// The minimum vector magnitude step for a brush stroke to activate
	private float m_brushMinStep = 5;

	// The currently loaded brush array (alpha)
	private float[,] m_brushArray;

	private int m_AdditivePower = 1;

	// The area in which I can draw
	private Rect m_activeArea { get; set; }

	// is the mouse currently over a terrain piece (raycast)
	private bool m_mouseOverTerrain;

	private string _path;

	private bool m_isSmoothing = false;

	private bool m_noiseDraw = false;

	private bool m_isAllowedToDraw = true;

	Vector2 lastBrushDimensions;

	//----------------------------------- Methods -----------------------------------
	// Use this for initialization
	void Start () {

		// New attribute list (widget)
		m_allLinkedAttributes = gameObject.AddComponent<Widget>();

		// Add all used attributes
		m_allLinkedAttributes.AddAttribute <int> ("ActiveTool", 0); // 0 Rect Brush, 1 Gradient Brush, 2 Circle Brush, 3 Image Brush
		m_allLinkedAttributes.AddAttribute <Gradient> ("FalloffGradient", new Gradient());   // Gradient Brush falloff
		m_allLinkedAttributes.AddAttribute <Gradient> ("FalloffCircle", new Gradient()); 	// Circle falloff
		m_allLinkedAttributes.AddAttribute <Gradient> ("FalloffSmoothing", new Gradient());  // Smoothing falloff
		m_allLinkedAttributes.AddAttribute <int> ("Radius", 15); 				// Radius of the circle brush 
		m_allLinkedAttributes.AddAttribute <float> ("BrushWeight", 1.0f);
		m_allLinkedAttributes.AddAttribute <Vector2> ("BrushArrayDimensions", new Vector2(1,1));
		m_allLinkedAttributes.AddAttribute <Vector2> ("BrushAnchor", new Vector2 (0, 0));

		// init the gradient attributes
		m_allLinkedAttributes.GetAttribute <Gradient> ("FalloffGradient").SetKeys (new GradientColorKey[]{new GradientColorKey(Color.white,0)} ,new GradientAlphaKey[]{new GradientAlphaKey(1,0) , new GradientAlphaKey(0,1)});
		m_allLinkedAttributes.GetAttribute <Gradient> ("FalloffCircle").SetKeys (new GradientColorKey[]{new GradientColorKey(Color.white,0)} ,new GradientAlphaKey[]{new GradientAlphaKey(1,0) , new GradientAlphaKey(0,1)});
		m_allLinkedAttributes.GetAttribute <Gradient> ("FalloffSmoothing").SetKeys (new GradientColorKey[]{new GradientColorKey(Color.white,0)} ,new GradientAlphaKey[]{new GradientAlphaKey(1,0) , new GradientAlphaKey(0,1)});

		m_allLinkedAttributes.AddAttribute <float> ("NoiseScale", 1.0f);
		m_allLinkedAttributes.AddAttribute <Vector2> ("NoiseOffset", Vector2.zero);

		// Cache the map handler script
		m_mapHandlerScript = GameObject.FindGameObjectWithTag ("MapHandler").GetComponent<MapHandler> ();

		m_activeArea = new Rect (0, 0, Screen.width, Screen.height);

		SetBrushArraySquare ();
	}

	public void SetAllowedToDraw(bool allowed){
		m_isAllowedToDraw = allowed;
	}

	void SetBrushProjector(){
		Texture2D m_projection = new Texture2D((int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x + 2, (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y + 2);

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				float alpha = 0;

				alpha = m_brushArray [i, j];

				m_projection.SetPixel(i + 1, j + 1, new Color(alpha, alpha, alpha));
			}
		}

		for (int i = 0; i < m_projection.width; ++i) {
			m_projection.SetPixel (i, 0, Color.black);
			m_projection.SetPixel (i, m_projection.height - 1, Color.black);
		}

		for (int j = 0; j < m_projection.height; ++j) {
			m_projection.SetPixel (0, j, Color.black);
			m_projection.SetPixel (m_projection.width - 1, j, Color.black);
		}

		m_projection.wrapMode = TextureWrapMode.Clamp;

		m_projection.Apply ();

		m_brushProjector.GetComponent<Projector>().material.SetTexture("_ShadowTex", m_projection);
		m_brushProjector.GetComponent<Projector>().orthographicSize = ((int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x / 2 >= 1) ? (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x / 2 : 1;
	}

	// Apply an output brush array to the map (1 by 1)
	public void SetBrushArraySquare(){
		// generate a new brush array
		m_brushArray = new float[(int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x, (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y];

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				m_brushArray [i, j] = .5f;
			}
		}

		SetBrushProjector ();
	}

	// Apply an output brush array to the map (1 by 1)
	public void SetBrushArrayCircle(){
		int radius = m_allLinkedAttributes.GetAttribute <int> ("Radius");

		int oddMod = (radius % 2 == 0) ? 0 : 1;

		Gradient grad = m_allLinkedAttributes.GetAttribute <Gradient> ("FalloffGradient");

		Debug.Log (grad.alphaKeys.Length);

		// generate a new brush array
		m_brushArray = new float[(radius* 2) + oddMod, (radius * 2)  + oddMod];

		m_allLinkedAttributes.SetAttribute <Vector2> ("BrushArrayDimensions", new Vector2((radius * 2)  + oddMod, (radius * 2)  + oddMod));

		int px, nx, py, ny, d;

		for (int x = 0; x <= radius; x++)
		{
			d = (int)Mathf.Ceil(Mathf.Sqrt(radius * radius - x * x));
			for (int y = 0; y <= d; y++)
			{
				float weightAlpha = grad.Evaluate ((1 / (float)radius) * x).a * grad.Evaluate ((1 / (float)radius) * y).a;

				px = radius + x;
				nx = radius - x;
				py = radius + y;
				ny = radius - y;

				m_brushArray [px, py] = weightAlpha;
				m_brushArray [nx, py] = weightAlpha;
				m_brushArray [px, ny] = weightAlpha;
				m_brushArray [nx, ny] = weightAlpha;
			}
		}

		SetBrushProjector ();
	}


	// Apply an output brush array to the map (1 by 1)
	public void SetBrushArrayTexture(){

		m_allLinkedAttributes.SetAttribute <Vector2> ("BrushArrayDimensions", new Vector2(m_loadedTexture.width, m_loadedTexture.height));

		// generate a new brush array
		m_brushArray = new float[m_loadedTexture.width, m_loadedTexture.height];

		Color[] pixels = m_loadedTexture.GetPixels();

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				m_brushArray [i, j] = pixels[(i * (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x) + j].r;
			}
		}

		SetBrushProjector ();
	}

	public void ToggleNoiseDraw () {
		m_noiseDraw = !m_noiseDraw;
	}

	// Update is called once per frame
	void Update () {
		if (GameObject.FindGameObjectWithTag ("IMGVIS") != null) {
			GameObject.FindGameObjectWithTag ("IMGVIS").GetComponent<RawImage>().texture = m_loadedTexture;
		}

		if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.LeftCommand)) {
			m_AdditivePower = -1;
		} else {
			m_AdditivePower = 1;
		}

		if (Input.GetKey (KeyCode.LeftShift)) {
			m_isSmoothing = true;
		} else {
			m_isSmoothing = false;
		}
			
		// Hit out from raycast
		RaycastHit hit;

		if (m_isAllowedToDraw) {
			// Cast the mouse position to world hit
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			// Ray hit a collider
			if (Physics.Raycast (ray, out hit)) {

				// Make sure cursor is actually hovering a valid terrain piece
				if (hit.collider.tag == "Terrain") {
					Vector2 projectAnchorOff = new Vector2 (
						                          Mathf.Floor (m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x / 2) * (m_allLinkedAttributes.GetAttribute<Vector2> ("BrushAnchor").x),
						                          Mathf.Floor (m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y / 2) * (m_allLinkedAttributes.GetAttribute<Vector2> ("BrushAnchor").y)
					                          );

					m_mouseOverTerrain = true;
					m_brushProjector.transform.position = hit.point + new Vector3 (0 - (int)projectAnchorOff.x, 500, 0 - (int)projectAnchorOff.y);

					// Am I sculpting
					if (Input.GetMouseButton (0)) {

						// Mouse is clicked and has moved more than the threshold
						if ((Input.mousePosition - m_lastMousePos).magnitude > m_brushMinStep) {
						
							// Check if the cursor is within the active draw area
							if (Input.mousePosition.x > m_activeArea.x && Input.mousePosition.x < m_activeArea.x + m_activeArea.width && Input.mousePosition.y > m_activeArea.y && Input.mousePosition.y < m_activeArea.y + m_activeArea.height) {

								Vector2 anchorOff = new Vector2 (
									                   Mathf.Floor (m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x / 2) * (m_allLinkedAttributes.GetAttribute<Vector2> ("BrushAnchor").x + 1),
									                   Mathf.Floor (m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y / 2) * (m_allLinkedAttributes.GetAttribute<Vector2> ("BrushAnchor").y + 1)
								                   );

								// In the current mouse step, which chunks were modified?
								List<Vector2> ModifiedChunks = new List<Vector2> ();

								if (m_isSmoothing) {
									ModifiedChunks = SmoothBrush (anchorOff, hit.point.x, hit.point.z);
								} else if (m_noiseDraw) {
									ModifiedChunks = DrawNoise (anchorOff, hit.point.x, hit.point.z);
								} else {
									ModifiedChunks = DrawBrush (anchorOff, hit.point.x, hit.point.z);
								}


								m_mapHandlerScript.PropogateChunks (ModifiedChunks);
							}
						}
					}
				} else {
					m_mouseOverTerrain = false;
				}

				if (Input.GetMouseButtonDown (0)) {
					if (hit.collider.tag == "CaveCurveSection") {
						Debug.Log ("CLICKED CAVE SECTION");
						hit.transform.GetComponent<CaveCubicBezier> ().SelectCurve ();
					} else {
						if (hit.collider.tag != "GizmoHandle" && hit.collider.tag != "GizmoRing") {
							GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ().DeSelectCaveBezier ();
						}
					}
				}
			}
				
			if (m_mouseOverTerrain) {
				m_brushProjector.SetActive (true);
			} else {
				m_brushProjector.SetActive (false);
			}
		}

		if (Input.GetKeyDown (KeyCode.W)) {
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("GizmoObject")) {
				go.GetComponent<Gizmo> ().SetHandleProcess (ProcessType.Translate);
			}
		}else if (Input.GetKeyDown (KeyCode.E)) {
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("GizmoObject")) {
				go.GetComponent<Gizmo> ().SetHandleProcess (ProcessType.Rotate);
			}
		}



		//Cache the last position, so as to only update if we've moved the mouse a certain "step" instead of every tick
		m_lastMousePos = Input.mousePosition;
	}

	List<Vector2> DrawBrush(Vector2 anchorOff, float x, float z){
		LayerManager layerManager = GameObject.FindGameObjectWithTag ("LayerManager").GetComponent<LayerManager> ();
		List<Vector2> ModifiedChunks = new List<Vector2>();

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				if (m_brushArray [i, j] != 0) { //Reject if brush has no influence
					foreach (Vector2 chunkCoord in layerManager.GetSelectedLayer().SetHeightFromWorld (Mathf.RoundToInt (x) + i - (int)anchorOff.x, Mathf.RoundToInt (z) + j - (int)anchorOff.y, layerManager.GetSelectedLayer().GetHeightFromWorld (Mathf.RoundToInt (x) + i - (int)anchorOff.x, Mathf.RoundToInt (z) + j - (int)anchorOff.y) + (m_brushArray[i,j] * m_allLinkedAttributes.GetAttribute<float>("BrushWeight") * m_AdditivePower))) {
						if (!ModifiedChunks.Contains (chunkCoord)) {
							ModifiedChunks.Add (chunkCoord);
						}
					}
				}
			}
		}

		return ModifiedChunks;
	}

	List<Vector2> SmoothBrush(Vector2 anchorOff, float x, float z){
		LayerManager layerManager = GameObject.FindGameObjectWithTag ("LayerManager").GetComponent<LayerManager> ();
		List<Vector2> ModifiedChunks = new List<Vector2>();

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				if (m_brushArray [i, j] != 0) { //Reject if brush has no influence
					float weight = 0;

					for (int sX = 0; sX < 3; ++sX) {
						for (int sY = 0; sY < 3; ++sY) {
							weight += layerManager.GetSelectedLayer().GetHeightFromWorld (Mathf.RoundToInt (x) + i - (int)anchorOff.x + (sX - 1), Mathf.RoundToInt (z) + j - (int)anchorOff.y + (sY - 1));
						}
					}
						
					foreach (Vector2 chunkCoord in layerManager.GetSelectedLayer().SetHeightFromWorld (Mathf.RoundToInt (x) + i - (int)anchorOff.x, Mathf.RoundToInt (z) + j - (int)anchorOff.y, (weight/9))) {
						if (!ModifiedChunks.Contains (chunkCoord)) {
							ModifiedChunks.Add (chunkCoord);
						}
					}
				}
			}
		}

		return ModifiedChunks;
	}

	List<Vector2> DrawNoise(Vector2 anchorOff, float x, float z){
		List<Vector2> ModifiedChunks = new List<Vector2>();

		GeneratePerlin perScript = GetComponent<GeneratePerlin> ();

		for (int i = 0; i < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").x; ++i) {
			for (int j = 0; j < (int)m_allLinkedAttributes.GetAttribute <Vector2> ("BrushArrayDimensions").y; ++j) {
				if (m_brushArray [i, j] != 0) { //Reject if brush has no influence
					foreach (Vector2 chunkCoord in perScript.PaintPerlinNoise(Mathf.RoundToInt (x) + i - (int)anchorOff.x, Mathf.RoundToInt (z) + j - (int)anchorOff.y, (m_brushArray[i,j] * m_allLinkedAttributes.GetAttribute<float>("BrushWeight") * m_AdditivePower))){
						if (!ModifiedChunks.Contains (chunkCoord)) {
							ModifiedChunks.Add (chunkCoord);
						}
					}
				}
			}
		}

		return ModifiedChunks;
	}
		
	// FILE LOADINGS 

	public void DoFileDialogue() {
		WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));

		if (_path == null)
			return;

		StartCoroutine(GetComponent<TextureImport>().ImportFile(_path, "ToolManager"));
	}

	// Multi file
	public void WriteResult(string[] paths) {
		if (paths.Length == 0) {
			return;
		}

		_path = "";
		foreach (var p in paths) {
			_path += p + "\n";
		}
	}

	// Single file
	public void WriteResult(string path) {
		_path = path;
	}
}
	