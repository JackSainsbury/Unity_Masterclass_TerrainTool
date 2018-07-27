using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetToggleLabelText : MonoBehaviour {

	public WindowInitializer m_initializer;

	// Use this for initialization
	void Start () {
		GetComponent<Text> ().text = m_initializer.m_windowTitle;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
