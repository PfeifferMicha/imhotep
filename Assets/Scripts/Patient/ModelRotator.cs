// Model Rotator 
// based on SimpleMouseRotator from unity utility

using System;
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
	public float rotationSpeed = 2.0f;

	private Quaternion targetRotation;
	public float autoRotateSpeed = 720f;
	private float rotationStartTime = 0;
	private float rotationTime = 0.3f;

	void Start()
	{
		targetRotation = transform.localRotation;
	}

	private void Update()
	{
		if (UI.Core.instance.mouseIsOverUIObject == false) {
			if (Input.GetMouseButton (1) || Input.GetKey (KeyCode.M)) {
				float inputH = -Input.GetAxis ("Mouse X");
				float inputV = -Input.GetAxis ("Mouse Y");

				Vector3 upVector = Camera.main.transform.up;
				Vector3 rightVector = Camera.main.transform.right;
				transform.RotateAround (transform.position, upVector, inputH * rotationSpeed);
				transform.RotateAround (transform.position, rightVector, -inputV * rotationSpeed);

				targetRotation = transform.localRotation;
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

