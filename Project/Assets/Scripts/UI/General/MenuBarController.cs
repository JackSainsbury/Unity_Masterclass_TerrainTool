using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuBarController : MonoBehaviour {

	public GameObject[] m_dropMenus;

    public void OnPressMenuButton(){
		for (int i = 0; i < m_dropMenus.Length; ++i)
        {
			m_dropMenus [i].GetComponent<DropMenuScript> ().SetMenuVisibility (false);

        }
    }
}
