using UnityEngine;
using UnityEngine.EventSystems;

/* This extends the StandaloneInputModule, to place the mouse position at a "fake"
 * position, i.e. on the UI camera space rather than using the "global" screen pointer.
 * Script adapted from:
 * http://forum.unity3d.com/threads/fake-mouse-position-in-4-6-ui-answered.283748/
 * */

public class MouseInputModule : StandaloneInputModule {

	private Camera UICamera;
	private Mouse3DMovement mMouse;
	private Vector2 mTextureSize;

	public GameObject controllerLeft;
	public GameObject controllerRight;

	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	private bool triggerPressedPrev = false;

	public new void Start()
	{
		mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> ();
		UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();
		mTextureSize.x = UICamera.targetTexture.width;
		mTextureSize.y = UICamera.targetTexture.height;
		trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject> ();
	}

	// This is the real function we want, the two commented out lines (Input.mousePosition)
	// are replaced with m_cursorPos (our fake mouse position, set with the public function,
	// UpdateCursorPosition)
	private readonly MouseState m_MouseState = new MouseState();
	protected override MouseState GetMousePointerEventData( int id = 0 )
	{
		// convert to a Screen space position:
		Vector2 cursorPos = mMouse.getUVCoordinates();
		cursorPos.x *= mTextureSize.x;
		cursorPos.y *= mTextureSize.y;

		//MouseState m = new MouseState();

		// Populate the left button...
		PointerEventData leftData;
		var created = GetPointerData( kMouseLeftId, out leftData, true );

		leftData.Reset();

		if (created)
			leftData.position = cursorPos;
		//leftData.position = Input.mousePosition;

		//Vector2 pos = Input.mousePosition;
		Vector2 pos = cursorPos;
		leftData.delta = pos - leftData.position;
		leftData.position = pos;
		leftData.scrollDelta = Input.mouseScrollDelta;
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

		PointerEventData.FramePressState triggerState = PointerEventData.FramePressState.NotChanged;
		bool triggerPressed = controller.GetPressDown (triggerButton);

		if (triggerPressed == triggerPressedPrev) {
			triggerState = PointerEventData.FramePressState.NotChanged;
		} else {
			if (triggerPressed) {
				triggerState = PointerEventData.FramePressState.Pressed;
			} else {
				triggerState = PointerEventData.FramePressState.Released;
			}
		}
		triggerPressedPrev = triggerPressed;

		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);

		//m_MouseState.SetButtonState(PointerEventData.InputButton.Left, triggerState, leftData);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

		//Debug.Log( m_MouseState.GetButtonState (PointerEventData.InputButton.Left) );

		//Debug.Log (triggerState);
		return m_MouseState;
	}

}
