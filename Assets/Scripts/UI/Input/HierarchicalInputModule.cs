using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UI;

/*! Handles input using the controllers or mouse.
 * This module sends raycasts into the main scene. If the UI Mesh is hit, it also sends raycasts into the UI scene.
 * Once done, it will raise any necessary events (mouse enter, mouse exit, pointer click etc.)
 * Note: This system treats the Vive Controller's trigger just like a left-click! */
public class HierarchicalInputModule : BaseInputModule {

	private GameObject activeGameObject = null;
	private GameObject previousActiveGameObject = null;

	private LineRenderer lineRenderer;

	private CustomEventData eventData;

	CustomEventData leftData;
	CustomEventData rightData;
	CustomEventData middleData;
	CustomEventData triggerData;

	private Vector2 lastTextureCoord;
	private Vector3 lastHitWorldPos;
	//! Position on UI camera (in pixels!):
	private Vector2 fakeUIScreenPosition;

	private RaycastHit raycastHit;
	private RaycastResult raycastResult;

	public void Start()
	{
		lineRenderer = this.GetComponent<LineRenderer>();

		eventData = new CustomEventData (eventSystem);
		leftData = new CustomEventData (eventSystem);
		rightData = new CustomEventData (eventSystem);
		middleData = new CustomEventData (eventSystem);
		triggerData = new CustomEventData (eventSystem);
	}

	//! Called every frame by the event system
	public override void UpdateModule()
	{
		previousActiveGameObject = activeGameObject;
		activeGameObject = null;

		Vector2 hitTextureCoord = Vector2.zero;
		Vector3 hitWorldPos = Vector3.zero;

		InputDeviceManager idm = InputDeviceManager.instance;
		if( idm.currentInputDevice != null )
		{
			// Get a ray from the current input device (mouse or controller):
			Ray ray = idm.currentInputDevice.createRay ();
			lineRenderer.SetPosition (0, ray.origin);


			// 1. First, cast this ray into the scene:
			int layerMask = ~ (1 << LayerMask.NameToLayer( "UI" )); // Everything but UI Elements (but UIMesh could be hit!)
			if (Physics.Raycast (ray, out raycastHit, Mathf.Infinity)) {

				activeGameObject = raycastHit.transform.gameObject;
				hitTextureCoord = raycastHit.textureCoord;
				hitWorldPos = raycastHit.point;
				lineRenderer.SetPosition (1, raycastHit.point);

				// 2. If the UI Mesh was hit, check if the mouse is actually over a UI element:
				if (raycastHit.transform.gameObject == Platform.instance.UIMesh) {
					
					if (UIRaycast (raycastHit.textureCoord, out raycastResult)) {
						activeGameObject = raycastResult.gameObject;
						lineRenderer.SetPosition (1, raycastHit.point);
						hitWorldPos = raycastResult.worldPosition;
					} else {
						// 3. If no UI element was hit, raycast again but ignore the UIMesh:
						layerMask = ~ ( 1 << LayerMask.NameToLayer( "UIMesh" ) );
						if (Physics.Raycast (ray, out raycastHit, Mathf.Infinity, layerMask)) {
							activeGameObject = raycastHit.transform.gameObject;
							lineRenderer.SetPosition (1, raycastHit.point);

							hitTextureCoord = raycastHit.textureCoord;
							hitWorldPos = raycastHit.point;
						} else {
							activeGameObject = null;
						}
					}
				} else if ( raycastHit.transform.gameObject.layer == LayerMask.NameToLayer( "UITool" ) ) {
					if (raycastHit.transform.GetComponent<CanvasRaycaster> () != null) {
						RectTransform tf = raycastHit.transform.GetComponent<RectTransform> ();
						PointerEventData data = new PointerEventData (EventSystem.current);
						data.position = new Vector2 (tf.InverseTransformPoint (raycastHit.point).x, tf.InverseTransformPoint (raycastHit.point).y);
						List<RaycastResult> raycastResults = new List<RaycastResult> ();
						raycastHit.transform.GetComponent<CanvasRaycaster> ().Raycast (data, raycastResults);
						if (raycastResults.Count > 0) {
							raycastResult = raycastResults [0];
							activeGameObject = raycastResult.gameObject;
							lineRenderer.SetPosition (1, raycastHit.point);
							hitWorldPos = raycastResult.worldPosition;
						}
					}
				}
			}

			// Send end point of line:
			if (activeGameObject != null) {
				lineRenderer.SetPosition (1, raycastHit.point);
			} else {
				// If nothing was hit at all, just show a very long ray:
				lineRenderer.SetPosition (1, ray.origin + ray.direction*300f);
			}

			if (activeGameObject == previousActiveGameObject) {
				eventData.delta = hitTextureCoord - lastTextureCoord;
				eventData.delta3D = hitWorldPos - lastHitWorldPos;
			} else {
				eventData.delta = Vector2.zero;
				eventData.delta3D = Vector3.zero;
			}
		}

		//Debug.Log ("Active: " + activeGameObject + " previous: " + previousActiveGameObject);

		lastTextureCoord = hitTextureCoord;
		lastHitWorldPos = hitWorldPos;
	}

	//! Check into UI scene to see if the ray at rayOrigin would hit anything:
	private bool UIRaycast( Vector2 screenPoint, out RaycastResult result )
	{
		Camera uiCamera = UI.Core.instance.UICamera;
		fakeUIScreenPosition = new Vector2 (
			screenPoint.x * uiCamera.targetTexture.width,
			screenPoint.y * uiCamera.targetTexture.height);
		Vector3 rayOrigin = new Vector3 (
			fakeUIScreenPosition.x,
			fakeUIScreenPosition.y,
			0 );
		//Ray uiRay = UI.Core.instance.UICamera.ScreenPointToRay ( rayOrigin );

		//int uiLayer = LayerMask.NameToLayer ("UI");

		PointerEventData data = new PointerEventData (EventSystem.current);
		data.position = new Vector2 (rayOrigin.x, rayOrigin.y);
		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll( data, raycastResults );
		if (raycastResults.Count > 0) {
			result = raycastResults [0];
			return true;
		} else {
			result = new RaycastResult ();
			return false;
		}
	}

	//! Called every frame after UpdateModule (but only if this module is active!)
	public override void Process()
	{
		SendUpdateEventToSelectedObject();

		InputDeviceManager idm = InputDeviceManager.instance;

		// Get a list of buttons from the input module, which tells me if buttons are pressed, released or not changed:
		ButtonInfo buttonInfo;
		if (idm.currentInputDevice != null) {
			buttonInfo = idm.currentInputDevice.updateButtonInfo ();
		} else {
			buttonInfo = new ButtonInfo ();
		}

		if (previousActiveGameObject != activeGameObject) {
			if (previousActiveGameObject != null) {
				ExecuteEvents.ExecuteHierarchy (previousActiveGameObject, eventData, ExecuteEvents.pointerExitHandler);
				eventData.pointerEnter = null;
			}
			
			if (activeGameObject != null) {
				eventData.pointerEnter = activeGameObject;
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerEnterHandler);
			}
		}

		// ----------------------------------
		// Fill the EventData with current information from the last hit:
		eventData.scrollDelta = idm.currentInputDevice.getScrollDelta();
		eventData.position = fakeUIScreenPosition;
		eventData.pointerCurrentRaycast = raycastResult;


		CopyFromTo (eventData, leftData);
		CopyFromTo (eventData, rightData);
		CopyFromTo (eventData, middleData);
		CopyFromTo (eventData, triggerData);
		leftData.button = PointerEventData.InputButton.Left;
		rightData.button = PointerEventData.InputButton.Right;
		middleData.button = PointerEventData.InputButton.Middle;
		triggerData.button = PointerEventData.InputButton.Left;		// Treat trigger like a left click!


		// Stop any selection if anything was pressed:
		if( AnyPressed( buttonInfo ) )
		{
			EventSystem.current.SetSelectedGameObject( null, eventData );
		}

		// ----------------------------------
		// Handle left click:
		if (activeGameObject) {
			HandleButton (ButtonType.Left, buttonInfo.buttonStates [ButtonType.Left], leftData, true);
		}


		// ----------------------------------
		// Handle right click:
		if (activeGameObject) {
			HandleButton (ButtonType.Right, buttonInfo.buttonStates [ButtonType.Right], rightData, false);
		}

		// ----------------------------------
		// Handle middle click:
		if (activeGameObject) {
			HandleButton (ButtonType.Middle, buttonInfo.buttonStates [ButtonType.Middle], middleData, false);
		}


		// ----------------------------------
		// Handle trigger:
		if (activeGameObject) {
			HandleButton (ButtonType.Trigger, buttonInfo.buttonStates [ButtonType.Trigger], triggerData, true);
		}

		// ----------------------------------
		// Handle scroll:

		if (!Mathf.Approximately(eventData.scrollDelta.sqrMagnitude, 0.0f)) {
			//Debug.Log ("Scrolling: " + eventData.scrollDelta + " " + activeGameObject.name);
			//ExecuteEvents.ExecuteHierarchy(activeGameObject, eventData, ExecuteEvents.scrollHandler);

			var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler> (activeGameObject);
			ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
			//ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.scrollHandler);
		}



	}

	private void HandleButton( ButtonType buttonType, PointerEventData.FramePressState buttonState, CustomEventData eventData, bool allowDragging )
	{
		if (buttonState == PointerEventData.FramePressState.PressedAndReleased) {
			eventData.pointerPress = activeGameObject;
			eventData.pressPosition = eventData.position;
			eventData.pointerPressRaycast = raycastResult;
			ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
			eventData.pointerPress = null;
			if (allowDragging && eventData.pointerDrag != null) {
				Debug.Log ("Dragging!");
				ExecuteEvents.ExecuteHierarchy (eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
				eventData.dragging = false;
				eventData.pointerDrag = null;
			}
		} else if (buttonState == PointerEventData.FramePressState.Pressed) {
			eventData.pointerPressRaycast = raycastResult;
			eventData.pointerPress = activeGameObject;
			eventData.pressPosition = eventData.position;
			eventData.dragging = false;
			ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerDownHandler);
		} else if (buttonState == PointerEventData.FramePressState.Released) {
			// If the current object receiving the pointerUp event is also the one which received the
			// pointer down event, this results in a click!
			if (eventData.pointerPress == activeGameObject) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
			}
			if (allowDragging && eventData.pointerDrag != null) {
				ExecuteEvents.ExecuteHierarchy (eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
				eventData.dragging = false;
				eventData.pointerDrag = null;
			}
			ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerUpHandler);
			eventData.pointerPress = null;
		} else if (buttonState == PointerEventData.FramePressState.NotChanged) {
			if (allowDragging && eventData.pointerPress != null )
			{
				if (eventData.pointerDrag == null) {
					eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler> (eventData.pointerPress);
					//ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.dragHandler);

					if (eventData.pointerDrag != null) {
						ExecuteEvents.Execute (eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);

						if (!eventData.dragging) {
							ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
							eventData.dragging = true;
						}
					}
				} else {
					ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
				}
			}
		}
	}

	private bool AnyPressed( ButtonInfo buttonInfo )
	{
		foreach (KeyValuePair<ButtonType, PointerEventData.FramePressState> entry in buttonInfo.buttonStates) {
			if (entry.Value == PointerEventData.FramePressState.Pressed || entry.Value == PointerEventData.FramePressState.PressedAndReleased) {
				return true;
			}
		}
		return false;
	}

	protected bool SendUpdateEventToSelectedObject()
	{
		if (eventSystem.currentSelectedGameObject == null)
			return false;

		var data = GetBaseEventData();
		ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
		return data.used;
	}

	//! Called when this module is activated
	public override void ActivateModule()
	{
	}

	public override void DeactivateModule()
	{
	}

	public override bool IsPointerOverGameObject(int pointerId)
	{
		// Don't care about the pointer ID (we only have one pointer at the time being):
		return (activeGameObject != null);
	}

	public override bool IsModuleSupported()
	{
		// sure!
		return true;
	}

	public override bool ShouldActivateModule()
	{
		// sure!
		return true;
	}

	protected void CopyFromTo(CustomEventData @from, CustomEventData @to)
	{
		@to.position = @from.position;
		@to.delta = @from.delta;
		@to.scrollDelta = @from.scrollDelta;
		@to.pointerCurrentRaycast = @from.pointerCurrentRaycast;
		@to.pointerEnter = @from.pointerEnter;
	}
}
