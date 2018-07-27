using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntFieldWidget : Widget {

	// the attribute to get from this widget
	public string m_intAttributeName = "IntAttribute";

	// For validation fail, reset
	private int m_lastVal;

	public void Start() {
		// Declare and initialize a new attribute
		AddAttribute<int> (m_intAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<int> (m_intAttributeName));

		GetComponent<InputField> ().text = GetAttribute<int> (m_intAttributeName).ToString();
	}

	// From the canvas input field trigger, set the attribute to tex val (integrating function) called from "on text commit"
	public void SetAttributeValue(){
		int val;

		// try to parse the input, if it cannot be converted to a float, reset the field to the last cached parsable value
		if (int.TryParse (GetComponent<InputField> ().text, out val)) {
			if (val != m_lastVal) {
				SetAttribute<int> (m_intAttributeName, val);

				GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().SetAttribute<int> (m_intAttributeName, val);

				AllMethodMessages ();

				m_lastVal = val;
			}
		} else {
			GetComponent<InputField> ().text = m_lastVal.ToString ();
		}
	}
}
