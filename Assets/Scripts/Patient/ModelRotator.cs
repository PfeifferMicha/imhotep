// Model Rotator 
// based on SimpleMouseRotator from unity utility

using System;
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
	public Camera mainCamera;
	public float rotationSpeed = 2.0f;

	private void Update()
	{
		if (Input.GetMouseButton (2))
		{
			float inputH = -Input.GetAxis ("Mouse X");
			float inputV = -Input.GetAxis ("Mouse Y");

			Vector3 upVector = mainCamera.transform.up;
			Vector3 rightVector = mainCamera.transform.right;
			transform.RotateAround (transform.position, upVector, inputH*rotationSpeed );
			transform.RotateAround (transform.position, rightVector, -inputV*rotationSpeed );
		}
	}
}

