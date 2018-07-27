using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSettings : MonoBehaviour {

	void SelectedLayer(){
		foreach (Transform child in transform) {
			child.gameObject.SendMessage("ReGetAttributeValue");
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
