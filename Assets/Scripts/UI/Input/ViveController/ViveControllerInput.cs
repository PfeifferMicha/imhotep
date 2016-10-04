using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ViveControllerInput : MonoBehaviour, InputDeviceInterface {

	//--------------- controller stuff---------------------
	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	//private PointerEventData.FramePressState leftButtonState;
	private bool triggerPressedDown = false; //True of the trigger is pressed down

	private bool helpState = false; //Before releasing the button press it again to avoid scrolling in lists
	//-----------------------------------------------------

	private PointerEventData.FramePressState leftButtonState = PointerEventData.FramePressState.NotChanged;

	private bool visualizeRay = true;

	private LineRenderer lineRenderer;

	private bool previousRayHitSomething;
	private Vector2 texCoordPrevious;
	private Vector2 texCoordDelta;
	private Vector3 positionPrevious;
	private Vector3 positionDelta;

	public void activateVisualization()
	{
		visualizeRay = true;
	}

	public void deactivateVisualization()
	{
		visualizeRay = false;
		Vector3 zero = new Vector3(0, 0, 0);
		lineRenderer.SetPosition(0, zero);
		lineRenderer.SetPosition(1, zero);
	}

	public Ray createRay()
	{
		Ray ray;
		ray= new Ray(this.gameObject.transform.position, this.gameObject.transform.forward);
		return ray;
	}

	public PointerEventData.FramePressState getLeftButtonState()
	{
		return leftButtonState;
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
		return new Vector2(0,0); //TODO?
	}

	public Vector2 getTexCoordMovement()
	{
		return texCoordDelta;
	}
	public Vector3 getMovement()
	{
		return positionDelta;
	}

	public RaycastHit getRaycastHit()
	{
		RaycastHit hit;
		Ray ray = createRay();       
		//LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer
		if (Physics.Raycast(ray, out hit, Mathf.Infinity))
		{
			return hit;
		}
		else
		{
			//Debug.LogError("No hit found. Can not return currect UV Coordiantes"); //TODO?
			return hit;
		}
	}

	public bool isVisualizerActive()
	{
		return visualizeRay;
	}

	// Use this for initialization
	void Start () {
		//initialization
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();

		//register device

		if (InputDeviceManager.instance != null)
		{
			InputDeviceManager.instance.registerInputDevice(this.gameObject);
			Debug.LogWarning("Vive controller registered");
		}

		//find line renderer
		lineRenderer = this.GetComponent<LineRenderer>();
		if (lineRenderer == null)
		{
			Debug.LogError("[MouseInput.cs] Line renderer not set");
		}


	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit result = new RaycastHit();
		bool hit = false;
		if (visualizeRay)
		{
			Ray ray = createRay();
			//LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer
			if (Physics.Raycast(ray, out result, Mathf.Infinity))
			{
				lineRenderer.SetPosition(0, this.gameObject.transform.position);
				lineRenderer.SetPosition(1, result.point);
				hit = true;
			}
		}

		// If we hit something, update the delta movement vector:
		if( hit ) {
			if (previousRayHitSomething) {
				// Update 2D position:
				texCoordDelta = result.textureCoord - texCoordPrevious;
				// Update 3D position:
				positionDelta = result.point - positionPrevious;
			}
			previousRayHitSomething = true;
			texCoordPrevious = result.textureCoord;
			positionPrevious = result.point;
		} else {
			previousRayHitSomething = false;
		}


		if (controller == null) {
			return;
		}

		switch (leftButtonState) {
		case PointerEventData.FramePressState.NotChanged:
			if (triggerPressed () && !triggerPressedDown) {
				leftButtonState = PointerEventData.FramePressState.Pressed;
			}
			if (!triggerPressed () && triggerPressedDown) {
				if (helpState == false) {
					leftButtonState = PointerEventData.FramePressState.Pressed;
					helpState = true;
				} else {
					leftButtonState = PointerEventData.FramePressState.Released;
				}
			}
			break;

		case PointerEventData.FramePressState.Pressed:
			if (helpState) {
				helpState = false;
				leftButtonState = PointerEventData.FramePressState.Released;
			} else {
				triggerPressedDown = true;
				if (triggerPressed () && triggerPressedDown) {
					leftButtonState = PointerEventData.FramePressState.NotChanged;
				} else if (!triggerPressed () && triggerPressedDown) {
					leftButtonState = PointerEventData.FramePressState.Released;
				}
			}
			break;

			//case PointerEventData.FramePressState.PressedAndReleased:
			//break;

		case PointerEventData.FramePressState.Released:
			if (!triggerPressed ()) {
				leftButtonState = PointerEventData.FramePressState.NotChanged;
			} else {
				leftButtonState = PointerEventData.FramePressState.Pressed;
			}
			triggerPressedDown = false;
			break;			
		}

	}

	private bool triggerPressed(){
		//Checks if the trigger is pressed down till it clicks
		//Returns true as long as the trigger is pressed down
		if (controller.GetAxis (triggerButton) == new Vector2 (1.0f, 0.0f)) {
			return true;
			//Debug.Log ("Trigger compelete pressed");
		}
		return false;
	}
}
