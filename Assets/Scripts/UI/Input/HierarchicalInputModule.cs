using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UI;

/*! Handles input using the controllers or mouse.
 * This module sends raycasts into the main scene. If the UI Mesh is hit, it also sends raycasts into the UI scene.
 * Once done, it will raise any necessary events (mouse enter, mouse exit, pointer click etc.) */
public class HierarchicalInputModule : BaseInputModule {

	private GameObject activeGameObject = null;
	private GameObject previousActiveGameObject = null;

	private LineRenderer lineRenderer;

	private CustomEventData eventData;

	private Vector2 lastTextureCoord;
	private Vector3 lastHitWorldPos;

	public void Start()
	{
		lineRenderer = this.GetComponent<LineRenderer>();

		eventData = new CustomEventData (eventSystem);
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
			RaycastHit result;
			int layerMask = ~ LayerMask.NameToLayer( "UI" ); // Everything but UI Elements (but UIMesh could be hit!)
			if (Physics.Raycast (ray, out result, Mathf.Infinity, layerMask)) {

				activeGameObject = result.transform.gameObject;
				hitTextureCoord = result.textureCoord;
				hitWorldPos = result.point;

				// 2. If the UI Mesh was hit, check if the mouse is actually over a UI element:
				if (result.transform.gameObject == Platform.instance.UIMesh) {
					RaycastResult resultUI;
					if (ProcessUIRaycast (result.textureCoord, out resultUI)) {
						activeGameObject = resultUI.gameObject;
						lineRenderer.SetPosition (1, result.point);
						hitWorldPos = resultUI.worldPosition;
					} else {
						// 3. If no UI element was hit, raycast again but ignore the UIMesh:
						layerMask = ~ LayerMask.NameToLayer( "UIMesh" );
						if (Physics.Raycast (ray, out result, Mathf.Infinity, layerMask)) {
							activeGameObject = result.transform.gameObject;
							lineRenderer.SetPosition (1, result.point);

							hitTextureCoord = result.textureCoord;
							hitWorldPos = result.point;
						}
					}
				}
			}

			// If nothing was hit at all, just show a very long ray:
			if (activeGameObject == null) {
				lineRenderer.SetPosition (1, ray.origin + ray.direction*300f);
			}

			if (previousActiveGameObject == previousActiveGameObject) {
				eventData.delta = hitTextureCoord - lastTextureCoord;
				eventData.delta3D = hitWorldPos - lastHitWorldPos;
			} else {
				eventData.delta = Vector2.zero;
				eventData.delta3D = Vector3.zero;
			}
		}

		lastTextureCoord = hitTextureCoord;
		lastHitWorldPos = hitWorldPos;
	}

	//! Check into UI scene to see if the ray at rayOrigin would hit anything:
	private bool ProcessUIRaycast( Vector2 screenPoint, out RaycastResult result )
	{
		Camera uiCamera = UI.Core.instance.UICamera;
		Vector3 rayOrigin = new Vector3 (
			screenPoint.x * uiCamera.targetTexture.width,
			screenPoint.y * uiCamera.targetTexture.height,
			0 );
		Ray uiRay = UI.Core.instance.UICamera.ScreenPointToRay ( rayOrigin );

		int uiLayer = LayerMask.NameToLayer ("UI");

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
		InputDeviceManager idm = InputDeviceManager.instance;

		// Get a list of buttons from the input module, which tells me if buttons are pressed, released or not changed:
		ButtonInfo buttonInfo;
		if (idm.currentInputDevice != null) {
			buttonInfo = idm.currentInputDevice.updateButtonInfo ();
		} else {
			buttonInfo = new ButtonInfo ();
		}

		if (previousActiveGameObject != activeGameObject) {
			if( previousActiveGameObject != null )
				ExecuteEvents.ExecuteHierarchy (previousActiveGameObject, eventData, ExecuteEvents.pointerExitHandler);
			
			if( activeGameObject != null )
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerEnterHandler);
		}

		// ----------------------------------
		// Fill the EventData with current information from the last hit:
		eventData.scrollDelta = idm.currentInputDevice.getScrollDelta();
		//eventData.delta = 

		// ----------------------------------
		// Handle left click:

		// If we just pressed the button, remember on which object we pressed it (activeGameObject could be null!)
		// so that we can send a click event when releaseing the button over the same object:
		if(buttonInfo.buttonStates [ButtonType.Left] == PointerEventData.FramePressState.Pressed)
		{
			eventData.pointerPress = activeGameObject;
			// Also stop any selection:
			EventSystem.current.SetSelectedGameObject( null );
		}
		
		if (activeGameObject) {
			eventData.button = PointerEventData.InputButton.Left;
			if (buttonInfo.buttonStates [ButtonType.Left] == PointerEventData.FramePressState.PressedAndReleased) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
			} else if (buttonInfo.buttonStates [ButtonType.Left] == PointerEventData.FramePressState.Pressed) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerDownHandler);
				eventData.pointerPress = activeGameObject;
			} else if (buttonInfo.buttonStates [ButtonType.Left] == PointerEventData.FramePressState.Released) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerUpHandler);
				// If the current object receiving the pointerUp event is also the one which received the
				// pointer down event, this results in a click!
				if (eventData.pointerPress == activeGameObject) {
					ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
				}
			}
		}


		// ----------------------------------
		// Handle right click:
		if (activeGameObject) {
			eventData.button = PointerEventData.InputButton.Right;
			if (buttonInfo.buttonStates [ButtonType.Right] == PointerEventData.FramePressState.PressedAndReleased) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
			} else if (buttonInfo.buttonStates [ButtonType.Right] == PointerEventData.FramePressState.Pressed) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerDownHandler);
				eventData.pointerPress = activeGameObject;
			} else if (buttonInfo.buttonStates [ButtonType.Right] == PointerEventData.FramePressState.Released) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerUpHandler);
				// If the current object receiving the pointerUp event is also the one which received the
				// pointer down event, this results in a click!
				if (eventData.pointerPress == activeGameObject) {
					ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
				}
			}
		}

		// ----------------------------------
		// Handle middle click:
		if (activeGameObject) {
			eventData.button = PointerEventData.InputButton.Middle;
			if (buttonInfo.buttonStates [ButtonType.Middle] == PointerEventData.FramePressState.PressedAndReleased) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
			} else if (buttonInfo.buttonStates [ButtonType.Middle] == PointerEventData.FramePressState.Pressed) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerDownHandler);
				eventData.pointerPress = activeGameObject;
			} else if (buttonInfo.buttonStates [ButtonType.Middle] == PointerEventData.FramePressState.Released) {
				ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerUpHandler);
				// If the current object receiving the pointerUp event is also the one which received the
				// pointer down event, this results in a click!
				if (eventData.pointerPress == activeGameObject) {
					ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.pointerClickHandler);
				}
			}
		}


		// ----------------------------------
		// Handle scroll:

		if (eventData.scrollDelta != Vector2.zero) {
			ExecuteEvents.ExecuteHierarchy(activeGameObject, eventData, ExecuteEvents.scrollHandler);
			//ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, ExecuteEvents.scrollHandler);
		}
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
}
