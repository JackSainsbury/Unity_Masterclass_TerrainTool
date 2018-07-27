using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour {

	public GameObject m_dropHandler;
	public GameObject m_label;

	public string m_optionText;

	public int m_optionID;

	void Start(){
		m_label.GetComponent<Text> ().text = m_optionText;
	}

	public void SelectDrop(){
		m_dropHandler.GetComponent<DropDownWidget> ().DropSelected (m_optionID);
	}
}
