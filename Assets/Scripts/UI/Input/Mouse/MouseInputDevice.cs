using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEditor;

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

    public PointerEventData.FramePressState getLeftButtonState()
    {
        return mouseButtonStateHandler(leftButtonState, 0);
    }


    public PointerEventData.FramePressState getMiddleButtonState()
    {
        return mouseButtonStateHandler(middleButtonState, 2);
    }

    public PointerEventData.FramePressState getRightButtonState()
    {
        return mouseButtonStateHandler(rightButtonState, 1);
    }

    public Vector2 getScrollDelta()
    {
        return Input.mouseScrollDelta;
    }

	public Vector2 getTexCoordMovement()
	{
		return texCoordDelta;
	}
	public Vector3 getMovement()
	{
		return positionDelta;
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


		//TODO -----------------------------------
		RaycastHit hit;
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);       
		//LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer
		Physics.Raycast(ray, out hit, Mathf.Infinity);
		lastPos = hit.point;


		//----------------------------------------

    }
	
	// Update is called once per frame
	/*void Update () {
		RaycastHit result = new RaycastHit();
		bool hit = false;
		if (visualizeMouseRay) {
			Ray ray = createRay ();
			//LayerMask onlyMousePlane = 1 << 8; // hit only the mouse plane layer
			if (Physics.Raycast (ray, out result, Mathf.Infinity)) {
				Vector3 offset = new Vector3 (0.4f, -0.4f, 0);
				lineRenderer.SetPosition (0, Camera.main.transform.position + offset);
				lineRenderer.SetPosition (1, result.point);
				hit = true;
				Debug.Log ("hit: " + result.transform.name);
				PointerEventData data = new PointerEventData (EventSystem.current);
				ExecuteEvents.Execute (result.transform.gameObject, data, ExecuteEvents.pointerEnterHandler);
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
    }*/

    private PointerEventData.FramePressState mouseButtonStateHandler(PointerEventData.FramePressState s, int buttonID)
    {
        PointerEventData.FramePressState result = PointerEventData.FramePressState.NotChanged;
        switch (s)
        {
            case PointerEventData.FramePressState.NotChanged:
                if (Input.GetMouseButtonDown(buttonID))
                {
                    result = PointerEventData.FramePressState.Pressed;
                }
                if (Input.GetMouseButtonUp(buttonID))
                {
                    result = PointerEventData.FramePressState.Released;
                }
                break;
            case PointerEventData.FramePressState.Pressed:
                if (Input.GetMouseButtonUp(buttonID))
                {
                    result = PointerEventData.FramePressState.Released;
                }
                else
                {
                    result = PointerEventData.FramePressState.NotChanged;
                }
                break;
            case PointerEventData.FramePressState.Released:
                if (Input.GetMouseButtonDown(buttonID))
                {
                    result = PointerEventData.FramePressState.Pressed;
                }
                else
                {
                    result = PointerEventData.FramePressState.NotChanged;
                }
                break;
            default:
                break;
        }
        return result;
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