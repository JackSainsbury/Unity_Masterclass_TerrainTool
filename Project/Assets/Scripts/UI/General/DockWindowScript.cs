using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WindowState { 
	Free, 
	Docked, 
	Minimized 
};

public class DockWindowScript : MonoBehaviour {

    //--------------PUBLIC MEMBERS--------------

    public WindowState m_currentState;

	// The content panel (background)
    public GameObject m_contentPanel;
	// The widget scroll panel (innerArea)
	public GameObject m_windowPanelScrollArea;
	// The panel to actually hold the content
	public GameObject m_windowPanelContentArea;

	// The title at the top of the window
	public GameObject m_windowTitleLabel;

	// The active drag area where the user may click to drag the window
    public RectTransform m_dragRect;

	// Max pixel distance this window is allowed to be off screen
    public float m_maxOffScreenPixelBuffer = 20;

	// Bool to determine if this current window is being dragged by the user
    public bool m_beingDragged = false;

	// This window's id
    public int m_windowID = 0;

	// The initial, reset and inactive dock dimensions of the window
	public Vector2 m_initialWindowDimensions;

	// Set of panel content widgets to hold in this window's scroll area
	public List<GameObject> m_scrollPanelWindowPanels;
	// Instaned panels
	public List<GameObject> m_activeScrollPanels;

	// 2 Scroll bars (individual in the event I need both simultaneously)
	public GameObject m_verticalScrollBar;
	public GameObject m_horizontalScollBar;

	// The width of scroll bars (get from window controller? - settings menu?)
	public float m_scrollBarWidth = 15.0f;

    //--------------PRIVATE MEMBERS--------------


    private GameObject m_windowController;
    private WindowController m_windowControllerScriptCache;

    private Rect m_dragBarCombinedDimensions;
    private Rect defaultFreeRect;

    private RectTransform m_myTransform;

    private Vector2 m_lastMousePosition;
    private Vector2 m_lastScreenRes;

    private Vector2 m_dragStartVector;

    private bool m_dragCached;

	// Containing the content offset
	private Vector2 m_scrollOffset;

	// if docked, is it horizontal? Messy.
	private bool m_horizontallyDocked = true;

	// Temporary value as UI is not focus
	private float m_widgetDefaultSquare = 190;

    // Use this for initialization
    void Start () {
		m_widgetDefaultSquare = m_widgetDefaultSquare - m_scrollBarWidth;

		GetComponent<RectTransform> ().sizeDelta = m_initialWindowDimensions;

        m_myTransform = GetComponent<RectTransform>();

        defaultFreeRect = m_myTransform.rect;

        // Align to top left
        m_dragBarCombinedDimensions = new Rect();
        UpdateRecPositions();
        m_lastMousePosition = Input.mousePosition;
        m_dragStartVector = Input.mousePosition;
        m_dragCached = false;
        m_windowController = GameObject.FindGameObjectWithTag("WindowController");

		// Set the window to free initially
        m_currentState = WindowState.Free;
        m_windowControllerScriptCache = m_windowController.GetComponent<WindowController>();

		// Set the scroll area background to just smaller than the window
		ResizeScrollArea ();

		// Initialize to no scroll offset ( will happen anyway, but this is safe, if I then want to init to non zero value, just Set externally from scroll bars in start )
		m_scrollOffset = Vector2.zero;

		// Initially draw the panels
		ReDrawPanels ();

		//Scale the scrollbars accordingly
		m_verticalScrollBar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_scrollBarWidth, m_verticalScrollBar.GetComponent<RectTransform> ().rect.width - m_scrollBarWidth * 1.5f);
		m_horizontalScollBar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_horizontalScollBar.GetComponent<RectTransform> ().rect.height  - m_scrollBarWidth * 1.5f, m_scrollBarWidth);

		// Now a free window, so swap the active scroll bar to just vert
		m_verticalScrollBar.SetActive (true);
		m_horizontalScollBar.SetActive(false);
    }

	public void ReDrawPanels(){
		if (m_activeScrollPanels.Count > 0) {
			foreach (GameObject go in m_activeScrollPanels) {
				Destroy (go);
			}
			m_activeScrollPanels.Clear ();
		}

		// Instantiate and Initialize the window widgets
		for (int i = 0; i < m_scrollPanelWindowPanels.Count; ++i) {
			// Replace the widget references with instantiated objects of the reference - lose the ability to instance new references - Messy?
			m_activeScrollPanels.Add(Instantiate (m_scrollPanelWindowPanels [i], m_windowPanelScrollArea.GetComponent<RectTransform> ().position, Quaternion.identity) as GameObject);

			// Parent all widgets to the scroll area
			m_activeScrollPanels [i].transform.SetParent(m_windowPanelContentArea.transform);

			m_activeScrollPanels [i].GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_widgetDefaultSquare, m_widgetDefaultSquare);
		}
			
		// Position the scroll widgets initially, vertically
		ScrollOffset(true);
	}

	public void SetWindowTitle(string title){
		m_windowTitleLabel.GetComponent<Text> ().text = title;
	}

	float GetMaxScrollSize (bool horizontalScroll){
		// Difference between stacked widgets size and the active scroll axis
		float scrollAxisDelta = (horizontalScroll) ? m_windowPanelContentArea.GetComponent<RectTransform> ().rect.width : m_windowPanelContentArea.GetComponent<RectTransform> ().rect.height;

		// Check if there is over spill
		float ScrollAllowance = (((m_activeScrollPanels.Count) * m_widgetDefaultSquare) - scrollAxisDelta > 0) ? ((m_activeScrollPanels.Count) * m_widgetDefaultSquare) - scrollAxisDelta : 0;

		m_horizontalScollBar.GetComponent<Scrollbar>().size = Mathf.Clamp(m_windowPanelContentArea.GetComponent<RectTransform> ().rect.width / ((m_activeScrollPanels.Count) * m_widgetDefaultSquare), 0, 1);
		m_verticalScrollBar.GetComponent<Scrollbar>().size = Mathf.Clamp(m_windowPanelContentArea.GetComponent<RectTransform> ().rect.height / ((m_activeScrollPanels.Count) * m_widgetDefaultSquare), 0, 1);

		return ScrollAllowance;
	}
		
	// Mod the scroll value
	public void ScrollOffset(bool horizontalScroll){
		// Basic scrollbar input values
		float deltaHor = m_horizontalScollBar.GetComponent<Scrollbar>().value;
		float deltaVer = m_verticalScrollBar.GetComponent<Scrollbar>().value;
	
		// Set the active axis to the in component, and leave the inactive axis as the current value
		m_scrollOffset = new Vector2(
			(horizontalScroll) ? deltaHor * GetMaxScrollSize(horizontalScroll) : m_scrollOffset.x, 
			(horizontalScroll) ? m_scrollOffset.y : deltaVer * GetMaxScrollSize(horizontalScroll)
		);

		// Position widgets in content area, plus scroll offset
		PositionScrollWidgets (m_scrollOffset);
	}

	//Position the widgets in the scroll area
	void PositionScrollWidgets(Vector2 scrollOffset){
		for (int i = 0; i < m_activeScrollPanels.Count; ++i) {
			float offset = (-m_widgetDefaultSquare * i) + ((m_horizontallyDocked) ? scrollOffset.y : scrollOffset.x);

			// Stack all the panelWidgets & apply current scroll offset
			m_activeScrollPanels [i].transform.position = new Vector3 (
				m_windowPanelContentArea.transform.position.x - ((m_horizontallyDocked) ? 0 : offset + ((m_windowPanelContentArea.GetComponent<RectTransform>().rect.width/2) - (m_widgetDefaultSquare/2))), 
				m_windowPanelContentArea.transform.position.y + ((m_horizontallyDocked) ? offset + ((m_windowPanelContentArea.GetComponent<RectTransform>().rect.height/2) - (m_widgetDefaultSquare/2)) : 0), 
				0
			);
		}
	}
		
	// Resize the innner scroll area to fit the content panel (typically on dock or maximize)
	void ResizeScrollArea(){
		m_windowPanelScrollArea.GetComponent<RectTransform> ().position = new Vector3 (transform.position.x, transform.position.y - (m_dragRect.rect.height/2), 0);
		m_windowPanelScrollArea.GetComponent<RectTransform> ().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width - 10, GetComponent<RectTransform>().rect.height - m_dragRect.rect.height - 10);

		PositionScrollWidgets (m_scrollOffset);
	}

    void UpdateRecPositions()
    {
        //Check if the window has been dragged off screen OR if whilst resizing it has ended up somewhere it shouldn't be
        if (m_myTransform.position.x < (-m_maxOffScreenPixelBuffer))
        {
            GetComponent<RectTransform>().position = new Vector3(-m_maxOffScreenPixelBuffer, m_myTransform.position.y, 0);
        }
        else if (m_myTransform.position.x > (Screen.width + m_maxOffScreenPixelBuffer))
        {
            GetComponent<RectTransform>().position = new Vector3(Screen.width + m_maxOffScreenPixelBuffer, m_myTransform.position.y, 0);
        }

        if (m_myTransform.position.y < (m_maxOffScreenPixelBuffer - (m_myTransform.rect.height/2)))
        {
            GetComponent<RectTransform>().position = new Vector3(m_myTransform.position.x, (m_maxOffScreenPixelBuffer - (m_myTransform.rect.height / 2)), 0);
        }
        else if (m_myTransform.position.y > (Screen.height - (m_myTransform.rect.height / 2)))
        {
            GetComponent<RectTransform>().position = new Vector3(m_myTransform.position.x, Screen.height - (m_myTransform.rect.height / 2), 0);
        }

        //Align bottom left
        m_dragBarCombinedDimensions.x = m_myTransform.position.x - m_myTransform.rect.width / 2;
        m_dragBarCombinedDimensions.y = m_myTransform.position.y + (m_myTransform.rect.height / 2) - m_dragRect.rect.height;
        m_dragBarCombinedDimensions.width = m_dragRect.rect.width;
        m_dragBarCombinedDimensions.height = m_dragRect.rect.height;
    }

	// Break from locked state
    void BreakWindowFreeze(float breakDist)
    {
        if ((m_dragStartVector - new Vector2(Input.mousePosition.x, Input.mousePosition.y)).magnitude > breakDist)
        {
            transform.Translate(new Vector3(Input.mousePosition.x - m_dragStartVector.x, Input.mousePosition.y - m_dragStartVector.y, 0));

            // Reset the window size to default free scale
            m_myTransform.sizeDelta = new Vector2(defaultFreeRect.width, defaultFreeRect.height);

            // Redisplay the panel
            m_contentPanel.SetActive(true);

            UpdateRecPositions();

            m_currentState = WindowState.Free;

			ResizeScrollArea ();
			m_horizontallyDocked = true;
			ScrollOffset (m_horizontallyDocked);


			// Now a free window, so swap the active scroll bar to just vert
			m_verticalScrollBar.SetActive (true);
			m_horizontalScollBar.SetActive(false);
        }
    }

	// Drag a window (limited logic currently!)
	void DragWindow(){
		//DO drag logic
		transform.Translate (new Vector3 (Input.mousePosition.x - m_lastMousePosition.x, Input.mousePosition.y - m_lastMousePosition.y, 0));
	}

	// Test if a window is within a dock range, if so Dock and resize scroll area appropriately
	void TestForDock(){
		dockClass dockTest = m_windowController.GetComponent<WindowController> ().CheckDocks (new Vector2 (m_dragBarCombinedDimensions.position.x + (m_dragBarCombinedDimensions.width / 2), m_dragBarCombinedDimensions.position.y + (m_dragBarCombinedDimensions.height / 2)));

		// Valid rect, possible dock in range - snap to
		if (dockTest.m_dockDimension.x != -1) {
			float xDrift = 0;
			float yDrift = 0;


			// BEFORE ACTUAL DOCK OCCURS
			if (dockTest.m_dockDimension.x == 0) {
				// Docked right or left, use vertical scroll bar
				m_horizontallyDocked = true;

				m_myTransform.sizeDelta = new Vector2 (m_initialWindowDimensions.x, dockTest.m_dockDimension.y);

				//vertical, check if bottom
				xDrift = dockTest.m_dockObject.DockDirection.x;

			} else {
				// Docked top or bot, use horizontal scroll bar
				m_horizontallyDocked = false;

				m_myTransform.sizeDelta = new Vector2 (dockTest.m_dockDimension.x, m_initialWindowDimensions.x + m_dragBarCombinedDimensions.height);

				//horizontal, check if right
				yDrift = dockTest.m_dockObject.DockDirection.y;
			}

			transform.position = new Vector3(dockTest.m_dockCentre.x + (xDrift * (m_myTransform.rect.width/2)), dockTest.m_dockCentre.y + (yDrift * (m_myTransform.rect.height/2)), 0);

			m_dragStartVector = Input.mousePosition;

			m_currentState = WindowState.Docked;

			ResizeScrollArea ();
			ScrollOffset (m_horizontallyDocked);
		}
	}

    // Update is called once per frame
    void Update () {
		// Juggle the scroll bars
		if (m_currentState == WindowState.Docked) {
			if (m_horizontallyDocked) {
				m_verticalScrollBar.SetActive (true);
				m_horizontalScollBar.SetActive (false);
			} else {
				m_verticalScrollBar.SetActive (false);
				m_horizontalScollBar.SetActive (true);
			}
		}

        // Check if screen has been resized
        if (Screen.width != m_lastScreenRes.x || Screen.height != m_lastScreenRes.y)
        {
            UpdateRecPositions();
        }

        // I have clicked the top bar
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x > m_dragBarCombinedDimensions.x && 
                 Input.mousePosition.x < m_dragBarCombinedDimensions.x + m_dragBarCombinedDimensions.width &&
                 Input.mousePosition.y > m_dragBarCombinedDimensions.y &&
                 Input.mousePosition.y < m_dragBarCombinedDimensions.y + m_dragBarCombinedDimensions.height)
             {
                if (m_windowController.GetComponent<WindowController>().CheckIfCanBeDragged(m_windowID))
                {
                    m_beingDragged = true;
                }
             }
        }

        // I might have released the window
        if (Input.GetMouseButtonUp(0))
        {
            m_dragCached = false;
            m_beingDragged = false;
        }

        // Window has qualified for dragging
        if (m_beingDragged)
        {
            // Cache the initial mouse down (for dock/minimized distance pulling)
            if (!m_dragCached)
            {
                m_dragStartVector = Input.mousePosition;
                m_dragCached = true;
            }

            switch (m_currentState)
            {
			case WindowState.Free:
				{
					DragWindow ();
					TestForDock ();

					UpdateRecPositions ();
				}
                    break;
			case WindowState.Docked:
				{
					BreakWindowFreeze (150);
				}
                    break;
			case WindowState.Minimized:
				{
					BreakWindowFreeze (100);
				}
                    break;
            }
        }

        m_lastMousePosition = Input.mousePosition;
        m_lastScreenRes = new Vector2(Screen.width, Screen.height);
    }


	//--------------BUTTON FUNCTIONS CALLED THROUGH THE CANVAS EVENT SYSTEM--------------

	public void CloseButton()
	{
		Destroy(gameObject);
		m_windowController.GetComponent<WindowController>().CloseWindow(m_windowID);
	}

	public void MinimizeButton()
	{
		// Resize the panel to avoid edge safety cap
		m_myTransform.sizeDelta = new Vector2(m_windowControllerScriptCache.m_minimizeWidth, m_dragBarCombinedDimensions.height);

		// Move to minimize list
		m_myTransform.position = new Vector3(
			(((m_windowControllerScriptCache.m_minimizeDisplayPixelBuffer *2) + m_windowControllerScriptCache.m_minimizeWidth /2) + (m_windowController.GetComponent<WindowController>().getNextMinimizedSlot(m_windowID) * (m_windowControllerScriptCache.m_minimizeWidth))) + m_windowControllerScriptCache.m_minimizeDisplayPixelBuffer, m_windowControllerScriptCache.m_minimizeDisplayPixelBuffer *5, 0);


		m_contentPanel.SetActive(false);

		UpdateRecPositions();

		m_currentState = WindowState.Minimized; //getNextMinimizedSlot
	}
}
