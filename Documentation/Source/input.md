Input
=================================

The IMHOTEP input system contains an abstract interface to the input devices, so that - for the most part - the developer does not need to worry about whether a mouse or controller is active.

It also tries to stay as close as possible to the standard Unity input system, which means that events are raised in a similar manner. However, there are places where the system is slightly different to the standard Unity input system, for example because we need additional buttons for the controllers.

Note: To simplify things, the trigger of the right controller is handled in the same way as a click with the left mouse button.

Generally, any GameObject can receive input events when it is being clicked, pointed at etc.
To receive these events, you need to:

1. Make sure that an active collider is attached to the GameObject
2. Attach a script to the GameObject which implements the specific event-interface (see below)



Implementing an interface
---------------------------------

To be able to receive events, the specific interface needs to be implemented. For example, the ToolChoise class listenes to the click event. This is done by "inheriting" the interface IPointerClickHandler and implementing its method "OnPointerClick":

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	public class ToolChoise : MonoBehaviour, IPointerClickHandler {
		// ...

		public void OnPointerClick( PointerEventData eventData )
		{
			toolControl.chooseTool (this);
		}	

		// ...
	}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Note: You'll need to include the Unity EventSystem at the top of your file:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	using UnityEngine.EventSystems;
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The PointerEventData can be used to check where the object was clicked, which button was clicked (right, middle, left), what the texture-coordinates at that position are etc. See the Unity documentation on 'PointerEventData' for more details.

A lot of the Unity event interfaces are also implemented in IMHOTEP. The most commonly used events are probably:

- IPointerClickHandler - void OnPointerClick( PointerEventData data )
	Called when the mouse (or controller trigger) is clicked *and* released over the object.
- IPointerEnterHandler - void OnPointerEnter( PointerEventData data )
	Called when the mouse/controller pointer moves onto the object.
- IPointerExitHandler - void OnPointerExit( PointerEventData data )
	Called when the mouse/controller pointer moves off the object.
- IPointerDownHandler - void OnPointerDown( PointerEventData data )
	Called when the mouse/controller is pressed while on the object.
- IPointerUpHandler - void OnPointerUp( PointerEventData data )
	Called when the mouse/controller is released while on the object.
- IScrollHandler - void OnScroll( PointerEventData data )
	Called when the mouse wheel was used while hovering over the object.
	Note: This is also called when using the controller's touchpad.

Custom events added by us:

- IPointerHoverHandler - void OnPointerHover( PointerEventData data )
	Called whenever the mouse is over an object.

Note:
When you've clicked on (or hovered over) an object, the texture coordinates are passed along as well.
However, since Unity does not pass these along, the PointerEventData which we send to the above events
is actually a CustomEventData, which holds this extra information (it inherits from PointerEventData.
To access the texture coordinate of the hit position, you can cast the PointerEventData to a 
CustomEventData:
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	public class MyClass : MonoBehaviour, IPointerClickHandler {

		public void OnPointerClick( PointerEventData eventData )
		{
			CustomEventData cEventData = eventData as CustomEventData;
			if( cEventData != null )
			{
				Debug.Log("u,v coordinates: " + cEventData.textureCoord );
			}
		}	
	}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

See also CustomEventData.

Get Raw Input
------------------------------------
The interface above abstracts the events so that in many cases, you don't need to worry about whether the mouse or the controllers are active. However, there are some times when you do need to handle the mouse and the controllers differently. For this, you can retrieve the current input device:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

First, you should check which type of input we're currently getting:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) 
	{
		// ...
	} else if(inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.Mouse) {
		// ...
	}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

In case the mouse is active, you can use Unity's standard Input class to get the Mouse Position and speed etc. For example:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	Input.GetAxis("Mouse X")
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

If the Vive controllers are active, you can cast the input device to a Controller:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	Controller c = inputDevice as Controller;
	if (c != null) {
		// ...
	}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

If the Vive controller is active then the other controller can be accessed by (Note: always check if ``lc`` is not null - it might not be set if the controller is currently not tracked!)

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	LeftController lc = InputDeviceManager.instance.leftController;
	if (lc != null) {
		Controller c = inputDevice as Controller;
		if (c != null) {
			// ...
		}
	}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

In both cases, you can check whether the trigger is pressed, where the controller is, how it's oriented etc. (see Controller for details):

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{.cs}
	// Check if trigger is pressed all the way:
	if( c.triggerPressed() ) {
		// ...
	}
	
	// Check how much the trigger is pressed down:
	float pressAmount = c.triggerValue();

	// Let the controller shake briefly (please don't overuse!)
	c.shake( 1000 );

	// Get the world position of the controller:
	Vector3 pos = c.transform.position;
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
