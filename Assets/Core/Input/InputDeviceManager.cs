using UnityEngine;
using System.Collections.Generic;



public class InputDeviceManager : MonoBehaviour {

    public enum RayInfoStates
    {
        rayHitsUI,
        rayHitsMesh,
        rayHitsBackground
    }

	public enum InputDeviceType
	{
		Mouse,
		ViveController
	}

    public RayInfoStates rayInfo = RayInfoStates.rayHitsBackground;

	public InputDevice currentInputDevice = null; //Defines with game object controlls the input

	private List<InputDevice> deviceList = new List<InputDevice>(); //List of registered input devices (e.g. mouse, vive contoller ...) 

	public static InputDeviceManager instance { private set; get; }

	public LeftController leftController { private set; get; }

	public InputDeviceManager()
	{
		instance = this;
	}

	public void registerInputDevice(InputDevice device)
    {
		deviceList.Add(device);
		currentInputDevice = device; //TODO how to change currentInputDevice in game?
    }

	public void registerLeftController( LeftController left )
	{
		leftController = left;
	}

	public void shakeLeftController( ushort milliseconds )
	{
		if (leftController != null) {
			leftController.shake (milliseconds);
		}
	}
}

