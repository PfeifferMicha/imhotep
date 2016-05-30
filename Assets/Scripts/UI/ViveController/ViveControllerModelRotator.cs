using UnityEngine;
using System.Collections;

public class ViveControllerModelRotator : MonoBehaviour {

	public GameObject meshNode;
	public float rotationSpeed = 200.0f;

	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private bool triggerPressed = false;
	private Vector3 previousPos = new Vector3 (0, 0, 0);

	// Use this for initialization
	void Start () {
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
	}
	
	// Update is called once per frame
	void Update () {
		LeftButtonState lbs = this.GetComponent<LeftButtonState> ();
		if (lbs != null && lbs.getLeftButtonState () == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed) {
			triggerPressed = true;
			previousPos = this.transform.localPosition;
		}else if (lbs.getLeftButtonState () == UnityEngine.EventSystems.PointerEventData.FramePressState.Released) {
			triggerPressed = false;
			previousPos = new Vector3 (0, 0, 0);
		}

		if (triggerPressed) {
			Vector3 upVector = Camera.main.transform.up;
			Vector3 rightVector = Camera.main.transform.right;
			meshNode.transform.RotateAround(meshNode.transform.position, upVector, (previousPos.x - this.transform.localPosition.x)*rotationSpeed);
			meshNode.transform.RotateAround(meshNode.transform.position, rightVector, -(previousPos.y - this.transform.localPosition.y)*rotationSpeed );

			previousPos = this.transform.localPosition;
		}

		/*
		if (controller.GetPress (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
			float inputV = 0;
			float inputH = 0;
			Vector2 axis = controller.GetAxis ();
			//Debug.LogWarning (axes);
			if (axis.x > 0.5) {
				inputH = -0.5f;
			}else if(axis.x < -0.5){
				inputH = 0.5f;
			}

			if (axis.y > 0.5) {
				inputV = -0.5f;
			}else if(axis.y < -0.5){
				inputV = 0.5f;
			}

			Vector3 upVector = Camera.main.transform.up;
			Vector3 rightVector = Camera.main.transform.right;
			meshNode.transform.RotateAround(meshNode.transform.position, upVector, inputH*rotationSpeed);
			meshNode.transform.RotateAround(meshNode.transform.position, rightVector, -inputV*rotationSpeed );
		} */

	}

}
