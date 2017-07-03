// Model Zoomer 
//

using System;
using UnityEngine;

public class ModelZoomer : MonoBehaviour
{


    public float zoomingSpeed = 1;
    public float maxZoom = 2f;
    public float minZoom = 0.2f;

	private Vector3 targetZoom;
	private Vector3 zoomVelocity;

	public float autoZoomSpeed = 0.5f;
	private float scaleTime = 0.3f;

	private bool zooming = false;
	private float originalDist = 0;
	private Vector3 mOriginalZoom;

    private void Start()
    {
		targetZoom = transform.localScale;
    }


    private void Update()
    {
		if (UI.Core.instance.layoutSystem.activeScreen == UI.Screen.center) {

			InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
			if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.Mouse) {
				// Let mouse handle zooming:
				if (!UI.Core.instance.pointerIsOverPlatformUIObject) {
					if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
							
						float inputScroll = Input.GetAxis ("Mouse ScrollWheel");

						float zoom = transform.localScale.x + inputScroll / (1 / zoomingSpeed);

						zoom = Mathf.Clamp (zoom, minZoom, maxZoom);

						transform.localScale = new Vector3 (zoom, zoom, zoom);
						targetZoom = transform.localScale;
					}
				}
			} else if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {

					// Let left Vive controller handle zooming:
					LeftController lc = InputDeviceManager.instance.leftController;
					if (lc != null) {
						UnityEngine.EventSystems.PointerEventData.FramePressState triggerState = lc.triggerButtonState;
						if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Pressed && zooming == false) {
							zooming = true;
							originalDist = (lc.transform.position - transform.position).magnitude;
							mOriginalZoom = transform.localScale;
						} else if (triggerState == UnityEngine.EventSystems.PointerEventData.FramePressState.Released && zooming == true) {
							zooming = false;
						}

						if (zooming) {

							float dist = (lc.transform.position - transform.position).magnitude;

							float distDiff = dist - originalDist;

							Vector3 newScale = mOriginalZoom + mOriginalZoom * distDiff;

							setTargetZoom (newScale);
						}
					}

				}
		}

		// Auto-Zoom to target, if given:
		transform.localScale = Vector3.SmoothDamp(transform.localScale, targetZoom, ref zoomVelocity, scaleTime);
    }

	public void setTargetZoom( Vector3 zoom, float timeForScaling = 0f )
	{
		targetZoom = zoom;
		zoomVelocity = new Vector3 (0, 0, 0);
		if (timeForScaling == 0) {
			scaleTime = 1f;
			transform.localScale = targetZoom;
		} else {
			scaleTime = timeForScaling;
		}
	}
}


