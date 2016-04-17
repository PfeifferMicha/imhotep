using UnityEngine;
using System.Collections;

/*
    If an element uses this component, you can drag this element and the parent canvas and the parent game objects moves too.
*/
public class DragableUI : MonoBehaviour {

    private bool mDragged = false;

	private Camera UICamera;
	//private MouseSphereMovement mMouse;
	private Vector2 mViewSize;
	private Vector3 mOffset;

	// Use this for initialization
	void Start () {

		//mMouse = GameObject.Find ("Mouse3D").GetComponent<MouseSphereMovement> ();
		UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();
		mViewSize.y = UICamera.orthographicSize * 2;
		mViewSize.x = mViewSize.y * UICamera.aspect;
	}
	
	// Update is called once per frame
	void Update () {
		if (mDragged)
        {
            if (Input.GetMouseButton(0))
            {
                if (transform.parent != null && transform.parent.transform.parent != null)
                {
                    Transform grandparent = transform.parent.transform.parent;

					RaycastHit hit;
					//Ray ray = new Ray(Camera.main.transform.position, mouse.transform.localPosition - Camera.main.transform.position);
					Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                    LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMousePlane))
                    {
						//Vector3 offset = (mouse.transform.localPosition - Camera.main.transform.position).normalized * 10f;
						Vector2 uv = hit.textureCoord2;
						Vector3 mousePos = new Vector3 (uv.x * mViewSize.x, uv.y * mViewSize.y, 0);
						grandparent.localPosition = mousePos - mOffset;
					}
                }
            }
            else
            {
				mDragged = false;
            }
        }
    }

    public void MoveElement()
    {
		if (mDragged == false) {
			mOffset = new Vector3 (0, 0, 0);
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

			if (Physics.Raycast (ray, out hit, Mathf.Infinity, onlyMousePlane)) {
				Transform grandparent = transform.parent.transform.parent;

				//Debug.Log ("name:");
				Vector2 uv = hit.textureCoord2;
				Vector3 mousePos = new Vector3 (uv.x * mViewSize.x, uv.y * mViewSize.y, 0);
				mOffset = mousePos - grandparent.localPosition;
			}
			mDragged = true;
		}
    }

}
