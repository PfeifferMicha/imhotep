using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class ViveControllerInputDevice : MonoBehaviour, InputDevice {

	//--------------- controller stuff---------------------
	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	//private PointerEventData.FramePressState leftButtonState;
	private bool triggerPressedDown = false; //True if the trigger is pressed down

	private bool helpState = false; //Before releasing the button press it again to avoid scrolling in lists
	//-----------------------------------------------------

	//private PointerEventData.FramePressState leftButtonState = PointerEventData.FramePressState.NotChanged;

	private bool visualizeRay = true;

	private LineRenderer lineRenderer;

	private bool previousRayHitSomething;
	private Vector2 texCoordPrevious;
	private Vector2 texCoordDelta;
	private Vector3 positionPrevious;
	private Vector3 positionDelta;
	private Vector2 previousTouchpad = Vector2.zero;
	private Vector2 touchpadValue = Vector2.zero;
	private Vector2 touchpadDelta = Vector2.zero;

	private ButtonInfo buttonInfo = new ButtonInfo();
	private Camera fakeCamera;

	public void activateVisualization()
	{
		visualizeRay = true;
	}

	public void deactivateVisualization()
	{
		visualizeRay = false;
		Vector3 zero = new Vector3(0, 0, 0);
		lineRenderer.SetPosition(0, zero);
		lineRenderer.SetPosition(1, zero);
	}

	public Ray createRay()
	{
		Ray ray;
		ray = new Ray(this.gameObject.transform.position, this.gameObject.transform.forward);
		return ray;
	}

	public PointerEventData.FramePressState getMiddleButtonState()
	{
		return PointerEventData.FramePressState.NotChanged; //TODO?
	}

	public PointerEventData.FramePressState getRightButtonState()
	{
		return PointerEventData.FramePressState.NotChanged; //TODO?
	}

	public Vector2 getScrollDelta()
	{
		return touchpadDelta*100;
	}

	public Vector2 getTexCoordMovement()
	{
		return texCoordDelta;
	}
	public Vector3 getMovement()
	{
		return positionDelta;
	}

	public bool isVisualizerActive()
	{
		return visualizeRay;
	}

	// Use this for initialization
	void Start () {
		//initialization
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();

		//register device

		if (InputDeviceManager.instance != null)
		{
			InputDeviceManager.instance.registerInputDevice(this);
			Debug.Log("Vive controller registered");
		}

		fakeCamera = gameObject.AddComponent<Camera> () as Camera;
		fakeCamera.enabled = false;
		//find line renderer
		/*lineRenderer = this.GetComponent<LineRenderer>();
		if (lineRenderer == null)
		{
			Debug.LogError("[MouseInput.cs] Line renderer not set");
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


	public ButtonInfo updateButtonInfo ()
	{
		if (controller == null) {
			return buttonInfo;
		}

		touchpadValue = controller.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
		touchpadDelta = touchpadValue - previousTouchpad;
		previousTouchpad = touchpadValue;

		switch (buttonInfo.buttonStates[ButtonType.Trigger]) {
		case PointerEventData.FramePressState.NotChanged:
			if (triggerPressed () && !triggerPressedDown) {
				buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Pressed;
			}
			if (!triggerPressed () && triggerPressedDown) {
				if (helpState == false) {
					buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Pressed;
					helpState = true;
				} else {
					buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Released;
				}
			}
			break;

		case PointerEventData.FramePressState.Pressed:
			if (helpState) {
				helpState = false;
				buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Released;
			} else {
				triggerPressedDown = true;
				if (triggerPressed () && triggerPressedDown) {
					buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.NotChanged;
				} else if (!triggerPressed () && triggerPressedDown) {
					buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Released;
				}
			}
			break;

			//case PointerEventData.FramePressState.PressedAndReleased:
			//break;

		case PointerEventData.FramePressState.Released:
			if (!triggerPressed ()) {
				buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.NotChanged;
			} else {
				buttonInfo.buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.Pressed;
			}
			triggerPressedDown = false;
			break;			
		}

		return buttonInfo;
	}

	public Camera getEventCamera() {
		return fakeCamera;
	}
}
