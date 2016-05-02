using UnityEngine;
using System.Collections;

public class CanvasOrientation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 vectorToCamera = Camera.main.transform.position - this.transform.position;
        this.transform.LookAt(Camera.main.transform);
    }
}
