using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockScript : MonoBehaviour {
    // Dock lists of CHILD windows in the respective directions
    public List<GameObject> uDocks;
    public List<GameObject> dDocks;
    public List<GameObject> rDocks;
    public List<GameObject> lDocks;

    public Vector2 DockDirection { get; set; }

    // Called to all dock objects when a window is Docked or Un-docked
    public void DockChanged()
    {
        /*foreach(GameObject d in uDocks)
        {

        }
        foreach (GameObject d in dDocks)
        {

        }
        foreach (GameObject d in rDocks)
        {

        }
        foreach (GameObject d in lDocks)
        {

        }*/
    }
}
