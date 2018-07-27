using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public struct dockClass
{
    // Used to reference the dock area in screen space
    public Vector2 m_dockCentre;
    // Non 0 value represents dock orientation
    public Vector2 m_dockDimension;
    // The GameObject associated with this dock area
    public DockScript m_dockObject;
}

public class WindowController : MonoBehaviour {
	
	//--------------PUBLIC MEMBERS--------------

    public int m_dockPixelProximity = 200;
    public int m_minimizeWidth = 100;
    public int m_minimizeDisplayPixelBuffer = 10;

    public GameObject m_screenDockPrefab;

    public List<dockClass> m_dockWindows;

	public GameObject m_newWindowPrefab;

	public GameObject m_togglesContainer;

	public List<Toggle> m_windowToggles;

	// Rect cache to buffer the top base dock down past the menu bar
	public RectTransform m_topBarRect;

	//--------------PRIVATE MEMBERS--------------

    private List<GameObject> m_activeWindows;


	public void Start()
	{
		foreach (Transform child in m_togglesContainer.transform) {
			m_windowToggles.Add (child.GetComponent<Toggle>());
		}

		m_activeWindows = new List<GameObject>();
		m_dockWindows = new List<dockClass>();

		// Add our 4 base docks
		newBaseDock(new Vector2(-1,0));
		newBaseDock(new Vector2(1, 0));
		newBaseDock(new Vector2(0, -1));
		newBaseDock(new Vector2(0, 1));
	}

    dockClass makeDockClass(Vector2 Centre, Vector2 Dimension, DockScript DockScriptReference)
    {
        dockClass newDockClass = new dockClass();

        newDockClass.m_dockCentre = Centre;
        newDockClass.m_dockDimension = Dimension;
        newDockClass.m_dockObject = DockScriptReference;

        return newDockClass;
    }

    public dockClass CheckDocks(Vector2 inPos)
    {

        dockClass closestDock = new dockClass();

        bool success = false;

        for(int i = 0; i < m_dockWindows.Count; ++i)
        {
            // Is this a horizontal or a vertical boundary?
            if (m_dockWindows[i].m_dockDimension.x != 0)
            {
                float yCheck = Mathf.Abs(inPos.y - m_dockWindows[i].m_dockCentre.y);
                if (yCheck < m_dockPixelProximity)
                {
                    //Candidate is in range, is it the closest?
                    if (yCheck <= Mathf.Abs(inPos.y - closestDock.m_dockCentre.y) && yCheck < Mathf.Abs(inPos.x - closestDock.m_dockCentre.x))
                    {
                        closestDock = m_dockWindows[i];
                        success = true;
                    }
                }
            }
            else
            {
                float xCheck = Mathf.Abs(inPos.x - m_dockWindows[i].m_dockCentre.x);

                if (xCheck < m_dockPixelProximity)
                {
                    //Candidate is in range, is it the closest?
                    if (xCheck <= Mathf.Abs(inPos.x - closestDock.m_dockCentre.x) && xCheck < Mathf.Abs(inPos.y - closestDock.m_dockCentre.y))
                    {
                        closestDock = m_dockWindows[i];
                        success = true;
                    }
                }
            }
        }

        if (success)
        {
            return closestDock;
        }
        else
        {
            //Use this clunky method to imply a null return
            dockClass blankDock = new dockClass();
            blankDock.m_dockDimension = new Vector2(-1,0);

            // Failed (not in dock range - width should never be -1 if dock is possible, so this is a safe way to check
            return blankDock;
        }
    }
		
    void newBaseDock(Vector2 dir)
    {
		float menuBarBuffer = m_topBarRect.rect.height + m_topBarRect.rect.y + 8;

        // Create the 4 bounding docks, put y and x into the oposite dock dimensions as they are perpendicular to the centres
		GameObject nDock = Instantiate(m_screenDockPrefab, new Vector3(Screen.width / 2 - ((Screen.width / 2) * dir.x), Screen.height / 2 - ((Screen.height / 2) * dir.y) - ((dir.y == - 1) ? menuBarBuffer : 0), 0), Quaternion.identity) as GameObject;
        nDock.transform.SetParent(transform);
		m_dockWindows.Add(makeDockClass(new Vector2(nDock.transform.position.x, nDock.transform.position.y - ((dir.y == 0) ? menuBarBuffer/2 : 0)), new Vector2(Screen.width * Mathf.Abs(dir.y), (Screen.height - menuBarBuffer) * Mathf.Abs(dir.x)), nDock.GetComponent<DockScript>()));
        
        // Set the door direction
        m_dockWindows[m_dockWindows.Count - 1].m_dockObject.DockDirection = dir;
    }

    public bool CheckIfCanBeDragged(int windowID)
    {
        bool canDrag = true;

        foreach (GameObject w in m_activeWindows)
        {
            if (w.GetComponent<DockWindowScript>().m_windowID != windowID)
            {
                // A window is already being dragged
                if (w.GetComponent<DockWindowScript>().m_beingDragged == true)
                {
                    canDrag = false;
                }
            }
        }
        return canDrag;
    }

    // Minimized windows along the bottom of the window
    public int getNextMinimizedSlot(int windowID)
    {
        int nextSlot = 0;

        foreach (GameObject w in m_activeWindows)
        {
            if (windowID != w.GetComponent<DockWindowScript>().m_windowID)
            {
                if (w.GetComponent<DockWindowScript>().m_currentState == WindowState.Minimized)
                {
                    nextSlot++;
                }
            }
        }

        return nextSlot;
    }

    public void ChangeWindowOpenState(int windowID)
    {
        if(m_windowToggles[windowID].isOn == false)
        {
			CloseOldWindow (windowID);
        }
        else
        {
			InstanceNewWindow (windowID);
        }
    }

	void CloseOldWindow(int windowID){
		GameObject toRemove = null;
		foreach(GameObject w in m_activeWindows){
			if (w.GetComponent<DockWindowScript>().m_windowID == windowID)
			{
				toRemove = w;
			}
		}

		if (toRemove != null)
		{
			m_activeWindows.Remove(toRemove);
			toRemove.GetComponent<DockWindowScript>().CloseButton();
		}
	}

	void InstanceNewWindow(int windowID){
		// Instance a fresh window ready for widgets to be added
		GameObject newWindow = Instantiate(m_newWindowPrefab, new Vector3(Screen.width / 2, Screen.height / 2, 0), Quaternion.identity) as GameObject;

		// Parent to the window container
		newWindow.transform.SetParent(this.transform, true);

		// Set the new window id
		newWindow.GetComponent<DockWindowScript> ().m_windowID = windowID;

		// Set the title of the new window
		newWindow.GetComponent<DockWindowScript> ().SetWindowTitle (m_windowToggles [windowID].GetComponent<WindowInitializer> ().m_windowTitle);

		// Add the widgets to the newly created window
		m_windowToggles [windowID].GetComponent<WindowInitializer> ().addWindowPanels (newWindow);

		// Add our new window to the window list
		m_activeWindows.Add(newWindow);
	}

    public void CloseWindow(int windowID)
    {
        m_windowToggles[windowID].isOn = false;
    }
}
