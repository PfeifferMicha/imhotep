using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/*! Handles input using the controllers or mouse.
 * This module sends raycasts into the main scene. If the UI Mesh is hit, it also sends raycasts into the UI scene.
 * Once done, it will raise any necessary events (mouse enter, mouse exit, pointer click etc.) */
public class HierarchicalInputModule : BaseInputModule {

	private GameObject activeGameObject = null;
	private GameObject previousActiveGameObject = null;

	private LineRenderer lineRenderer;

	public void Start()
	{
		lineRenderer = this.GetComponent<LineRenderer>();
	}

	//! Called every frame by the event system
	public override void UpdateModule()
	{
		previousActiveGameObject = activeGameObject;
		activeGameObject = null;

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

				// 2. If the UI Mesh was hit, check if the mouse is actually over a UI element:
				if (result.transform.gameObject == Platform.instance.UIMesh) {
					RaycastResult resultUI;
					if (ProcessUIRaycast (result.textureCoord, out resultUI)) {
						activeGameObject = resultUI.gameObject;
						lineRenderer.SetPosition (1, result.point);
					} else {
						// 3. If no UI element was hit, raycast again but ignore the UIMesh:
						layerMask = ~ LayerMask.NameToLayer( "UIMesh" );
						if (Physics.Raycast (ray, out result, Mathf.Infinity, layerMask)) {
							activeGameObject = result.transform.gameObject;
							lineRenderer.SetPosition (1, result.point);
						}
					}
				}
			}

			// If nothing was hit at all, just show a very long ray:
			if (activeGameObject == null) {
				lineRenderer.SetPosition (1, ray.origin + ray.direction*300f);
			}
		}
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
		//if (m_TargetObject == null)
		//	return;
		if (previousActiveGameObject != activeGameObject) {
			if( previousActiveGameObject != null )
				ExecuteEvents.ExecuteHierarchy (previousActiveGameObject, new PointerEventData (eventSystem), ExecuteEvents.pointerEnterHandler);
			
			if( activeGameObject != null )
				ExecuteEvents.ExecuteHierarchy (activeGameObject, new PointerEventData (eventSystem), ExecuteEvents.pointerEnterHandler);
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
		// Don't care about the pointer ID (we only have one pointer):
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
