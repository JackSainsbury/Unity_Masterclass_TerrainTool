using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatFieldWidget : Widget {

	// the attribute to get from this widget
	public string m_floatAttributeName = "FloatAttribute";

	// For validation fail, reset
	private float m_lastVal;

	public void Start() {
		// Declare and initialize a new attribute
		AddAttribute<float> (m_floatAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<float> (m_floatAttributeName));

		GetComponent<InputField> ().text = GetAttribute<float> (m_floatAttributeName).ToString();

		m_lastVal = GetAttribute<float> (m_floatAttributeName);
	}

	public void ReGetAttributeValue () {
		// Declare and initialize a new attribute
		SetAttribute<float> (m_floatAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<float> (m_floatAttributeName));
		GetComponent<InputField> ().text = GetAttribute<float> (m_floatAttributeName).ToString();

		m_lastVal = GetAttribute<float> (m_floatAttributeName);
	}

	// From the canvas input field trigger, set the attribute to tex val (integrating function) called from "on text commit"
	public void SetAttributeValue(){

		float val;

		// try to parse the input, if it cannot be converted to a float, reset the field to the last cached parsable value
		if (float.TryParse (GetComponent<InputField> ().text, out val)) {
			if (val != m_lastVal) {
				
				SetAttribute<float> (m_floatAttributeName, val);

				GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().SetAttribute<float> (m_floatAttributeName, val);

				AllMethodMessages ();

				m_lastVal = val;
			}
		} else {
			GetComponent<InputField>().text = m_lastVal.ToString();
		}
	}
}
