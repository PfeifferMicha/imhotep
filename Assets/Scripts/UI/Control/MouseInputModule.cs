using UnityEngine;
using UnityEngine.EventSystems;

/* This extends the StandaloneInputModule, to place the mouse position at a "fake"
 * position, i.e. on the UI camera space rather than using the "global" screen pointer.
 * Script adapted from:
 * http://forum.unity3d.com/threads/fake-mouse-position-in-4-6-ui-answered.283748/
 * */

public class MouseInputModule : StandaloneInputModule {

	private Camera UICamera;

    private InputDeviceManager idm;
	private Vector2 mTextureSize;

	//public PointerEventData.FramePressState framePressStateLeft { get; set; } //Other scripts can get the current state of the left mouse button TODO entfernen, veraltet


	public new void Start()
	{
        //mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> ();
        idm = GameObject.Find ("GlobalScript").GetComponent<InputDeviceManager> ();
        UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();
		mTextureSize.x = UICamera.targetTexture.width;
		mTextureSize.y = UICamera.targetTexture.height;
		//framePressStateLeft = PointerEventData.FramePressState.NotChanged;
	}

	// This is the real function we want, the two commented out lines (Input.mousePosition)
	// are replaced with m_cursorPos (our fake mouse position, set with the public function,
	// UpdateCursorPosition)
	private readonly MouseState m_MouseState = new MouseState();
	protected override MouseState GetMousePointerEventData( int id = 0 )
	{
		InputDevice inputDevice = idm.currentInputDevice;

        // convert to a Screen space position:
		Vector2 cursorPos = Vector2.zero;
        /*if (inputDevice.getRaycastHit().transform.gameObject.layer == 8) //Ray hits UI Plane
        {
            cursorPos = inputDevice.getRaycastHit().textureCoord2;
            cursorPos.x *= mTextureSize.x;
		    cursorPos.y *= mTextureSize.y;
        }
        else
        {
            cursorPos = Camera.main.WorldToScreenPoint(inputDevice.getRaycastHit().point);
        }*/

		//MouseState m = new MouseState();

		// Populate the left button...
		PointerEventData leftData;
		var created = GetPointerData( kMouseLeftId, out leftData, true );

		leftData.Reset();

		if (created)
        {
			leftData.position = cursorPos;
        }
		//leftData.position = Input.mousePosition;

		//Vector2 pos = Input.mousePosition;
		Vector2 pos = cursorPos;
		leftData.delta = pos - leftData.position;
		leftData.position = pos;
		leftData.scrollDelta = inputDevice.getScrollDelta();
		leftData.button = PointerEventData.InputButton.Left;
		eventSystem.RaycastAll(leftData, m_RaycastResultCache);
		var raycast = FindFirstRaycast(m_RaycastResultCache);
		leftData.pointerCurrentRaycast = raycast;
		m_RaycastResultCache.Clear();

		// copy the apropriate data into right and middle slots
		PointerEventData rightData;
		GetPointerData(kMouseRightId, out rightData, true);
		CopyFromTo(leftData, rightData);
		rightData.button = PointerEventData.InputButton.Right;

		PointerEventData middleData;
		GetPointerData(kMouseMiddleId, out middleData, true);
		CopyFromTo(leftData, middleData);
		middleData.button = PointerEventData.InputButton.Middle;

        
        //framePressStateLeft = inputDevice.getLeftButtonState(); //TODO entfernen
        m_MouseState.SetButtonState (PointerEventData.InputButton.Left, inputDevice.getLeftButtonState(), leftData);
        m_MouseState.SetButtonState (PointerEventData.InputButton.Right, inputDevice.getRightButtonState(), rightData);
		m_MouseState.SetButtonState (PointerEventData.InputButton.Middle, inputDevice.getMiddleButtonState(), middleData);

        //trigger event
        if(inputDevice.getLeftButtonState() == PointerEventData.FramePressState.Pressed)
        {
            //InputEventSystem.triggerEventOnLayer(InputEventSystem.Event.INPUTDEVICE_LeftButtonPressed, inputDevice.getRaycastHit().transform.gameObject.layer, inputDevice.getRaycastHit());
        }
        if (inputDevice.getLeftButtonState() == PointerEventData.FramePressState.Released)
        {
            //InputEventSystem.triggerEventOnLayer(InputEventSystem.Event.INPUTDEVICE_LeftButtonReleased, inputDevice.getRaycastHit().transform.gameObject.layer, inputDevice.getRaycastHit());
        }

        return m_MouseState;
	}

}
