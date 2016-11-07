// Model Rotator 
// based on SimpleMouseRotator from unity utility

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelRotator : MonoBehaviour
{
	public float rotationSpeedMouse = 2.0f;
	public float rotationSpeedVive = 200.0f;

	private Quaternion targetRotation;
	public float autoRotateSpeed = 720f;
	private float rotationStartTime = 0;
	private float rotationTime = 0.3f;

	private Vector3 previousVivePos;
	private bool rotating = false;

	void Start()
	{
		targetRotation = transform.localRotation;
	}

	private void Update()
	{
		if (UI.Core.instance.pointerIsOverPlatformUIObject == false) {
			InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
			if (inputDevice.getDeviceType() == InputDeviceManager.InputDeviceType.Mouse) {
				// Let mouse handle rotation:
				if (Input.GetMouseButton (1) || Input.GetKey (KeyCode.M)) {
					float inputH = -Input.GetAxis ("Mouse X");
					float inputV = -Input.GetAxis ("Mouse Y");

					Vector3 upVector = Camera.main.transform.up;
					Vector3 rightVector = Camera.main.transform.right;
					transform.RotateAround (transform.position, upVector, inputH * rotationSpeedMouse);
					transform.RotateAround (transform.position, rightVector, -inputV * rotationSpeedMouse);

					targetRotation = transform.localRotation;	// Make sure it doesn't auto-rotate back.
				}
			} else if( inputDevice.getDeviceType() == InputDeviceManager.InputDeviceType.ViveController ) {
				// Let left Vive controller handle rotation:
				LeftController lc = InputDeviceManager.instance.leftController;
				if (lc != null) {
					UnityEngine.EventSystems.PointerEventData.FramePressState triggerState = lc.triggerButtonState;
					if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed) {
						rotating = true;
						previousVivePos = lc.transform.localPosition;
					} else if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Released) {
						rotating = false;
						previousVivePos = new Vector3 (0, 0, 0);
					}
					if (rotating) {
						Vector3 upVector = Camera.main.transform.up;
						Vector3 rightVector = Camera.main.transform.right;
						transform.RotateAround (transform.position, upVector, (previousVivePos.x - lc.transform.localPosition.x) * rotationSpeedVive);
						transform.RotateAround (transform.position, rightVector, -(previousVivePos.y - lc.transform.localPosition.y) * rotationSpeedVive);
						targetRotation = transform.localRotation;	// Make sure it doesn't auto-rotate back.
						previousVivePos = lc.transform.localPosition;
					}
				}
			}
		}

		// Slowly rotate towards target, if any:
		//float step =  Time.time;
		transform.localRotation = Quaternion.Slerp( transform.localRotation, targetRotation, (Time.time - rotationStartTime)/rotationTime );

	}

	public void setTargetOrientation( Quaternion orientation, float timeForRotation = 0f )
	{
		targetRotation = orientation;
		rotationStartTime = Time.time;
		if (timeForRotation == 0f) {
			transform.localRotation = orientation;
			rotationTime = 1f;	// Avoid division by zero
		} else {
			rotationTime = timeForRotation;
		}
	}
}

