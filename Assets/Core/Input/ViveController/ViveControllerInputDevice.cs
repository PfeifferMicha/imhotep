using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class ViveControllerInputDevice : Controller, InputDevice {

	public InputDeviceManager.InputDeviceType getDeviceType ()
	{
		return InputDeviceManager.InputDeviceType.ViveController;
	}

	private Vector2 texCoordDelta;

	private ButtonInfo buttonInfo = new ButtonInfo();
	private Camera fakeCamera;

	public Ray createRay()
	{
		Ray ray;
		ray = new Ray(this.gameObject.transform.position, this.gameObject.transform.forward);
		return ray;
	}

	public PointerEventData.FramePressState getMiddleButtonState()
	{
		// Return "not changed", because we don't have a middle button.
		return PointerEventData.FramePressState.NotChanged;
	}

	public PointerEventData.FramePressState getRightButtonState()
	{
		// Return "not changed", because we don't have a right button.
		return PointerEventData.FramePressState.NotChanged;
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

	// Use this for initialization
	new public void Start () {
		base.Start ();

		//register device
		if (InputDeviceManager.instance != null)
		{
			InputDeviceManager.instance.registerInputDevice(this);
			Debug.Log("Vive controller registered");
		}

		fakeCamera = gameObject.AddComponent<Camera> () as Camera;
		fakeCamera.enabled = false;
		fakeCamera.nearClipPlane = 0.0001f;
	}

	public ButtonInfo updateButtonInfo ()
	{
		buttonInfo.buttonStates [ButtonType.Trigger] = triggerButtonState;

		return buttonInfo;
	}


	public Camera getEventCamera() {
		return fakeCamera;
	}

	public Vector2 getTexCoordDelta() {
		return texCoordDelta;
	}
	public Vector3 get3DDelta() {
		return positionDelta;
	}

	public void setTexCoordDelta( Vector2 delta ) {
		texCoordDelta = delta;
	}
}
