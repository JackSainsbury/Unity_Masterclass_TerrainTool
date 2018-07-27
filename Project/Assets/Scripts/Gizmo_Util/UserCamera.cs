using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCamera : MonoBehaviour {

	// Public members

	public float m_cameraSensitivity = 90;
	public float m_climbSpeed = 4;
	public float m_moveSpeed = 10;

	public float m_boostFactor = 3;
	public float m_crawlFactor = 0.25f;



	// Private members

	private float m_rotationX = 0.0f;
	private float m_rotationY = 0.0f;

	private bool m_LookDown = false;

	// First tick
	void Start ()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	// Native update tick
	void Update ()
	{
		if (Input.GetMouseButton (1)) {
			m_LookDown = true;
		} else {
			m_LookDown = false;
		}

		if (m_LookDown) {
			CamControls ();
		}else{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

	}

	// Camera move and mouse look function
	void CamControls(){
		Cursor.visible = false;

		Cursor.lockState = CursorLockMode.Locked;

		m_rotationX += Input.GetAxis ("Mouse X") * m_cameraSensitivity * Time.deltaTime;
		m_rotationY += Input.GetAxis ("Mouse Y") * m_cameraSensitivity * Time.deltaTime;
		m_rotationY = Mathf.Clamp (m_rotationY, -90, 90);

		transform.localRotation = Quaternion.AngleAxis (m_rotationX, Vector3.up);
		transform.localRotation *= Quaternion.AngleAxis (m_rotationY, Vector3.left);

		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			// Boosted Move input
			transform.position += transform.forward * m_moveSpeed * m_boostFactor * Input.GetAxis ("Vertical") * Time.deltaTime;
			transform.position += transform.right * m_moveSpeed * m_boostFactor * Input.GetAxis ("Horizontal") * Time.deltaTime;
		} else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) {
			// Crawling Move input
			transform.position += transform.forward * m_moveSpeed * m_crawlFactor * Input.GetAxis ("Vertical") * Time.deltaTime;
			transform.position += transform.right * m_moveSpeed * m_crawlFactor * Input.GetAxis ("Horizontal") * Time.deltaTime;
		} else {
			// Standard Move input
			transform.position += transform.forward * m_moveSpeed * Input.GetAxis ("Vertical") * Time.deltaTime;
			transform.position += transform.right * m_moveSpeed * Input.GetAxis ("Horizontal") * Time.deltaTime;
		}

		// Vertical Processing
		if (Input.GetKey (KeyCode.Q)) {
			transform.position += transform.up * m_climbSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.E)) {
			transform.position -= transform.up * m_climbSpeed * Time.deltaTime;
		}
	}
}