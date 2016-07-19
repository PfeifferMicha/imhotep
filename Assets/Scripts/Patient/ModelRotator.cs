// Model Rotator 
// based on SimpleMouseRotator from unity utility

using System;
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
	public float rotationSpeed = 2.0f;

	private void Update()
	{
		if (UI.UICore.instance.mouseIsOverUIObject == false) {
			if (Input.GetMouseButton (1) || Input.GetKey (KeyCode.M)) {
				float inputH = -Input.GetAxis ("Mouse X");
				float inputV = -Input.GetAxis ("Mouse Y");

				Vector3 upVector = Camera.main.transform.up;
				Vector3 rightVector = Camera.main.transform.right;
				transform.RotateAround (transform.position, upVector, inputH * rotationSpeed);
				transform.RotateAround (transform.position, rightVector, -inputV * rotationSpeed);
			}
		}
	}
}

