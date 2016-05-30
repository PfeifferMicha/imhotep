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

	private float mTargetZoom;
	private float mOriginalZoom;
	private Vector2 previousAxis;
	private bool fingerOnTouchpad = false;

	// Use this for initialization
	void Start ()
	{
		previousAxis = new Vector2 (0.0f, 0.0f);
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
	}
	
	// Update is called once per frame
	void Update ()
	{			
		Vector2 axis = controller.GetAxis ();
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
			meshNode.transform.localScale = new Vector3 (mTargetZoom, mTargetZoom, mTargetZoom);				
			previousAxis = axis;

		}

		/*
			
*/


	}
}
