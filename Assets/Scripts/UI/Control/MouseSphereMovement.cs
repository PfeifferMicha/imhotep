using UnityEngine;
using System.Collections;

public class MouseSphereMovement : MonoBehaviour {

    public Transform mousePlane;
    public float mouseSensitivity = 1;
    private Vector2 position;
    private Vector2 max;

    // Use this for initialization
    void Start () {
        transform.position = mousePlane.transform.position;
        Vector3 size = mousePlane.GetComponent<Renderer>().bounds.size;

        position.x = 0;
        position.y = 0;
        max.x = size.x / 2;
        max.y = size.y / 2;
    }
	
	// Update is called once per frame
	void Update () {
        if (!Input.GetMouseButton(2))
        {
            
            position.x += Input.GetAxis("Mouse X") * mouseSensitivity;
            position.x = position.x > max.x ? max.x : position.x;
            position.x = position.x < -max.x ? -max.x : position.x;
            position.y += Input.GetAxis("Mouse Y") * mouseSensitivity;
            position.y = position.y > max.y ? max.y : position.y;
            position.y = position.y < -max.y ? -max.y : position.y;

            transform.position = new Vector3(mousePlane.transform.position.x + position.x, mousePlane.transform.position.y + position.y, mousePlane.transform.position.z);
        }
    }
}
