using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientWidget : Widget {
	public string m_gradientAttributeName = "GradientAttribute";

	public RawImage m_gradientContainer;

	public GameObject m_keyReference;

	public float m_keyMarkerPixelWidth = 3.0f;

	private List<GameObject> m_keys;

	private bool gradBeingClicked = false;

	private bool keyHeld = false;

	int selectedKey = 0;

	private float initX = 0;
	private float initY = 0;


	private float initKeyX = 0;
	private float initKeyY = 0;

	// Use this for initialization
	void Start () {
		m_keys = new List<GameObject> ();

		Gradient defaultGrad = GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<Gradient> (m_gradientAttributeName);

		AddAttribute<Gradient> (m_gradientAttributeName, defaultGrad);

		DrawGradient ();

	
		// Add the default keys
		foreach (GradientAlphaKey key in defaultGrad.alphaKeys) {
			NewKeyMarker (key.time);
		}
	}

	private void NewKeyMarker(float createTime){
		// Create a new marker object
		GameObject newKeyMarker = Instantiate (m_keyReference, transform.position, Quaternion.identity) as GameObject;

		newKeyMarker.transform.position = new Vector3((m_gradientContainer.transform.position.x - (m_gradientContainer.GetComponent<RectTransform>().rect.width / 2)) + (m_gradientContainer.GetComponent<RectTransform>().rect.width * createTime), m_gradientContainer.transform.position.y, 0);

		newKeyMarker.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_keyMarkerPixelWidth, m_gradientContainer.GetComponent<RectTransform>().rect.height + (m_keyMarkerPixelWidth * 2));

		newKeyMarker.transform.SetParent (m_gradientContainer.transform, true);

		int insert = 0;

		for (int i = 0; i < m_keys.Count; ++i) {
			if (m_keys [i].transform.position.x < m_keys [insert].transform.position.x) {
				insert = i;
			}
		}

		selectedKey = insert;

		m_keys.Insert (insert, newKeyMarker);
	}

	private void NewKey(float createTime){
		Gradient gradient = GetAttribute<Gradient> (m_gradientAttributeName);

		float gradientLength = m_gradientContainer.GetComponent<RectTransform> ().rect.width;

		GradientAlphaKey[] aKeys = new GradientAlphaKey[gradient.alphaKeys.Length + 1];

		bool added = false;

		// add existing keys back
		for (int i = 0; i < gradient.alphaKeys.Length; ++i) {
			if (!added) {
				if (gradient.alphaKeys [i].time < createTime) {
					aKeys [i] = gradient.alphaKeys [i];
				} else {
					aKeys [i] = new GradientAlphaKey (gradient.Evaluate (createTime).a, createTime);
					aKeys [i + 1] = gradient.alphaKeys [i];
					added = true;
				}
			} else {
				aKeys [i + 1] = gradient.alphaKeys [i];
			}
		}

		// Set the now updates alpha keys array
		gradient.alphaKeys = aKeys;

		NewKeyMarker (createTime);
	}

	void OrderKeys(){
		m_keys.Sort(delegate(GameObject a, GameObject b) {
			return (a.transform.position.x).CompareTo(b.transform.position.x);
		});
	}

	// Check a click against the gradient
	int CheckKeysAgainstClick(){
		float gradientLength = m_gradientContainer.GetComponent<RectTransform> ().rect.width;

		// The proportion along my gradient (horizontally) I have just clicked
		float gradientX = (Input.mousePosition.x - (m_gradientContainer.transform.position.x - (m_gradientContainer.GetComponent<RectTransform> ().rect.width / 2))) / gradientLength;

		Gradient gradient = GetAttribute<Gradient> (m_gradientAttributeName);

		float SelectDistProportion = .1f;

		int candidate = -1;

		for(int k = 0; k < gradient.alphaKeys.Length; ++ k){
			float dif = Mathf.Abs(gradient.alphaKeys [k].time - gradientX);

			// This key is within range
			if (dif < SelectDistProportion) {
				// Is this the first key I've come to? || is this key closer than another candidate
				if (candidate == -1 || dif < Mathf.Abs(gradient.alphaKeys [candidate].time - gradientX)) {
					candidate = k;
				}
			}
		}

		return candidate;
	}

	public void GradClick(){
		if (!keyHeld) {
			int gradientLength = Mathf.RoundToInt (m_gradientContainer.GetComponent<RectTransform> ().rect.width);

			int validKey = CheckKeysAgainstClick ();

			if (validKey != -1) {
				selectedKey = validKey;
			} else if (GetAttribute<Gradient> (m_gradientAttributeName).alphaKeys.Length < 7) {
				NewKey ((Input.mousePosition.x - (m_gradientContainer.transform.position.x - (m_gradientContainer.GetComponent<RectTransform> ().rect.width / 2))) / gradientLength);
			}
		}
	}

	void DrawGradient(){		
		int gradientLength = Mathf.RoundToInt(m_gradientContainer.GetComponent<RectTransform> ().rect.width);

		Texture2D newTexture = new Texture2D (gradientLength, 1); 

		Gradient gradient = GetAttribute<Gradient> (m_gradientAttributeName);

		for (int i = 0; i < gradientLength; ++i) {
			float alphaVal = gradient.Evaluate ((1/(float)gradientLength) * i).a;

			newTexture.SetPixel (i, 0, new Color(alphaVal, alphaVal, alphaVal));
		}

		newTexture.Apply ();

		m_gradientContainer.texture = newTexture;
	}

	void UpdateKeyPos(){
		float gradientLength = m_gradientContainer.GetComponent<RectTransform>().rect.width;

		m_keys[selectedKey].transform.position = new Vector3(
			Mathf.Clamp(initKeyX + (Input.mousePosition.x - initX), m_gradientContainer.transform.position.x - (gradientLength/2), m_gradientContainer.transform.position.x + (gradientLength/2)), 
			m_gradientContainer.transform.position.y, 
			0);


		float newAlpha = Mathf.Clamp01( initKeyY + ((1/200.0f) * (Input.mousePosition.y - initY)));


		GradientAlphaKey[] newKeys = GetAttribute<Gradient> (m_gradientAttributeName).alphaKeys; 

		newKeys[selectedKey].alpha = newAlpha;
		newKeys[selectedKey].time = (m_keys[selectedKey].transform.position.x - (m_gradientContainer.transform.position.x - (m_gradientContainer.GetComponent<RectTransform> ().rect.width / 2))) / gradientLength;

		GetAttribute<Gradient> (m_gradientAttributeName).SetKeys (GetAttribute<Gradient> (m_gradientAttributeName).colorKeys, newKeys);

		OrderKeys ();
		DrawGradient();
	}

	bool ClickedGradient() {
		if (
			Input.mousePosition.x > m_gradientContainer.transform.position.x - (m_gradientContainer.GetComponent<RectTransform> ().rect.width / 2) &&
			Input.mousePosition.x < m_gradientContainer.transform.position.x + (m_gradientContainer.GetComponent<RectTransform> ().rect.width / 2) &&
			Input.mousePosition.y > m_gradientContainer.transform.position.y - (m_gradientContainer.GetComponent<RectTransform> ().rect.height / 2) &&
			Input.mousePosition.y < m_gradientContainer.transform.position.y + (m_gradientContainer.GetComponent<RectTransform> ().rect.height / 2)) {
			return true;
		}
		return false;
	}

	// Dragging keys on gradient
	void Update(){
		if (Input.GetMouseButtonDown (0)) {
			if (ClickedGradient()) {
				int keyTest = CheckKeysAgainstClick ();

				// my keydown was in range of a key, make selected
				if (keyTest != -1) {
					selectedKey = keyTest;
					initX = Input.mousePosition.x;
					initY = Input.mousePosition.y;

					initKeyY = GetAttribute <Gradient> (m_gradientAttributeName).alphaKeys [selectedKey].alpha;
					initKeyX = m_keys [selectedKey].transform.position.x;

					keyHeld = true;
				} else {
					keyHeld = false;
				}
			}
		}

		// Mouse being held
		if (Input.GetMouseButton (0)) {
			if (ClickedGradient()){
				gradBeingClicked = true;
			}
		}

		// Mouse release, should I add a new key
		if (Input.GetMouseButtonUp (0)) {
			if (ClickedGradient ()) {
				GradClick ();
			}

			gradBeingClicked = false;
			keyHeld = false;
		}

		// Click began on gradient and is held down
		if (gradBeingClicked && keyHeld) {
			UpdateKeyPos ();
		}
	}
}
