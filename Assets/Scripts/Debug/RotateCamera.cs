using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

    public float speed = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(1))
        {
            transform.localRotation = transform.localRotation * Quaternion.Euler(-Input.GetAxis("Mouse Y") * speed, Input.GetAxis("Mouse X") * speed, 0);
        }
    }
}
