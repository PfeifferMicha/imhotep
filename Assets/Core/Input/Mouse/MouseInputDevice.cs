using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MouseInputDevice : MonoBehaviour, InputDevice {
	
	public InputDeviceManager.InputDeviceType getDeviceType ()
	{
		return InputDeviceManager.InputDeviceType.Mouse;
	}

	public bool developmentMode = true;
	public Vector3 rayOriginOffset;

    private LineRenderer lineRenderer;

	public float mouseSpeed = 1.4f;

	private Vector2 texCoordDelta;
	private Vector3 positionDelta;

	private Vector3 rayAngle = new Vector3( 0, 0, 0 );

	private ButtonInfo buttonInfo = new ButtonInfo();

    public Ray createRay()
    {

        Ray ray;
		if(developmentMode){
        	ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		}
		else{
			rayAngle = rayAngle + new Vector3 ( -Input.GetAxis("Mouse Y") * mouseSpeed, Input.GetAxis("Mouse X") * mouseSpeed, 0f );
			Vector3 rayDir = Quaternion.Euler (rayAngle) * Vector3.forward;

			ray = new Ray(Camera.main.transform.position + rayOriginOffset, rayDir); 
		}
        return ray;
    }

    public Vector2 getScrollDelta()
    {
        return Input.mouseScrollDelta*20f;
    }

    void Start () {
		lineRenderer = this.GetComponent<LineRenderer> ();
		if (lineRenderer == null) {
			Debug.LogError ("[MouseInput.cs] Line renderer not set");
		}

		if (InputDeviceManager.instance != null) {
			InputDeviceManager.instance.registerInputDevice (this);
			Debug.Log ("Mouse registered");
		}
	}

	public void Update() {
		if (!developmentMode && Input.GetKey ("escape")) {
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void OnEnable() {
		if (!developmentMode) {
			Cursor.lockState = CursorLockMode.Locked;
		}
    }

	public bool isLeftButtonDown()
	{
		return Input.GetMouseButton (0);
	}
	public bool isRightButtonDown()
	{
		return Input.GetMouseButton (1);
	}
	public bool isMiddleButtonDown()
	{
		return Input.GetMouseButton (2);
	}

	public ButtonInfo updateButtonInfo ()
	{

		if (Input.GetMouseButtonDown (0) && Input.GetMouseButtonUp (0)) {
			buttonInfo.buttonStates [ButtonType.Left] = PointerEventData.FramePressState.PressedAndReleased;
		} else if (Input.GetMouseButtonDown (0)) {
			buttonInfo.buttonStates [ButtonType.Left] = PointerEventData.FramePressState.Pressed;
		} else if (Input.GetMouseButtonUp (0)) {
			buttonInfo.buttonStates [ButtonType.Left] = PointerEventData.FramePressState.Released;
		} else {
			buttonInfo.buttonStates [ButtonType.Left] = PointerEventData.FramePressState.NotChanged;
		}

		if (Input.GetMouseButtonDown (1) && Input.GetMouseButtonUp (1)) {
			buttonInfo.buttonStates [ButtonType.Right] = PointerEventData.FramePressState.PressedAndReleased;
		} else if (Input.GetMouseButtonDown (1)) {
			buttonInfo.buttonStates [ButtonType.Right] = PointerEventData.FramePressState.Pressed;
		} else if (Input.GetMouseButtonUp (1)) {
			buttonInfo.buttonStates [ButtonType.Right] = PointerEventData.FramePressState.Released;
		} else {
			buttonInfo.buttonStates [ButtonType.Right] = PointerEventData.FramePressState.NotChanged;
		}

		if (Input.GetMouseButtonDown (2) && Input.GetMouseButtonUp (2)) {
			buttonInfo.buttonStates [ButtonType.Middle] = PointerEventData.FramePressState.PressedAndReleased;
		} else if (Input.GetMouseButtonDown (2)) {
			buttonInfo.buttonStates [ButtonType.Middle] = PointerEventData.FramePressState.Pressed;
		} else if (Input.GetMouseButtonUp (2)) {
			buttonInfo.buttonStates [ButtonType.Middle] = PointerEventData.FramePressState.Released;
		} else {
			buttonInfo.buttonStates [ButtonType.Middle] = PointerEventData.FramePressState.NotChanged;
		}

		return buttonInfo;
	}

	public Camera getEventCamera()
	{
		return Camera.main;
	}

	//! Returns the difference in texture coordinates between this and the last frame:
	public Vector2 getTexCoordDelta() {
		return texCoordDelta;
	}
	public Vector3 get3DDelta() {
		return positionDelta;
	}

	public void setTexCoordDelta( Vector2 delta ) {
		texCoordDelta = delta;
	}
	public void set3DDelta( Vector2 delta ) {
		positionDelta = delta;
	}
}

#if UNITY_EDITOR
//! Show/Hide 
[CustomEditor(typeof(MouseInputDevice))]
public class MouseInputDeviceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MouseInputDevice mid = target as MouseInputDevice;

		mid.developmentMode = GUILayout.Toggle(mid.developmentMode, "Development Mode");

		if (!mid.developmentMode) {
			mid.rayOriginOffset = EditorGUILayout.Vector3Field ("Ray Origin Offset", new Vector3 (0.2f, -0.3f, 0f));
			mid.mouseSpeed = EditorGUILayout.FloatField ("Mouse Speed", 1.5f);
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			mid.rayOriginOffset = Vector3.zero;
			mid.mouseSpeed = 1.5f;
			Cursor.lockState = CursorLockMode.None;
		}
	}
}
#endif