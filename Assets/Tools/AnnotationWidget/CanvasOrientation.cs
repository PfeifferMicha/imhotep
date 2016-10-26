using UnityEngine;
using System.Collections;

public class CanvasOrientation : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        //Vector3 vectorToCamera = Camera.main.transform.position - this.transform.position;
        this.transform.LookAt(Camera.main.transform);
		this.transform.Rotate (new Vector3 (0f, 180f, 0f));
    }
}
