using UnityEngine;
using System.Collections;


/*
This script moves the object it is attached to, over the object which has the layer 8 (MousePlane)
    */
public class MouseSphereMovement : MonoBehaviour {

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        if ( (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && !Input.GetMouseButton(2))
        {
            RaycastHit hit;
			//Vector3 dir = transform.localPosition - Camera.main.transform.position + new Vector3(Input.GetAxis("Mouse X") * scale, Input.GetAxis("Mouse Y") * scale, 0);
			//Ray ray = new Ray(Camera.main.transform.position, dir);
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

			Debug.Log (new Vector3 (Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"), 0));

			if (Physics.Raycast (ray, out hit, Mathf.Infinity, onlyMousePlane)) {
				//Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
				transform.position = hit.point;
				Debug.DrawRay(Camera.main.transform.position, dir, Color.red );
			} else {
				Debug.DrawRay(Camera.main.transform.position, dir, Color.green );
			}

           
        }
    }
}
