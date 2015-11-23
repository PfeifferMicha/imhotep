using UnityEngine;
using System.Collections;


/*
This script moves the object it is attached to, over the object which has the layer 8 (MousePlane)
    */
public class MouseSphereMovement : MonoBehaviour {

    public float speed = 10f;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {



        if ( (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && !Input.GetMouseButton(2))
        {

            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, transform.localPosition - Camera.main.transform.position + new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0));
            LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMousePlane))
            {
                Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
                transform.position = hit.point;
            }
           
        }
    }
}
