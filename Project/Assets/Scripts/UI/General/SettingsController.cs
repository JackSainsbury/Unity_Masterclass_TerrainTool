using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour {

	public bool m_isNewSettingsFile { get; set; }

	public string m_settingsPath = "Assets/Resources/";
	public string m_settingsFileName = "Preferences";


	// Load the value from my settings file
	public object LoadValueFromSettings(string name){
		Debug.Log (name);

		return null;
	}

	// Settings file didn't exist, write this attribute to the file
	public void WriteValueToSettings(string name, object value){
		Debug.Log (value);
	}

	// Use this for initialization
	void Start () {

		if (!Directory.Exists (m_settingsPath)) {
			//if it doesn't, create it
			Directory.CreateDirectory (m_settingsPath);
		}

		if (!System.IO.File.Exists (m_settingsPath + m_settingsFileName)) {
			m_isNewSettingsFile = true;

			System.IO.File.WriteAllText (m_settingsPath + m_settingsFileName, "");
		} else {
			m_isNewSettingsFile = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
