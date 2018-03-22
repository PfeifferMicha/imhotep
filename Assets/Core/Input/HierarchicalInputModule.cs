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

	private LayerMask layerMask;

	private Vector2 lastTextureCoord;
	private Vector3 lastHitWorldPos;
	//! Position on UI camera (in pixels!):
	private Vector2 fakeUIScreenPosition;

	private RaycastHit raycastHit;
	private RaycastResult raycastResult;

	private bool isPointerOverUI = false;
	private bool isPointerOverPlatformUI = false;

	public CustomEventData getPointerData()
	{
		return eventData;
	}

	protected override void Start()
	{
		lineRenderer = this.GetComponent<LineRenderer>();

		eventData = new CustomEventData (eventSystem);
		leftData = new CustomEventData (eventSystem);
		rightData = new CustomEventData (eventSystem);
		middleData = new CustomEventData (eventSystem);
		triggerData = new CustomEventData (eventSystem);

		//set Layer Mask on Default
		resetLayerMask ();
	}

	public void resetLayerMask() {
		//Sets every Layer to 1
		//activates every Layer
		layerMask = -1;

	}

	public void disableLayer(string layer) {
		//Disables the Given Layer
		int l = LayerMask.NameToLayer (layer);
		layerMask.value &= ~(1 << l);
	}

	public void enableLayer(string layer) {
		//Enables the given Layer
		int l = LayerMask.NameToLayer (layer);
		layerMask.value |= (1 << l);
	}

	//! Called every frame by the event system
	public override void UpdateModule()
	{
		previousActiveGameObject = activeGameObject;
		activeGameObject = null;

		Vector2 hitTextureCoord = Vector2.zero;
		int hitTriangleIndex = -1;
		Vector3 hitWorldPos = Vector3.zero;

		isPointerOverUI = false;
		isPointerOverPlatformUI = false;

		InputDeviceManager idm = InputDeviceManager.instance;
		if( idm.currentInputDevice != null )
		{
			// Get a ray from the current input device (mouse or controller):
			Ray ray = idm.currentInputDevice.createRay ();
			lineRenderer.SetPosition (0, ray.origin);


			// 1. First, cast this ray into the scene:
			int layerMaskLocal = ~(1 << LayerMask.NameToLayer ("UI")); // Everything but UI Elements (but UIMesh could be hit!)
			//apply global layer changes to LayerMask used for raycast
			layerMaskLocal &= layerMask.value;
			if (Physics.Raycast (ray, out raycastHit, Mathf.Infinity, layerMaskLocal)) {

				activeGameObject = raycastHit.transform.gameObject;
				hitTextureCoord = raycastHit.textureCoord;
				hitWorldPos = raycastHit.point;
				hitTriangleIndex = raycastHit.triangleIndex;
			
				lineRenderer.SetPosition (1, raycastHit.point);

				// 2. If the UI Mesh was hit, check if the mouse is actually over a UI element:
				if (raycastHit.transform.gameObject == Platform.instance.UIMesh) {
					
					if (UIRaycast (raycastHit.textureCoord, out raycastResult)) {
						activeGameObject = raycastResult.gameObject;
						lineRenderer.SetPosition (1, raycastHit.point);
						hitWorldPos = raycastResult.worldPosition;
						isPointerOverUI = true;
						isPointerOverPlatformUI = true;
						Vector2 localPoint;
						RectTransform rt = activeGameObject.GetComponent<RectTransform> ();
						RectTransformUtility.ScreenPointToLocalPointInRectangle ( rt,
							raycastResult.screenPosition, UI.Core.instance.UICamera, out localPoint);

						hitTextureCoord = new Vector2 ((localPoint.x - rt.rect.min.x) / rt.rect.width,
							(localPoint.y - rt.rect.min.y) / rt.rect.height);
						hitTriangleIndex = raycastHit.triangleIndex;

					} else {
						// 3. If no UI element was hit, raycast again but ignore the UIMesh:
						layerMaskLocal =  ~(1 << LayerMask.NameToLayer ("UIMesh"));
						layerMaskLocal &= layerMask.value;
						if (Physics.Raycast (ray, out raycastHit, Mathf.Infinity, layerMaskLocal)) {
							activeGameObject = raycastHit.transform.gameObject;
							lineRenderer.SetPosition (1, raycastHit.point);

							hitTextureCoord = raycastHit.textureCoord;
							hitTriangleIndex = raycastHit.triangleIndex;
							hitWorldPos = raycastHit.point;
						} else {
							activeGameObject = null;
						}
					}
				}

				if (raycastHit.transform != null) {		// If any 3D object was hit
					if (raycastHit.transform.gameObject.layer == LayerMask.NameToLayer ("UITool") ||
						raycastHit.transform.gameObject.layer == LayerMask.NameToLayer ("UIOrgans") ||
						raycastHit.transform.gameObject.layer == LayerMask.NameToLayer ("UIAnnotationEdit") ) {		// If the hit Object was a UI element
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
								data.pointerCurrentRaycast = raycastResult;
								isPointerOverUI = true;
								if (raycastHit.transform.gameObject.layer == LayerMask.NameToLayer ("UITool")) {
									isPointerOverPlatformUI = true;
								}
							}
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
			idm.currentInputDevice.setTexCoordDelta (eventData.delta);
			idm.currentInputDevice.set3DDelta (eventData.delta3D);
		}

		//Debug.Log ("Active: " + activeGameObject + " previous: " + previousActiveGameObject);

		lastTextureCoord = hitTextureCoord;
		lastHitWorldPos = hitWorldPos;
		eventData.textureCoord = hitTextureCoord;
		eventData.hitTriangleIndex = hitTriangleIndex;
	}

	//! Check into UI scene to see if the ray at rayOrigin would hit anything:
	private bool UIRaycast( Vector2 screenPoint, out RaycastResult result )
	{
		Camera uiCamera = UI.Core.instance.UICamera;
		fakeUIScreenPosition = new Vector2 (
			screenPoint.x * uiCamera.targetTexture.width,
			screenPoint.y * uiCamera.targetTexture.height);
		//Ray uiRay = UI.Core.instance.UICamera.ScreenPointToRay ( rayOrigin );

		//int uiLayer = LayerMask.NameToLayer ("UI");

		PointerEventData data = new PointerEventData (EventSystem.current);
		data.position = new Vector2 (fakeUIScreenPosition.x, fakeUIScreenPosition.y);
		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll( data, raycastResults );
		if (raycastResults.Count > 0) {
			int UILayer = LayerMask.NameToLayer ("UI");
			foreach (RaycastResult r in raycastResults) {
				if( r.gameObject != null && r.gameObject.layer == UILayer) {
					result = r;
					return true;
				}
			}
		}
		result = new RaycastResult ();
		return false;
	}

	//! Called every frame after UpdateModule (but only if this module is active!)
	public override void Process()
	{
		UI.Core.instance.setPointerIsOnUI (isPointerOverUI);
		UI.Core.instance.setPointerIsOnPlatformUI (isPointerOverPlatformUI);

		SendUpdateEventToSelectedObject();

		InputDeviceManager idm = InputDeviceManager.instance;

		// Get a list of buttons from the input module, which tells me if buttons are pressed, released or not changed:
		ButtonInfo buttonInfo;
		if (idm.currentInputDevice != null) {
			buttonInfo = idm.currentInputDevice.updateButtonInfo ();
		} else {
			buttonInfo = new ButtonInfo ();
		}


		// ----------------------------------
		// Fill the EventData with current information from the last hit:
		eventData.scrollDelta = idm.currentInputDevice.getScrollDelta();
		if (InputDeviceManager.instance.currentInputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController)
			eventData.scrollDelta *= 100;
		//eventData.position = fakeUIScreenPosition;
		// Because we're interested in the 3D positions only, copy it over to the 2D UI hit result:
		raycastResult.worldPosition = raycastHit.point;
		raycastResult.worldNormal = raycastHit.normal;
		eventData.pointerCurrentRaycast = raycastResult;
		eventData.position = raycastResult.screenPosition;

		HandlePointerExitAndEnter (eventData, activeGameObject);
		if (activeGameObject != null) {
			ExecuteEvents.ExecuteHierarchy (activeGameObject, eventData, CustomEvents.pointerHoverHandler);
		}


		CopyFromTo (eventData, leftData);
		CopyFromTo (eventData, rightData);
		CopyFromTo (eventData, middleData);
		CopyFromTo (eventData, triggerData);
		leftData.button = PointerEventData.InputButton.Left;
		rightData.button = PointerEventData.InputButton.Right;
		middleData.button = PointerEventData.InputButton.Middle;
		triggerData.button = PointerEventData.InputButton.Left;		// Treat trigger like a left click!


		// Stop any selection if anything was pressed:
		//if( AnyPressed( buttonInfo ) )
		//{
			//EventSystem.current.SetSelectedGameObject( null, eventData );
		//}

		// ----------------------------------
		// Handle left click:
		//if (activeGameObject) {
		HandleButton (ButtonType.Left, buttonInfo.buttonStates [ButtonType.Left], leftData, true);
		ProcessDrag (leftData);
		//}


		// ----------------------------------
		// Handle right click:
		//if (activeGameObject) {
		HandleButton (ButtonType.Right, buttonInfo.buttonStates [ButtonType.Right], rightData, false);
		ProcessDrag (rightData);
		//}

		// ----------------------------------
		// Handle middle click:
		//if (activeGameObject) {
		HandleButton (ButtonType.Middle, buttonInfo.buttonStates [ButtonType.Middle], middleData, false);
		ProcessDrag (middleData);
		//}


		// ----------------------------------
		// Handle trigger:
		//if (activeGameObject) {
		HandleButton (ButtonType.Trigger, buttonInfo.buttonStates [ButtonType.Trigger], triggerData, true);
		ProcessDrag (triggerData);
		//}


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
		GameObject currentOverGo = activeGameObject;

		// PointerDown notification
		if (buttonState == PointerEventData.FramePressState.Pressed || buttonState == PointerEventData.FramePressState.PressedAndReleased )
		{
			eventData.eligibleForClick = true;
			eventData.delta = Vector2.zero;
			eventData.dragging = false;
			eventData.useDragThreshold = true;
			eventData.pressPosition = eventData.position;
			eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

			DeselectIfSelectionChanged(currentOverGo, eventData);

			// search for the control that will receive the press
			// if we can't find a press handler set the press
			// handler to be what would receive a click.
			var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.pointerDownHandler);

			// didnt find a press handler... search for a click handler
			if (newPressed == null)
				newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

			// Debug.Log("Pressed: " + newPressed);

			float time = Time.unscaledTime;

			if (newPressed == eventData.lastPress)
			{
				var diffTime = time - eventData.clickTime;
				if (diffTime < 0.3f)
					++eventData.clickCount;
				else
					eventData.clickCount = 1;

				eventData.clickTime = time;
			}
			else
			{
				eventData.clickCount = 1;
			}

			eventData.pointerPress = newPressed;
			eventData.rawPointerPress = currentOverGo;

			eventData.clickTime = time;

			// Save the drag handler as well
			eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

			if (eventData.pointerDrag != null)
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
		}

		// PointerUp notification
		if (buttonState == PointerEventData.FramePressState.Released || buttonState == PointerEventData.FramePressState.PressedAndReleased )
		{
			// Debug.Log("Executing pressup on: " + pointer.pointerPress);
			ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

			// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

			// see if we mouse up on the same element that we clicked on...
			var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

			// PointerClick and Drop events
			if (eventData.pointerPress == pointerUpHandler && eventData.eligibleForClick)
			{
				ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
			}
			else if (eventData.pointerDrag != null && eventData.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.dropHandler);
			}

			eventData.eligibleForClick = false;
			eventData.pointerPress = null;
			eventData.rawPointerPress = null;

			if (eventData.pointerDrag != null && eventData.dragging)
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);

			eventData.dragging = false;
			eventData.pointerDrag = null;

			// redo pointer enter / exit to refresh state
			// so that if we moused over somethign that ignored it before
			// due to having pressed on something else
			// it now gets it.
			if (currentOverGo != eventData.pointerEnter)
			{
				HandlePointerExitAndEnter(eventData, null);
				HandlePointerExitAndEnter(eventData, currentOverGo);
			}
		}
	}

	protected virtual void ProcessDrag(PointerEventData pointerEvent)
	{
		bool moving = pointerEvent.IsPointerMoving();

		if (moving && pointerEvent.pointerDrag != null
			&& !pointerEvent.dragging
			&& ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
		{
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
			pointerEvent.dragging = true;
		}

		// Drag notification
		if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
		{
			// Before doing drag we should cancel any pointer down state
			// And clear selection!
			if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
			}
			ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
		}
	}
	private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
	{
		if (!useDragThreshold)
			return true;

		return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
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
		@to.textureCoord = @from.textureCoord;
		@to.hitTriangleIndex = @from.hitTriangleIndex;
	}

	protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
	{
		// Selection tracking
		var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
		// if we have clicked something new, deselect the old thing
		// leave 'selection handling' up to the press event though.
		if (selectHandlerGO != eventSystem.currentSelectedGameObject)
			eventSystem.SetSelectedGameObject(null, pointerEvent);
	}
}
