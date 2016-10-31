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

	private const float mouseSpeed = 0.4f;

	private Vector2 texCoordDelta;
	private Vector3 positionDelta;

	private Vector3 rayDir = new Vector3( 0, 0, 1 );

	private ButtonInfo buttonInfo = new ButtonInfo();

    public Ray createRay()
    {

		if (!developmentMode) {
			Cursor.lockState = CursorLockMode.Locked;
		}

        Ray ray;
		if(developmentMode){
        	ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		}
		else{ //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			rayDir = Quaternion.AngleAxis( Input.GetAxis("Mouse X") * mouseSpeed, Vector3.up ) * rayDir;
			rayDir = Quaternion.AngleAxis ( -Input.GetAxis ("Mouse Y") * mouseSpeed, Vector3.right) * rayDir;

			ray = new Ray(Camera.main.transform.position + rayOriginOffset, rayDir); 
		}
        return ray;
    }

    public Vector2 getScrollDelta()
    {
        return Input.mouseScrollDelta*20f;
    }

    // Use this for initialization
    void Start () {
        lineRenderer = this.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("[MouseInput.cs] Line renderer not set");
        }

		if (InputDeviceManager.instance != null)
        {
			InputDeviceManager.instance.registerInputDevice(this);
            Debug.Log("Mouse registered");
        }
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

		if ( ! mid.developmentMode )
			mid.rayOriginOffset = EditorGUILayout.Vector3Field ("Ray Origin Offset", new Vector3( 0.2f, -0.3f,  0f ));
		else
			mid.rayOriginOffset = Vector3.zero;
	}
}
#endif