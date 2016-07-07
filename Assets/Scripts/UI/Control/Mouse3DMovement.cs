using UnityEngine;
using System.Collections.Generic;


/*
This script moves the object it is attached to, over the object which has the layer 8 (MousePlane)
    */
public class Mouse3DMovement : MonoBehaviour {

	public GameObject owner; //Defines with gameobject controlls the 3D mouse

	public List<GameObject> availibleControllers = new List<GameObject> ();

	private Vector2 mCurrentUVCoordinates;
	private GameObject mouse;

    public float mouseSpeed = 0.03f;

	private MeshRenderer mRenderer;

    // Use this for initialization
    void Start () {
		mouse = new GameObject ("mouse"); //the real mouse has no gmaeobject to add to the list, so we create a fake mouse object
		availibleControllers.Add(mouse);
		mRenderer = transform.GetComponent<MeshRenderer> ();
		owner = mouse;
	}

	// Update is called once per frame
	void Update () {			
        if ( (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && !Input.GetMouseButton(2))
        {
            RaycastHit hit;
            Vector3 dir = transform.localPosition - Camera.main.transform.position + new Vector3(Input.GetAxis("Mouse X") * mouseSpeed, Input.GetAxis("Mouse Y") * mouseSpeed, 0);
            Ray ray = new Ray(Camera.main.transform.position, dir);
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMousePlane))
            {
                //Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
                transform.position = hit.point;

                // Remember my UV coordinates, because the MouseUIInteraction script will use them to handle UI input:
                this.setUVCoordinates(hit.textureCoord2, mouse);

                mRenderer.enabled = true;
            }
            else
            {
                mRenderer.enabled = false;
            }         
                
        }
    }

	public Vector2 getUVCoordinates()
	{
		return mCurrentUVCoordinates;
	}

	public void setVisible( bool vis )
	{
		mRenderer.enabled = vis;
	}

	//Controller defines with game object has set the uv coordinates
	public void setUVCoordinates(Vector2 v, GameObject controller)
	{
		mCurrentUVCoordinates = v;
		owner = controller;
	}
}
