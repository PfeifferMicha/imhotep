using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Controller : MonoBehaviour {

	//--------------- controller stuff---------------------
	private SteamVR_Controller.Device controller {
		get{
			if( trackedObj == null )
				trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
			if (trackedObj == null)
				return null;
			return SteamVR_Controller.Input ((int)trackedObj.index);
		}
	}
	private SteamVR_TrackedObject trackedObj = null;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	public SteamVR_TrackedObject.EIndex controllerIndex {
		get {
			if (trackedObj != null) {
				return trackedObj.index;
			} else {
				return 0;
			}
		}
	}

	//private PointerEventData.FramePressState leftButtonState;
	protected bool triggerPressedDown = false; //True if the trigger is pressed down
	protected bool helpState = false; //Before releasing the button press it again to avoid scrolling in lists

	protected Vector2 previousTouchpad = Vector2.zero;
	protected Vector2 touchpadValue = Vector2.zero;
	protected Vector2 touchpadDelta = Vector2.zero;

	protected PointerEventData.FramePressState triggerButtonState = PointerEventData.FramePressState.NotChanged;
	//-----------------------------------------------------

	//! The movement of the controller since the previous frame in world space:
	public Vector3 positionDelta { protected set; get; }

	private Vector3 previousPosition;

	// Use this for initialization
	void Start () {
		//trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
		positionDelta = Vector3.zero;
		previousPosition = Vector3.zero;
	}

	public void Update() {
		positionDelta = transform.position - previousPosition;
		previousPosition = transform.position;
	}

	/*! Returns true if the trigger is pressed down all the way. */
	public bool triggerPressed(){
		if( controller == null )
			return false;
		//Checks if the trigger is pressed down till it clicks
		//Returns true as long as the trigger is pressed down
		if (controller.GetAxis (triggerButton) == new Vector2 (1.0f, 0.0f)) {
			return true;
			//Debug.Log ("Trigger compelete pressed");
		}
		return false;
	}

	/*! Returns how far the controller's trigger is pressed (0 to 1) */
	public float triggerValue(){
		if( controller == null )
			return 0f;

		return controller.GetAxis (triggerButton).x;
	}

	protected void UpdateTouchpad() {
		touchpadValue = controller.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
		if( touchpadValue.sqrMagnitude < 0.01f || previousTouchpad.sqrMagnitude < 0.01f )
		{
			touchpadDelta = Vector2.zero;
		} else {
			touchpadDelta = touchpadValue - previousTouchpad;
		}
		previousTouchpad = touchpadValue;
	}

	protected PointerEventData.FramePressState UpdateTriggerState() {

		switch (triggerButtonState) {
		case PointerEventData.FramePressState.NotChanged:
			if (triggerPressed () && !triggerPressedDown) {
				triggerButtonState = PointerEventData.FramePressState.Pressed;
			}
			if (!triggerPressed () && triggerPressedDown) {
				if (helpState == false) {
					triggerButtonState = PointerEventData.FramePressState.Pressed;
					helpState = true;
				} else {
					triggerButtonState = PointerEventData.FramePressState.Released;
				}
			}
			break;

		case PointerEventData.FramePressState.Pressed:
			if (helpState) {
				helpState = false;
				triggerButtonState = PointerEventData.FramePressState.Released;
			} else {
				triggerPressedDown = true;
				if (triggerPressed () && triggerPressedDown) {
					triggerButtonState = PointerEventData.FramePressState.NotChanged;
				} else if (!triggerPressed () && triggerPressedDown) {
					triggerButtonState = PointerEventData.FramePressState.Released;
				}
			}
			break;

			//case PointerEventData.FramePressState.PressedAndReleased:
			//break;

		case PointerEventData.FramePressState.Released:
			if (!triggerPressed ()) {
				triggerButtonState = PointerEventData.FramePressState.NotChanged;
			} else {
				triggerButtonState = PointerEventData.FramePressState.Pressed;
			}
			triggerPressedDown = false;
			break;			
		}

		return triggerButtonState;
	}


	public void set3DDelta( Vector2 delta ) {
		positionDelta = delta;
	}
}
