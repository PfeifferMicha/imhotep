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


	private Sprite iconLeftControllerLeft;
	private Sprite iconLeftControllerRight;
	private Sprite iconLeftControllerUp;
	private Sprite iconLeftControllerDown;
	private Sprite iconRightControllerLeft;
	private Sprite iconRightControllerRight;
	private Sprite iconRightControllerUp;
	private Sprite iconRightControllerDown;


	public InputDeviceManager()
	{
		instance = this;
	}

	public void registerInputDevice(InputDevice device)
    {
		deviceList.Add(device);
		currentInputDevice = device; //TODO how to change currentInputDevice in game?

		if (device.getDeviceType () == InputDeviceType.ViveController) {
			Controller c = device as Controller;
			c.setTouchpadDirectionIcons (iconLeftControllerLeft, iconLeftControllerRight, iconLeftControllerUp, iconLeftControllerDown);
		}
    }

	public void registerLeftController( LeftController left )
	{
		leftController = left;
		left.setTouchpadDirectionIcons (iconLeftControllerLeft, iconLeftControllerRight, iconLeftControllerUp, iconLeftControllerDown);
	}

	public void shakeLeftController( ushort milliseconds )
	{
		if (leftController != null) {
			leftController.shake (milliseconds);
		}
	}

	///////////////////////////////////////////////
	/// Handle Icons:
	public void setLeftControllerTouchpadIcons( Sprite left, Sprite right, Sprite up, Sprite down )
	{
		iconLeftControllerLeft = left;
		iconLeftControllerRight = right;
		iconLeftControllerUp = up;
		iconLeftControllerDown = down;
		if (leftController != null) {
			leftController.setTouchpadDirectionIcons (left, right, up, down);
		}
	}

	public void setRightControllerTouchpadIcons( Sprite left, Sprite right, Sprite up, Sprite down )
	{
		iconRightControllerLeft = left;
		iconRightControllerRight = right;
		iconRightControllerUp = up;
		iconRightControllerDown = down;
		if (currentInputDevice != null && currentInputDevice.getDeviceType() == InputDeviceType.ViveController) {
			Controller c = currentInputDevice as Controller;
			c.setTouchpadDirectionIcons (left, right, up, down);
		}
	}

	public void resetToolIcons()
	{
		iconRightControllerLeft = null;
		iconRightControllerRight = null;
		iconRightControllerUp = null;
		iconRightControllerDown = null;
		iconLeftControllerLeft = null;
		iconLeftControllerRight = null;
		iconLeftControllerUp = null;
		iconLeftControllerDown = null;
		if (currentInputDevice != null && currentInputDevice.getDeviceType() == InputDeviceType.ViveController) {
			Controller c = currentInputDevice as Controller;
			c.setTouchpadDirectionIcons (null, null, null, null);
		}
		if (leftController != null) {
			leftController.setTouchpadDirectionIcons (null, null, null, null);
		}
	}
}

