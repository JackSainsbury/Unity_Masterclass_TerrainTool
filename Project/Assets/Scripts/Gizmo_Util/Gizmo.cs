using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ProcessType{
	Translate,
	Rotate
}

public class Gizmo : MonoBehaviour {
	// Visualizer to show which axis is clicked
	public GameObject m_colourView;

	// Handles that can be grabbed if gizmo is a translator
	public GameObject m_translateHandles;
	// Handles that can be grabbed if gizmo is a rotator
	public GameObject m_rotateHandles;

	// What process does this gizmo perform?
	public ProcessType m_processType;

	// The reset colour material for the centre of the gizmo
	public Material m_resetMaterial;


	// Is a gizmo being held down (disables drawing mode).
	public bool m_gizmoActive = false;

	// Scale of the gizmos
	public float m_gizmoScale = .1f;
	// Scale of the gizmos
	public float m_gizmoMovementPower = .1f;

	// If this gizmo should exist hierarchically (handles for bezier etc)
	public Transform m_childGizmo;



	//The handle set currently being displayed
	private GameObject m_instancedHandles;
	// The currently help axisHandle
	private GameObject m_activeHandle;

	// Which axis are active
	private Vector3 m_activeAxis = Vector3.zero;
	// Mouse position last tick
	private Vector3 m_lastMousePosition;

	// Has the axis been processed
	private bool m_axisProcessed = false;

	// Ring is being dragged
	private bool m_ringActive = false;

	// Number of axis manipulated by the active handle (1 for single axis, 2 for planar)
	private int numberOfAxis = 0;

	// Reference tool manager script
	private ToolManager m_toolManagerScript;

	// Use this for initialization
	void Start () {
		// Cache the last mouse pos on the first frame
		m_lastMousePosition = Input.mousePosition;	
		// Cache the cave manager
		m_toolManagerScript = GameObject.FindGameObjectWithTag ("ToolManager").GetComponent<ToolManager> ();

		SetHandleProcess (ProcessType.Translate);
	}

	// Only ever going to be 2 of the 3 axis
	int ProcessAxis (string m_processString) {
		int numberOfAxis = 0;

		foreach (char c in m_processString) {
			switch (c) {
			case 'X':
				m_activeAxis = new Vector3 (1, m_activeAxis.y, m_activeAxis.z);
				numberOfAxis++;
				break;
			case 'Y':
				m_activeAxis = new Vector3 (m_activeAxis.x, 1, m_activeAxis.z);
				numberOfAxis++;
				break;
			case 'Z':
				m_activeAxis = new Vector3 (m_activeAxis.x, m_activeAxis.y, 1);
				numberOfAxis++;
				break;
			}
		}

		return numberOfAxis;
	}

	//Change the process type
	public void SetHandleProcess(ProcessType newType){
		// The process has changed
		if (newType != m_processType) {
			if (m_instancedHandles != null) {
				Destroy (m_instancedHandles);
			}

			// Display Handles
			switch (newType) {
			case ProcessType.Translate:
				m_instancedHandles = Instantiate (m_translateHandles, transform.position, transform.rotation) as GameObject;
				m_instancedHandles.transform.SetParent (transform);
				break;
			case ProcessType.Rotate:
				m_instancedHandles = Instantiate (m_rotateHandles, transform.position, transform.rotation) as GameObject;
				m_instancedHandles.transform.SetParent (transform);
				break;
			}

			m_instancedHandles.transform.localScale = m_colourView.transform.localScale;

			m_processType = newType;
		}
	}

	// Update is called once per frame
	void Update () {
		float dist = Vector3.Distance (transform.position, Camera.main.transform.position);
		float m_size = ((dist > 1) ? dist : 1) * m_gizmoScale;

		transform.localScale = new Vector3 (m_size, m_size, m_size);

		// Hit out from raycast
		RaycastHit hit;

		// Cast the mouse position to world hit
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		// Ray hit a collider
		if (Physics.Raycast (ray, out hit)) {
			if (!m_gizmoActive) {
				// Get the clicked gizmo handle
				if (hit.collider.tag == "GizmoHandle") {

					if (Input.GetMouseButton (0)) {
						// Disable sketch mode
						m_gizmoActive = true;

						m_toolManagerScript.SetAllowedToDraw (false);

						// Cache the object
						m_activeHandle = hit.collider.gameObject;

						// Process the axis from the clicked object's name
						if (!m_axisProcessed) {
							numberOfAxis = ProcessAxis (hit.collider.gameObject.name);

							if (hit.transform.childCount == 0) {
								m_colourView.GetComponent<Renderer> ().material = hit.collider.GetComponent<Renderer> ().material;
							} else {
								m_colourView.GetComponent<Renderer> ().material = hit.transform.GetChild (0).GetComponent<Renderer> ().material;
							}
						}
					}
				} else {
					if (hit.collider.tag == "GizmoRing") {

						m_ringActive = true;

						m_toolManagerScript.SetAllowedToDraw (false);

						// Cache the object
						m_activeHandle = hit.collider.gameObject;
					}
				}
			}
		}

		if (!Input.GetMouseButton (0)){
			// Re-enable sketch mode
			m_gizmoActive = false;
			m_ringActive = false;
			m_toolManagerScript.SetAllowedToDraw (true);
		}

		if (m_ringActive) {
			float mouseMove = m_lastMousePosition.x - Input.mousePosition.x;
			if (mouseMove < 0) {
				if (m_activeHandle.transform.root.localScale.x > 2) {
					m_activeHandle.transform.root.localScale += new Vector3 (mouseMove * .1f, mouseMove * .1f, mouseMove * .1f);
				}
			} else {
				m_activeHandle.transform.root.localScale += new Vector3 (mouseMove * .1f, mouseMove * .1f, mouseMove * .1f);
			}

			CaveManager cmanager = GameObject.FindGameObjectWithTag ("CaveManager").GetComponent<CaveManager> ();

			if (m_activeHandle.transform.root != null) {
				m_activeHandle.transform.root.GetComponent<GizmoRing> ().m_bezier.UpdateMesh (cmanager.GetDivRes (), cmanager.GetRoundRes ());
			}
		}

		// The gizmo is active, do logic
		if (m_gizmoActive && m_activeHandle.transform.root.gameObject == this.gameObject) {
			Vector2 mouseMove = m_lastMousePosition - Input.mousePosition;

			// Do the correct process in the retrieved axis
			switch (m_processType) {
			case ProcessType.Translate:
				{
					// SingleAxis
					if (numberOfAxis == 1) {
						// Up is along axis, convert to screen space
						Vector3 screenPos = Camera.main.WorldToScreenPoint (m_activeHandle.transform.position);
						Vector3 screenPosUp = Camera.main.WorldToScreenPoint (m_activeHandle.transform.position + m_activeHandle.transform.up);

						float colinearity = Vector2.Dot (
							                    Vector3.Normalize (new Vector2 (screenPos.x - screenPosUp.x, screenPos.y - screenPosUp.y)),
							                    Vector3.Normalize (new Vector2 (mouseMove.x, mouseMove.y))
						                    );

						Vector3 newPos = m_activeHandle.transform.up * colinearity * (mouseMove.magnitude * m_gizmoMovementPower) * (m_size * m_gizmoMovementPower);

						newPos = (newPos.magnitude > 10000) ? Vector3.zero : newPos;

						transform.Translate (newPos, Space.World);

						if (m_childGizmo != null) {
							m_childGizmo.Translate (newPos, Space.World);
						}

					} else {
						// Planar motion
						Plane plane = new Plane (m_activeHandle.transform.up, transform.position);

						//Vector2 mouseMove = m_lastMousePosition - Input.mousePosition;
						// Create a current and last ray
						Ray curRay = Camera.main.ScreenPointToRay(Input.mousePosition);
						Ray lastRay = Camera.main.ScreenPointToRay(m_lastMousePosition);

						//Distance along ray (on plane)
						float distance;

						Vector3 curpos = Vector3.zero;
						Vector3 lastpos = Vector3.zero;;

						// Do the raycasts
						if (plane.Raycast (lastRay, out distance)) {
							lastpos = lastRay.GetPoint (distance);
						}
						if (plane.Raycast (curRay, out distance)) {
							curpos = curRay.GetPoint (distance);
						}

						Vector3 newPos = (curpos - lastpos) * (mouseMove.magnitude * m_gizmoMovementPower) * (m_size * m_gizmoMovementPower);

						newPos = (newPos.magnitude > 10000) ? Vector3.zero : newPos;

						transform.Translate (newPos, Space.World);

						if (m_childGizmo != null) {
							m_childGizmo.Translate (newPos, Space.World);
						}
					}
				}
				break;
			case ProcessType.Rotate:
				{
					transform.Rotate(m_activeHandle.transform.up * mouseMove.x, Space.World);

					if (m_childGizmo != null) {
						m_childGizmo.RotateAround(transform.position, m_activeHandle.transform.up, mouseMove.x);
					}
				}
				break;
			}
		} else {
			// Reset the core material colour
			m_colourView.GetComponent<Renderer> ().material = m_resetMaterial;
			// Reset the currently active axis (because I check for them additively)
			m_activeAxis = Vector3.zero;
			// A bit of a messy way to store active axis but oh well
			numberOfAxis = 0;
		}

		// Cache mouse pos at end of update tick
		m_lastMousePosition = Input.mousePosition;
	}
}
