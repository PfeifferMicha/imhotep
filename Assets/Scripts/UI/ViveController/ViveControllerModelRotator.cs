using UnityEngine;
using System.Collections;

public class ViveControllerModelRotator : MonoBehaviour {

	public GameObject meshNode;
	public float rotationSpeed = 2.0f;

	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId dPadLeft = Valve.VR.EVRButtonId.k_EButton_DPad_Left;
	private Valve.VR.EVRButtonId dPadRight = Valve.VR.EVRButtonId.k_EButton_DPad_Right;


	// Use this for initialization
	void Start () {
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (controller.GetPress (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
			float inputV = 0;
			float inputH = 0;
			Vector2 axes = controller.GetAxis ();
			//Debug.LogWarning (axes);
			if (axes.x > 0.5) {
				inputH = -0.5f;
				Debug.LogWarning ("1");
			}else if(axes.x < -0.5){
				inputH = 0.5f;
				Debug.LogWarning ("2");
			}

			if (axes.y > 0.5) {
				inputV = -0.5f;
				Debug.LogWarning ("3");
			}else if(axes.y < -0.5){
				inputV = 0.5f;
				Debug.LogWarning ("4");
			}

			Vector3 upVector = Camera.main.transform.up;
			Vector3 rightVector = Camera.main.transform.right;
			meshNode.transform.RotateAround(meshNode.transform.position, upVector, inputH*rotationSpeed);
			meshNode.transform.RotateAround(meshNode.transform.position, rightVector, -inputV*rotationSpeed );
		}

	}

}
