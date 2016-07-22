using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
/*
    If an element uses this component, you can drag this element and the parent canvas and the parent game objects moves too.
*/
public class DragableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	private bool mDragged = false;

	private Camera UICamera;
	//private MouseSphereMovement mMouse;
	private Vector2 mViewSize;
	private Vector2 mMinPos;
	private Vector2 mMaxPos;
	private Vector3 mOffset;

	private MouseInputModule mouseInputModul;
	private bool leftButtonPressed = false; //True if left mouse button or trigger is pressed

	private Mouse3DMovement mMouse;

	// Use this for initialization
	void Start () {

		//mMouse = GameObject.Find ("Mouse3D").GetComponent<MouseSphereMovement> ();
		UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();
		mViewSize.y = UICamera.orthographicSize * 2;
		mViewSize.x = mViewSize.y * UICamera.aspect;


		mouseInputModul = GameObject.Find ("GlobalScript").GetComponent<MouseInputModule>();
		mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> ();

		// Ensure the canvas is placed around the origin of the widget:
		RectTransform rt = transform.parent.GetComponent<RectTransform> ();
		rt.localPosition = new Vector3( 0,0,0 );
	}

	void Update () {
		if (mDragged)
		{
			updateLeftButtonPressed ();

			if (leftButtonPressed)
			{
				if (transform.parent != null && transform.parent.transform.parent != null)
				{
					Transform grandparent = transform.parent.transform.parent;


					RaycastHit hit;
					//Ray ray = new Ray(Camera.main.transform.position, mouse.transform.localPosition - Camera.main.transform.position);
					//Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
					Ray ray = new Ray(Camera.main.transform.position, mMouse.transform.localPosition - Camera.main.transform.position);

					LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer

					if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMousePlane))
					{
						//Vector3 offset = (mouse.transform.localPosition - Camera.main.transform.position).normalized * 10f;
						Vector2 uv = hit.textureCoord2;
						Vector3 mousePos = new Vector3 (uv.x * mViewSize.x, uv.y * mViewSize.y, 0);
						Vector3 newPos = mousePos - mOffset;

						Vector2 canvasSize = transform.parent.GetComponent<RectTransform> ().rect.size;
						Vector2 canvasPos = transform.parent.GetComponent<RectTransform> ().rect.position;
						Vector2 widgetScale = grandparent.localScale;

						Debug.Log ("Canvas Size: " + canvasSize);
						Vector2 widgetSize = new Vector2 (canvasSize.x * widgetScale.x,
							canvasSize.y * widgetScale.y);

						Debug.Log ("widegtSize: " + widgetSize);

						mMinPos = (-mViewSize + widgetSize) / 2f;
						mMaxPos = (mViewSize - widgetSize) / 2f;

						grandparent.localPosition = new Vector3 (
							Mathf.Clamp (newPos.x, mMinPos.x, mMaxPos.x),
							Mathf.Clamp (newPos.y, mMinPos.y, mMaxPos.y),
							newPos.y );
					}
				}
			}
			else
			{
				mDragged = false;
			}
		}
	}

	private void updateLeftButtonPressed(){
		if (mouseInputModul.framePressStateLeft == PointerEventData.FramePressState.Pressed) {
			leftButtonPressed = true;
		}else if (mouseInputModul.framePressStateLeft == PointerEventData.FramePressState.Released) {
			leftButtonPressed = false;
		}
	}

	// Event which is called when the mouse clicks on this panel:
	public void OnPointerDown(PointerEventData dt) {
		if (mDragged == false) {
			mOffset = new Vector3 (0, 0, 0);
			RaycastHit hit;
			//Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			Ray ray = new Ray(Camera.main.transform.position, mMouse.transform.localPosition - Camera.main.transform.position);
			LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer
			Transform grandparent = transform.parent.transform.parent;

			UI.WidgetControl.HighlightSelectedWidget (grandparent.gameObject);

			if (Physics.Raycast (ray, out hit, Mathf.Infinity, onlyMousePlane)) {

				//Debug.Log ("name:");
				Vector2 uv = hit.textureCoord2;
				Vector3 mousePos = new Vector3 (uv.x * mViewSize.x, uv.y * mViewSize.y, 0);
				mOffset = mousePos - grandparent.localPosition;
			}

			mDragged = true;
		}
	}

	// Event which is called when the mouse is released:
	public void OnPointerUp(PointerEventData dt) {
		//if (mDragged) {
		// Move the panel a few units forward while it's dragged:
		mDragged = false;
		//}
	}

}
