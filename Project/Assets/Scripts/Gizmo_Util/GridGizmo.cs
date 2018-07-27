using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGizmo : MonoBehaviour {

    // The Height at which the camera fades
    public float m_yGridStartFade = 100.0f;

	// The Height at which the camera fades
	public float m_yGridFullFade = 200.0f;

    // Distance between grid lines
    private float m_gridSize = 1;

    // The weight of these grid lines
    private float m_gridLineOpacity = .5f;

    // The user camera
    private GameObject m_userCamera;

    // Use this for initialization
    void Start()
    {

		// Order the fade heights appropriately
		if (m_yGridFullFade < m_yGridStartFade) {
			// Swap heights
			float temp = m_yGridStartFade;
			m_yGridStartFade = m_yGridFullFade;
			m_yGridFullFade = temp;
		}

        // Safety Check grid size
        m_gridSize = (m_gridSize <= 0) ? 1 : m_gridSize;
        // Safety Check grid opacity
        m_gridLineOpacity = (m_gridLineOpacity < 0 || m_gridLineOpacity > 1) ? 1 : m_gridLineOpacity;

        // Get the user camera, for testing offset
        m_userCamera = GameObject.FindGameObjectWithTag("MainCamera");


		// Init the opacity to default value
		Color m_newGridColor = new Color(GetComponent<MeshRenderer> ().material.color.r, GetComponent<MeshRenderer> ().material.color.g, GetComponent<MeshRenderer> ().material.color.b, m_gridLineOpacity);
		GetComponent<MeshRenderer> ().material.color = m_newGridColor;
    }

    void Update ()
    {
		if (m_userCamera.transform.position.y > m_yGridFullFade) 
		{
			if (GetComponent<MeshRenderer> ().material.color.a <= 0) 
			{
				GetComponent<MeshRenderer> ().enabled = false;
			}
		} else 
		{
			GetComponent<MeshRenderer> ().enabled = true;
		}

		// Height fading
		if (m_userCamera.transform.position.y > m_yGridStartFade)
        {
			// Get the current colour
			Color m_newGridColor = GetComponent<MeshRenderer> ().material.color;

			// If equal, quick time fade
			if (m_yGridStartFade == m_yGridFullFade) 
			{
				// Fade the grid
				if (GetComponent<MeshRenderer> ().material.color.a > 0) {
					m_newGridColor = new Color (m_newGridColor.r, m_newGridColor.g, m_newGridColor.b, m_newGridColor.a - Time.deltaTime);
					GetComponent<MeshRenderer> ().material.color = m_newGridColor;
				}
			} else 
			{
				// proportion based on range and current height
				float proportion = (m_userCamera.transform.position.y - m_yGridStartFade) / (m_yGridFullFade - m_yGridStartFade);

				// Proportionally Fade the grid
				m_newGridColor = new Color (m_newGridColor.r, m_newGridColor.g, m_newGridColor.b, m_gridLineOpacity * (1 - proportion));
				GetComponent<MeshRenderer> ().material.color = m_newGridColor;
			}
        }
        else
        {
			// If equal, quick time unfade
			if (m_yGridStartFade == m_yGridFullFade) 
			{
				// Get the current colour
				Color m_newGridColor = GetComponent<MeshRenderer> ().material.color;

				// UnFade the grid
				if (GetComponent<MeshRenderer> ().material.color.a < m_gridLineOpacity) {
					m_newGridColor = new Color (m_newGridColor.r, m_newGridColor.g, m_newGridColor.b, m_newGridColor.a + Time.deltaTime);
					GetComponent<MeshRenderer> ().material.color = m_newGridColor;
				}
			}
        }
			
		transform.position = new Vector3 (
			(m_userCamera.transform.position.x >= 0) ? 10 * Mathf.Floor(m_userCamera.transform.position.x / 10.0f) : 10 * Mathf.Ceil(m_userCamera.transform.position.x / 10.0f),
			transform.position.y, 
			(m_userCamera.transform.position.z >= 0) ? 10 * Mathf.Floor(m_userCamera.transform.position.z / 10.0f) : 10 * Mathf.Ceil(m_userCamera.transform.position.z / 10.0f)
		);
    }

    // Set the current grid size
    public void SetGridSize(float value)
    {
        m_gridSize = value;
    }

    // Set the current grid opacity
    public void SetGridOpacity(float value)
    {
        m_gridLineOpacity = value;
    }
}
