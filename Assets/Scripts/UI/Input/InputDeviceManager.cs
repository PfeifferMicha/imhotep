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

	public InputDevice currentInputDevice = null; //Defines with game object controlls the input

	private List<InputDevice> deviceList = new List<InputDevice>(); //List of registered input devices (e.g. mouse, vive contoller ...) 

	public static InputDeviceManager instance { private set; get; }

	public InputDeviceManager()
	{
		instance = this;
	}

	public bool registerInputDevice(InputDevice device)
    {
		deactivateAllVisualizer();
		deviceList.Add(device);
		currentInputDevice = device; //TODO how to change currentInputDevice in game?
        return true;
    }

	private void deactivateAllVisualizer(){
		foreach (InputDevice i in deviceList) {
			i.deactivateVisualization();
		}
	}
}

