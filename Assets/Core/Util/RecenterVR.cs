using UnityEngine;
using System.Collections;

public class RecenterVR : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.VR.InputTracking.Recenter();
        }
    }
}
