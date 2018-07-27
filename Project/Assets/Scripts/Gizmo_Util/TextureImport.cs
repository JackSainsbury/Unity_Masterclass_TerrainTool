
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class TextureImport : MonoBehaviour {

	WWW www;


	// Manager tag is used to direct the now loaded texture to the appropriate script object
	IEnumerator doWWW(string url, string managerTag)
	{
		www = new WWW(url);
		yield return www;


		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.Log(www.error);
		}
		else
		{
			GameObject.FindGameObjectWithTag (managerTag).GetComponent<ToolManager> ().m_loadedTexture = www.texture;
		}
	}

	// Use this for initialization
	public IEnumerator ImportFile(string filePath, string managerTag) {
		//VERY IMPORTANT OR WWW FREAKS \n character doesn't show in debug logs either
		filePath = (filePath.Replace('\\', '/')).Replace("\r", "").Replace("\n", "");

		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
			filePath = "file:///" + filePath;
		}

		IEnumerator co = doWWW(filePath, managerTag); // create an IEnumerator object;

		yield return StartCoroutine(co);
	}
}