using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatVector2FieldWidget : Widget {

	// the attribute to get from this widget
	public string m_vectorAttributeName;

	public InputField m_componentFieldX;
	public InputField m_componentFieldY;

	// For validation fail, reset
	private Vector2 m_lastVal;

	public void Start() {
		// Declare and initialize a new attribute
		AddAttribute<Vector2> (m_vectorAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<Vector2> (m_vectorAttributeName));

		m_lastVal = GetAttribute<Vector2> (m_vectorAttributeName);

		// Set the default value of each component
		m_componentFieldX.text = m_lastVal.x.ToString ();
		m_componentFieldY.text = m_lastVal.y.ToString ();
	}

	public void ReGetAttributeValue () {
		// Declare and initialize a new attribute
		SetAttribute<Vector2> (m_vectorAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<Vector2> (m_vectorAttributeName));

		m_lastVal = GetAttribute<Vector2> (m_vectorAttributeName);

		// Set the default value of each component
		m_componentFieldX.text = m_lastVal.x.ToString ();
		m_componentFieldY.text = m_lastVal.y.ToString ();
	}

	// From a canvas input field trigger, set the attribute to tex val (integrating function) called from "on text commit"
	public void SetComponentAttributeValue(int component){
		float val;
		// Mod the new component only, leave last component as is
		Vector2 oldVector = GetAttribute<Vector2> (m_vectorAttributeName);

		// try to parse the input, if it cannot be converted to a float, reset the field to the last cached parsable value
		if (float.TryParse ((component == 0) ? m_componentFieldX.text : m_componentFieldY.text , out val)) {
			if (val != ((component == 0) ? oldVector.x : oldVector.y)) {
				SetAttribute<Vector2> (m_vectorAttributeName, new Vector2 ((component == 0) ? val : oldVector.x, (component == 0) ? oldVector.y : val));

				// Connect to socket
				GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().SetAttribute<Vector2> (m_vectorAttributeName, GetAttribute<Vector2> (m_vectorAttributeName));

				AllMethodMessages ();

				// Cache new value
				m_lastVal = GetAttribute<Vector2> (m_vectorAttributeName);
			}
		} else {
			if(component == 0)
			{
				m_componentFieldX.text = m_lastVal.x.ToString ();
			}
			else
			{
				m_componentFieldY.text = m_lastVal.y.ToString ();
			}
		}
	}
}
