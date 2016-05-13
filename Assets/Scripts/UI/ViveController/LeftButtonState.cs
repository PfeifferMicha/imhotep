using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LeftButtonState : MonoBehaviour {

	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	private PointerEventData.FramePressState leftButtonState;
	private bool triggerPressedDown = false; //True of the trigger is pressed down

	// Use this for initialization
	void Start () {
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
		leftButtonState = PointerEventData.FramePressState.NotChanged;
	}
	
	// Update is called once per frame
	void Update () {
		if (controller == null) {
			return;
		}
			
		switch (leftButtonState) {
		case PointerEventData.FramePressState.NotChanged:
			if (triggerPressed () && !triggerPressedDown) {
				leftButtonState = PointerEventData.FramePressState.Pressed;
			}
			if (!triggerPressed () && triggerPressedDown) {
				leftButtonState = PointerEventData.FramePressState.Released;
			}
			break;

		case PointerEventData.FramePressState.Pressed:
			triggerPressedDown = true;
			//TODO improve clicking 
			if (triggerPressed () && triggerPressedDown) {
				leftButtonState = PointerEventData.FramePressState.NotChanged;
			}
			break;

		//case PointerEventData.FramePressState.PressedAndReleased:
			//break;

		case PointerEventData.FramePressState.Released:
			if (!triggerPressed ()) {
				leftButtonState = PointerEventData.FramePressState.NotChanged;
			} else {
				leftButtonState = PointerEventData.FramePressState.Pressed;
			}
			triggerPressedDown = false;
			break;			
		}


		//Detect trigger
		/*if (controller != null) {
			if (controller.GetAxis (triggerButton) == new Vector2 (1.0f, 0.0f)) {
				Debug.Log ("Trigger compelete pressed");
			}
			//Debug.Log("Get axis" + controller.GetAxis (triggerButton));



		} else {
			Debug.LogWarning ("Controller is null");
		}*/
	}

	private bool triggerPressed(){
		//Checks if the trigger is pressed down till it clicks
		//Returns true as long as the trigger is pressed down
		if (controller.GetAxis (triggerButton) == new Vector2 (1.0f, 0.0f)) {
			return true;
			//Debug.Log ("Trigger compelete pressed");
		}
		return false;
	}

	public PointerEventData.FramePressState getLeftButtonState(){
		return leftButtonState;
	}
}
