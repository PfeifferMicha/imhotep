using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class ViveControllerInputDevice : Controller, InputDevice {

	InputDeviceManager.InputDeviceType getType ()
	{
		return InputDeviceManager.InputDeviceType.ViveController;
	}

	private Vector2 texCoordDelta;
	private Vector3 positionDelta;

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

	// Use this for initialization
	void Start () {

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
		UpdateTouchpad ();

		buttonInfo.buttonStates [ButtonType.Trigger] = UpdateTriggerState ();

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
	public void set3DDelta( Vector2 delta ) {
		positionDelta = delta;
	}
}
