using System;
using UnityEngine;

public class ModelMover : MonoBehaviour
{
	public float smoothTime = 0.3f;
	public Vector3 targetPosition = Vector3.zero;

	private Vector3 currentVelocity = Vector3.zero;

	public void Start()
	{
		targetPosition = transform.localPosition;
	}

	void Update()
	{
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref currentVelocity, smoothTime);
	}
}

