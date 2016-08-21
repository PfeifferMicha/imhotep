using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {

    public float speed = 5f;

    private Vector3 cameraStartPosition;

	// Use this for initialization
	void Start () {
        cameraStartPosition = Camera.main.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }

            Vector3 movement = Camera.main.transform.forward * input.y + Camera.main.transform.right * input.x;

            //if left shift is pressed, movement is 3 times faster
            float shift = Input.GetKey(KeyCode.LeftShift) ? 3f : 1f;

            movement = movement * speed * shift;            

            transform.localPosition = transform.localPosition + movement;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Camera.main.transform.position = cameraStartPosition;
        }

    }
}
