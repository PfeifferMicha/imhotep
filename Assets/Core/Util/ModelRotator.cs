// Model Rotator 
// based on SimpleMouseRotator from unity utility

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelRotator : MonoBehaviour
{
	public float rotationSpeedMouse = 2.0f;
	public float rotationSpeedVive = 200.0f;
	public float rollSpeed = 0.5f;


	public enum RotationMode {
		//! Standard Mode maps Controller left-right movement to rotation around the global "up" vector
		//! and up-down movement to a global axis from left to right, so it ignores the model's current orientation.
		MODE_STANDARD,
		//! Fixed-Up-Axis Mode maps up-down movement to the model's tilt value. Also has a maximum tilt rotation of
		//! +- 85 degrees
		MODE_FIXED_UP_AXIS,
		//! Map Rotation of Controller to rotation of organ (Vive Controller only!)
		MODE_MAP_ROTATION,
		//! Map Rotation of Controller to roll of organ (Vive Controller only!)
		MODE_STANDARD_PLUS_ROLL
	}

	public RotationMode mouseRotationMode = RotationMode.MODE_STANDARD;
	public RotationMode viveRotationMode = RotationMode.MODE_STANDARD;

	public Quaternion targetRotation {
		get;
		private set;
	}
	public float autoRotateSpeed = 720f;
	private float rotationStartTime = 0;
	private float rotationTime = 0.3f;

	private Vector3 previousVivePos;
	private bool rotating = false;

	private float leftRightAng = 0;
	private float upDownAng = 0;

	public float manualRotationSpeed = 0.5f;

	private Quaternion previousControllerRotation;
	//private Quaternion initialModelOrientation;

	void Start()
	{
		targetRotation = transform.localRotation;
	}

	private void Update()
	{
		if (UI.Core.instance.layoutSystem.activeScreen == UI.Screen.center) {

			InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
			if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.Mouse) {
				// Let mouse handle rotation:
				if (Input.GetMouseButton (0) || Input.GetKey (KeyCode.M)) {
					float inputH = -Input.GetAxis ("Mouse X");
					float inputV = -Input.GetAxis ("Mouse Y");

					if (mouseRotationMode == RotationMode.MODE_FIXED_UP_AXIS) {
						// Fixed-Up-Axis Mode maps up-down movement to the model's tilt value. Also has a maximum tilt rotation of
						// +- 85 degrees

						leftRightAng = leftRightAng + inputH * rotationSpeedMouse;

						Quaternion leftRightQuat = Quaternion.AngleAxis ( leftRightAng, new Vector3 (0, 1, 0));
						transform.rotation = leftRightQuat * Quaternion.AngleAxis (90f, new Vector3 (1, 0, 0));

						// Decide in which direction to rotate by checking if the Y-Vector is facing towards the camera:
						float dist1 = Vector3.Distance (transform.TransformPoint (new Vector3 (0, 1, 0)), Camera.main.transform.position);
						float dist2 = Vector3.Distance (transform.TransformPoint (new Vector3 (0, -1, 0)), Camera.main.transform.position);
						if (dist1 > dist2)
							inputV = -inputV;
						upDownAng = Mathf.Clamp (upDownAng + inputV * rotationSpeedMouse, -85, 85);
						Quaternion upDownQuat = Quaternion.AngleAxis ( upDownAng, new Vector3 (1, 0, 0));
						transform.localRotation *= upDownQuat;
					} else {

						// Standard Mode maps Controller left-right movement to rotation around the global "up" vector
						// and up-down movement to a global axis from left to right, so it ignores the model's current orientation.

						Vector3 upVector = Camera.main.transform.up;
						Vector3 rightVector = Camera.main.transform.right;
						transform.RotateAround (transform.position, upVector, inputH * rotationSpeedMouse);
						//transform.RotateAround (transform.position, rightVector, -inputV * rotationSpeedMouse);
					}

					targetRotation = transform.localRotation;	// Make sure it doesn't auto-rotate back.
				}
			} else if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
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

						float inputH = (previousVivePos.x - lc.transform.localPosition.x);
						float inputV = -(previousVivePos.y - lc.transform.localPosition.y);
						if (viveRotationMode == RotationMode.MODE_STANDARD) {
							// Standard Mode maps Controller left-right movement to rotation around the global "up" vector
							// and up-down movement to a global axis from left to right, so it ignores the model's current orientation.
							Vector3 upVector = Camera.main.transform.up;
							Vector3 rightVector = Camera.main.transform.right;
							transform.RotateAround (transform.position, upVector, inputH * rotationSpeedVive);
							transform.RotateAround (transform.position, rightVector, inputV * rotationSpeedVive);

						} else if (viveRotationMode == RotationMode.MODE_FIXED_UP_AXIS) {
							// Fixed-Up-Axis Mode maps up-down movement to the model's tilt value. Also has a maximum tilt rotation of
							// +- 85 degrees

							leftRightAng = leftRightAng + inputH * rotationSpeedVive;

							Quaternion leftRightQuat = Quaternion.AngleAxis (leftRightAng, new Vector3 (0, 1, 0));
							transform.rotation = leftRightQuat * Quaternion.AngleAxis (90f, new Vector3 (1, 0, 0));

							// Decide in which direction to rotate by checking if the Y-Vector is facing towards the camera:
							float dist1 = Vector3.Distance (transform.TransformPoint (new Vector3 (0, 1, 0)), Camera.main.transform.position);
							float dist2 = Vector3.Distance (transform.TransformPoint (new Vector3 (0, -1, 0)), Camera.main.transform.position);
							if (dist1 <= dist2)
								inputV = -inputV;
							upDownAng = Mathf.Clamp (upDownAng + inputV * rotationSpeedVive, -85, 85);
							Quaternion upDownQuat = Quaternion.AngleAxis (upDownAng, new Vector3 (1, 0, 0));
							transform.localRotation *= upDownQuat;
						} else if (viveRotationMode == RotationMode.MODE_MAP_ROTATION) {
							// previousControllerRotation * delta = lc.transform.rotation;
							Quaternion delta = lc.transform.rotation * Quaternion.Inverse (previousControllerRotation);
							float angle;
							Vector3 axis;
							delta.ToAngleAxis (out angle, out axis);
							angle *= manualRotationSpeed;
							Quaternion scaledDelta = Quaternion.AngleAxis (angle, axis);
							transform.rotation = scaledDelta * transform.rotation;
						} else if (viveRotationMode == RotationMode.MODE_STANDARD_PLUS_ROLL) {
							// Standard Mode maps Controller left-right movement to rotation around the global "up" vector
							// and up-down movement to a global axis from left to right, so it ignores the model's current orientation.
							Vector3 upVector = Camera.main.transform.up;
							Vector3 rightVector = Camera.main.transform.right;
							transform.RotateAround (transform.position, upVector, inputH * rotationSpeedVive);
							transform.RotateAround (transform.position, rightVector, inputV * rotationSpeedVive);

							// Additional Rotation (rolling):
							Quaternion delta = lc.transform.rotation * Quaternion.Inverse (previousControllerRotation);
							float inputRoll = delta.eulerAngles.z;
							while (inputRoll > 180)
								inputRoll -= 360;
							while (inputRoll < -180)
								inputRoll += 360;
							Vector3 forwardVector = Camera.main.transform.forward;
							transform.RotateAround (transform.position, forwardVector, inputRoll*rollSpeed );
						}

						targetRotation = transform.localRotation;	// Make sure it doesn't auto-rotate back.
						previousVivePos = lc.transform.localPosition;
					}
					previousControllerRotation = lc.transform.rotation;
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

