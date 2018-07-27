using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisClampedDirectionWidget : Widget {
	[System.Serializable]
	public enum PinLayout {NineDir, CrossFour, DiagFour};

	public PinLayout m_pinLayout;

	public string m_ACIDAttributeName;

	// Pin back plate, for active and inactive direction pins
	public GameObject m_pinBack;
	// Pin core overlay, for active direction pin
	public GameObject m_pinCore;


	// Instantiated pin core
	private GameObject m_pinCoreInstance;

	//Instantiated pin bases
	private GameObject[,] m_pinBaseInstances;

	// Relative rect for placing pins
	private RectTransform m_widgetContainer;

	//Dimension of the pinBase image
	private float m_squarePinDimension;

	// Is the mouse still being held down?
	private bool m_mouseDown = false;

	private Vector2 m_initMousePos;

	// Use this for initialization
	void Start () {
		// new Array of pins
		m_pinBaseInstances = new GameObject[3,3];

		Vector2 loadedValue = GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<Vector2> (m_ACIDAttributeName);

		// Create the new value attribute
		AddAttribute <Vector2> (m_ACIDAttributeName, loadedValue);

		// Mouse pos cache
		m_initMousePos = Vector2.zero;

		// Cache my rect
		m_widgetContainer = GetComponent<RectTransform> ();
		m_squarePinDimension = m_pinBack.GetComponent<RectTransform> ().rect.width;

		// place the pins initially
		for (int i = 0; i < 3; ++i) {
			for (int j = 0; j < 3; ++j) {
				// Creat the new pin bases
				m_pinBaseInstances[i,j] = Instantiate(m_pinBack, transform.position, Quaternion.identity) as GameObject;

				// Parent to panel
				m_pinBaseInstances[i,j].transform.SetParent (transform);

				m_pinBaseInstances[i,j].transform.position = new Vector3 (
					transform.position.x + ((i - 1) * ((m_widgetContainer.rect.width / 2) - m_squarePinDimension)), 
					transform.position.y + ((j - 1) * ((m_widgetContainer.rect.height / 2) - m_squarePinDimension)), 
					0
				);
			}
		}

		// Creat the new active icon
		m_pinCoreInstance = Instantiate(m_pinCore, transform.position, Quaternion.identity) as GameObject;

		// Place the active icon to the incoming value
		PlacePinCore (loadedValue);
	}

	// Place pin core at active pin
	void PlacePinCore(Vector2 pinValue){
		// Parent to the base which was also created this loop (0,0)
		m_pinCoreInstance.transform.SetParent (m_pinBaseInstances[(int)pinValue.x + 1, (int)pinValue.y + 1].transform);

		// Offset accordingly
		m_pinCoreInstance.transform.position = new Vector3 (
			transform.position.x + (pinValue.x * ((m_widgetContainer.rect.width / 2) - m_squarePinDimension)), 
			transform.position.y + (pinValue.y * ((m_widgetContainer.rect.height / 2) - m_squarePinDimension)), 
			0
		);
	}

	// Widget has been clicked, check for the direction
	void CheckDir(){
		Vector2 modDir = Vector2.zero;

		// Xmod
		if (Mathf.Abs (Input.mousePosition.x - m_initMousePos.x) > m_widgetContainer.rect.width / 3) {
			modDir = (Input.mousePosition.x < m_initMousePos.x) ? new Vector2 (-1, modDir.y) : new Vector2 (1, modDir.y);
		}

		//Ymod
		if (Mathf.Abs (Input.mousePosition.y - m_initMousePos.y) > m_widgetContainer.rect.height / 3) {
			modDir = (Input.mousePosition.y < m_initMousePos.y) ? new Vector2 (modDir.x, -1) : new Vector2 (modDir.x, 1);
		}
			
		SetAttribute<Vector2> (m_ACIDAttributeName, modDir);
		GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().SetAttribute<Vector2> (m_ACIDAttributeName, modDir);
		PlacePinCore (modDir);
	}

	// Is the mouse within the active area
	bool IsMousePresent() {
		if (Input.mousePosition.x > m_widgetContainer.transform.position.x - (m_widgetContainer.rect.width / 2)
			&& Input.mousePosition.x < m_widgetContainer.transform.position.x + (m_widgetContainer.rect.width / 2)
			&& Input.mousePosition.y > m_widgetContainer.transform.position.y - (m_widgetContainer.rect.height / 2)
			&& Input.mousePosition.y < m_widgetContainer.transform.position.y + (m_widgetContainer.rect.height / 2)) {
			return true;
		}

		return false;
	}
	
	// Update is called once per frame
	void Update () {
		// Mouse was just clicked, check and cache if necessary
		if (Input.GetMouseButtonDown (0)) {
			if (IsMousePresent()) {
				m_initMousePos = Input.mousePosition;
			}
		}

		// If the mouse is released, mouse button down is definitely false
		if (Input.GetMouseButton (0)) {
			if (IsMousePresent()) 
			{
				// Mouse is down and was in the Widget zone
				m_mouseDown = true;
			}
		} else {
			m_mouseDown = false;
		}


		// Check the direction
		if (m_mouseDown) {
			CheckDir ();
		}
	}
}
