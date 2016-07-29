using UnityEngine;
using System.Collections;

public class ViveControllerModelZoomer : MonoBehaviour
{

	public GameObject meshNode;

	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index); } }

	private SteamVR_TrackedObject trackedObj;

	public float zoomingSpeed = 1;
	public float maxZoom = 2f;
	public float minZoom = 0.2f;

	//private float mTargetZoom;
	private Vector3 mOriginalZoom;
	private float originalDist;
	//private Vector2 previousAxis;
	//private bool fingerOnTouchpad = false;
	private bool triggerPressed = false;

	// Use this for initialization
	void Start ()
	{
		//previousAxis = new Vector2 (0.0f, 0.0f);
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
	}
	
	// Update is called once per frame
	void Update ()
	{			
		LeftButtonState lbs = this.GetComponent<LeftButtonState> ();
		if (lbs != null &&
			lbs.getLeftButtonState () == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed &&
			triggerPressed == false) {
			triggerPressed = true;
			originalDist = (this.transform.position - meshNode.transform.position).magnitude;
			mOriginalZoom = meshNode.transform.localScale;
		}else if (lbs.getLeftButtonState () == UnityEngine.EventSystems.PointerEventData.FramePressState.Released &&
			triggerPressed == true) {
			triggerPressed = false;
		}

		if (triggerPressed) {

			float dist = (this.transform.position - meshNode.transform.position).magnitude;

			float distDiff = dist - originalDist;

			Vector3 newScale = mOriginalZoom + mOriginalZoom * distDiff;

			meshNode.GetComponent<ModelZoomer>().setTargetZoom( newScale );	// Make sure it doesn't auto-rotate back.	// TODO: fix?
		}

		/*Vector2 axis = controller.GetAxis ();
		if (controller.GetTouchDown (Valve.VR.EVRButtonId.k_EButton_Axis0)) {
			;
			fingerOnTouchpad = true;
			previousAxis = axis;
		}
		if (controller.GetTouchUp (Valve.VR.EVRButtonId.k_EButton_Axis0)) {
			fingerOnTouchpad = false;
			previousAxis = new Vector2 (0.0f, 0.0f);
		}

		if (fingerOnTouchpad) {
			
			float inputScroll = axis.y - previousAxis.y;
				
			mOriginalZoom = meshNode.transform.localScale.x;
				
			mTargetZoom = mOriginalZoom + inputScroll / (1 / zoomingSpeed);
				
			if (mTargetZoom > maxZoom) {
				mTargetZoom = maxZoom;
			}
			if (mTargetZoom < minZoom) {
				mTargetZoom = minZoom;
			}				
			//meshNode.transform.localScale = new Vector3 (mTargetZoom, mTargetZoom, mTargetZoom);
			meshNode.GetComponent<ModelZoomer> ().setTargetZoom (new Vector3 (mTargetZoom, mTargetZoom, mTargetZoom));
			previousAxis = axis;

		}*/


	}
}
