using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownWidget : Widget {

	// the attribute to get from this widget
	public string m_selectAttributeName = "SelAttribute";

	// is the drop down menu currently open?
	private bool m_isDropped = false;

	public GameObject m_option;
	public GameObject m_dropPanel;

	public GameObject m_activeOptionText;

	private GameObject[] m_optionsInstances;
		
	// Use this for initialization
	void Start () {
		int typeCount = (int)LayerFunctionType.NUMFUNCTYPE;

		m_optionsInstances = new GameObject[typeCount];

		// Declare and initialize a new attribute
		AddAttribute<LayerFunctionType> (m_selectAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<LayerFunctionType> (m_selectAttributeName));

		m_activeOptionText.GetComponent<Text> ().text = GetAttribute<LayerFunctionType> (m_selectAttributeName).ToString ();

		m_dropPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 (m_dropPanel.transform.localScale.x, (m_option.GetComponent<RectTransform>().rect.height + 3) * typeCount);
		m_dropPanel.transform.position = new Vector3 (transform.position.x, m_option.GetComponent<RectTransform>().rect.height - ((m_dropPanel.GetComponent<RectTransform>().rect.height / 2) + 3)  + 250, 0);

		// Instance the options
		for(int i = 0; i < typeCount; ++ i) {
			m_optionsInstances[i] = Instantiate(m_option, transform.position, Quaternion.identity) as GameObject;

			m_optionsInstances [i].GetComponent<Option> ().m_dropHandler = this.gameObject;
			m_optionsInstances [i].GetComponent<Option> ().m_optionID = i;
			m_optionsInstances [i].GetComponent<Option> ().m_optionText = ((LayerFunctionType)i).ToString();

			m_optionsInstances [i].transform.position = new Vector3(transform.position.x, (m_option.GetComponent<RectTransform> ().rect.height / 2) - (((m_option.GetComponent<RectTransform>().rect.height + 3) * i) + 4) + 250, 0);

			m_optionsInstances [i].transform.SetParent (m_dropPanel.transform, true);
		}
	}

	public void ReGetAttributeValue () {
		// Declare and initialize a new attribute
		SetAttribute<LayerFunctionType> (m_selectAttributeName, GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().GetAttribute<LayerFunctionType> (m_selectAttributeName));
		m_activeOptionText.GetComponent<Text> ().text = GetAttribute<LayerFunctionType> (m_selectAttributeName).ToString ();
	}


	// Selection has been made, process
	public void DropSelected(int index){
		m_activeOptionText.GetComponent<Text> ().text = ((LayerFunctionType)index).ToString ();

		SetAttribute<LayerFunctionType> (m_selectAttributeName, (LayerFunctionType)index);

		GameObject.FindGameObjectWithTag (m_managerTag).GetComponent<Widget> ().SetAttribute<LayerFunctionType> (m_selectAttributeName, (LayerFunctionType)index);

		ToggleDropDown ();

		AllMethodMessages ();
	}


	// Toggle the dropped state of the menu
	public void ToggleDropDown(){
		if (m_isDropped) {
			m_isDropped = false;
			m_dropPanel.SetActive (m_isDropped);
		} else {
			m_isDropped = true;
			m_dropPanel.SetActive (m_isDropped);
		}
	}
}
