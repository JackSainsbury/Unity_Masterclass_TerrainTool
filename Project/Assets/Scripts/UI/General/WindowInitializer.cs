using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowInitializer : MonoBehaviour {

	public GameObject[] m_windowPanelsToAdd;

	public string m_windowTitle = "NewWindow";

	public void addWindowPanels(GameObject window){
		DockWindowScript m_dockScript = window.GetComponent<DockWindowScript> ();

		// Add all the widgets to the new reference in order
		foreach (GameObject widget in m_windowPanelsToAdd) {
			m_dockScript.m_scrollPanelWindowPanels.Add (widget);
		}
	}
}
