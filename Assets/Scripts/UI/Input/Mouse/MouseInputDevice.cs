﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEditor;
using UI;

public class MouseInputDevice : MonoBehaviour, InputDevice {


	public bool developmentMode = true;
	public Vector3 rayOriginOffset;

    private PointerEventData.FramePressState leftButtonState = PointerEventData.FramePressState.NotChanged;
    private PointerEventData.FramePressState middleButtonState = PointerEventData.FramePressState.NotChanged;
    private PointerEventData.FramePressState rightButtonState = PointerEventData.FramePressState.NotChanged;
    private bool visualizeMouseRay = true;

    private LineRenderer lineRenderer;

	private float mouseSpeed = 0.4f;
	private Vector3 lastPos = new Vector3(0,0,0);

	private bool previousRayHitSomething;
	private Vector2 texCoordPrevious;
	private Vector2 texCoordDelta;
	private Vector3 positionPrevious;
	private Vector3 positionDelta;

	private Vector3 rayDir = new Vector3( 0, 0, 1 );

	private ButtonInfo buttonInfo = new ButtonInfo();

    public void activateVisualization()
    {
        visualizeMouseRay = true;
    }

    public void deactivateVisualization()
    {
        visualizeMouseRay = false;
		Vector3 zero = new Vector3(0, 0, 0);
		lineRenderer.SetPosition(0, zero);
		lineRenderer.SetPosition(1, zero);
    }

    public Ray createRay()
    {
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
        return Input.mouseScrollDelta;
    }

    public bool isVisualizerActive()
    {
        return visualizeMouseRay;
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
}

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