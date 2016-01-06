using UnityEngine;
using System.Collections;

/*
    If an element uses this component, you can drag this element and the parent canvas and the parent game objects moves too.
*/
public class DragableUI : MonoBehaviour {

    private bool dragUI = false;

    public float speed = 5f;

    public GameObject mouse;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (dragUI)
        {
            if (Input.GetMouseButton(0))
            {
                if (transform.parent != null && transform.parent.transform.parent != null)
                {
                    Transform grandparent = transform.parent.transform.parent;

                    RaycastHit hit;
                    Ray ray = new Ray(Camera.main.transform.position, mouse.transform.localPosition - Camera.main.transform.position);
                    LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMousePlane))
                    {
                        //Vector3 offset = (mouse.transform.localPosition - Camera.main.transform.position).normalized * 10f;
						grandparent.position = hit.point - offset;

						// Vector from camera to position:
						Vector3 diff = grandparent.position - Camera.main.transform.position;
						diff.Normalize ();
						grandparent.position = Camera.main.transform.position + diff * hit.distance;

						// Trace toward center of widget:
						RaycastHit hitCenter;
						Ray rayCenter = new Ray(Camera.main.transform.position, grandparent.position - Camera.main.transform.position );
						if (Physics.Raycast (rayCenter, out hitCenter, Mathf.Infinity, onlyMousePlane)) {
							grandparent.transform.localRotation = Quaternion.FromToRotation (grandparent.transform.up, hitCenter.normal) * grandparent.transform.localRotation;
						}

                    }
                }
            }
            else
            {
                dragUI = false;
            }
        }
    }

    public void MoveElement()
    {
		if (dragUI == false) {
			offset = new Vector3 (0, 0, 0);
			RaycastHit hit;
			Ray ray = new Ray (Camera.main.transform.position, mouse.transform.localPosition - Camera.main.transform.position);
			LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

			if (Physics.Raycast (ray, out hit, Mathf.Infinity, onlyMousePlane)) {
				Transform grandparent = transform.parent.transform.parent;
				offset = hit.point - grandparent.position;
			}
			dragUI = true;
		}
    }

}
