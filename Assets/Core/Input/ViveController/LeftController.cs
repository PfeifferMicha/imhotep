using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LeftController : Controller {

	public float rotationSpeed = 200.0f;
	public GameObject scalingNode;

	private Vector3 previousPos;

	private bool rotating = false;
	private bool zooming = false;
	private Vector3 mOriginalZoom;
	private float originalDist;

	// Use this for initialization
	void Start () {
		InputDeviceManager.instance.registerLeftController (this);
	}

	public Vector2 getScrollDelta()
	{
		return touchpadDelta*100;
	}

	new void Update()
	{
		base.Update();
		UpdateTouchpad ();
		UpdateTriggerState ();
	}
}
