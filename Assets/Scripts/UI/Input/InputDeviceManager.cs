using UnityEngine;
using System.Collections.Generic;



public class InputDeviceManager : MonoBehaviour {
	

    public enum RayInfoStates
    {
        rayHitsUI,
        rayHitsMesh,
        rayHitsBackground
    }

    public RayInfoStates rayInfo = RayInfoStates.rayHitsBackground;

    public GameObject currentInputDevice { get; set; } //Defines with game object controlls the input

    private List<GameObject> deviceList = new List<GameObject>(); //List of registered input devices (e.g. mouse, vive contoller ...) 

	public static InputDeviceManager instance { private set; get; }

	public InputDeviceManager()
	{
		instance = this;
	}

    // Use this for initialization
    void Start () {        
        
	}

    void OnEnable()
    {
        // Register event callbacks:
        InputEventSystem.startListening(InputEventSystem.Event.INPUTDEVICE_LeftButtonPressed, 8, leftButtonPressed);
    }

    void OnDisable()
    {
        // Unregister myself:
        InputEventSystem.stopListening(InputEventSystem.Event.INPUTDEVICE_LeftButtonPressed, 8, leftButtonPressed);
    }

    // Update is called once per frame
    void Update () {
        //Update rayInfo
        if (UI.Core.instance.mouseIsOverUIObject)
        {
            rayInfo = RayInfoStates.rayHitsUI;
        }
        else
        {
            if (deviceList.Count <= 0)
            {
                return;
            }

            RaycastHit hit;
            Ray ray = currentInputDevice.GetComponent<InputDeviceInterface>().createRay();
            LayerMask onlyMeshViewLayer = 1000000000; // hits only the mesh view layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                rayInfo = RayInfoStates.rayHitsMesh;
            }
            else
            {
                rayInfo = RayInfoStates.rayHitsBackground;
            }
        }
    }

    public bool registerInputDevice(GameObject g)
    {
        if(g.GetComponent<InputDeviceInterface>() == null)
        {
            return false;
        }
		deactivateAllVisualizer();
        deviceList.Add(g);
        currentInputDevice = g; //TODO how to change currentInputDevice in game?

        return true;
    }

	private void deactivateAllVisualizer(){
		foreach (GameObject g in deviceList) {
			InputDeviceInterface i = g.GetComponent<InputDeviceInterface> ();
			if (i != null) {
				i.deactivateVisualization();
			}
		}
	}


    private void leftButtonPressed(object o)
    {
        RaycastHit rh = (RaycastHit)o;
        Debug.Log("Left button pressed on layer 8, point: "+ rh.point);
    }
}

