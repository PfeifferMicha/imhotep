using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

    public float speed = 1f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (!UI.Core.instance.pointerIsOverUIObject) {
			if (Input.GetMouseButton (2)) {
				transform.localRotation = transform.localRotation * Quaternion.Euler (-Input.GetAxis ("Mouse Y") * speed, Input.GetAxis ("Mouse X") * speed, 0);
				//transform.RotateAround( Vector3.zero, Vector3.up, -Input.GetAxis("Mouse X") * speed );
				//transform.RotateAround( Vector3.zero, Vector3.right, -Input.GetAxis("Mouse Y") * speed );
			}
		}
    }
}
