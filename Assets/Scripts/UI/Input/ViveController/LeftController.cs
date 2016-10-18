using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LeftController : Controller {

	public float rotationSpeed = 200.0f;
	public GameObject rotationNode;
	public GameObject scalingNode;

	private Vector3 previousPos;

	private bool rotating = false;
	private bool zooming = false;
	private Vector3 mOriginalZoom;
	private float originalDist;

	// Use this for initialization
	void Start () {
		InputDeviceManager.instance.registerLeftController (this);
	}

	public void shake( ushort milliseconds )
	{
		SteamVR_Controller.Input( (int)controllerIndex ).TriggerHapticPulse( milliseconds );
	}

	public Vector2 getScrollDelta()
	{
		return touchpadDelta*100;
	}

	void Update()
	{
		base.Update();
		UpdateTouchpad ();

		PointerEventData.FramePressState triggerState = UpdateTriggerState ();

		// -----------------------------------------
		// Rotation:
		if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed) {
			rotating = true;
			previousPos = this.transform.localPosition;

		} else if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Released) {
			rotating = false;
			previousPos = new Vector3 (0, 0, 0);
		}
		if (rotating) {
			Vector3 upVector = Camera.main.transform.up;
			Vector3 rightVector = Camera.main.transform.right;
			rotationNode.transform.RotateAround(rotationNode.transform.position, upVector, (previousPos.x - this.transform.localPosition.x)*rotationSpeed);
			rotationNode.transform.RotateAround(rotationNode.transform.position, rightVector, -(previousPos.y - this.transform.localPosition.y)*rotationSpeed );
			rotationNode.GetComponent<ModelRotator>().setTargetOrientation( rotationNode.transform.localRotation );	// Make sure it doesn't auto-rotate back.	// TODO: fix?
			previousPos = this.transform.localPosition;
		}

		if ( triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed && zooming == false) {
			zooming = true;
			originalDist = (this.transform.position - scalingNode.transform.position).magnitude;
			mOriginalZoom = scalingNode.transform.localScale;
		} else if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Released && zooming == true) {
			zooming = false;
		}

		if (zooming) {

			float dist = (this.transform.position - scalingNode.transform.position).magnitude;

			float distDiff = dist - originalDist;

			Vector3 newScale = mOriginalZoom + mOriginalZoom * distDiff;

			scalingNode.GetComponent<ModelZoomer>().setTargetZoom( newScale );	// Make sure it doesn't auto-rotate back.	// TODO: fix?
		}

		// -----------------------------------------
		// Scaling:
	}
}
