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

    private void Start()
    {
		targetZoom = transform.localScale;
    }


    private void Update()
    {
		if (UI.UICore.instance.mouseIsOverUIObject == false) {
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {

				//TODO make movement smooth

				float inputScroll = Input.GetAxis ("Mouse ScrollWheel");

				float zoom = transform.localScale.x + inputScroll / (1 / zoomingSpeed);

				zoom = Mathf.Clamp (zoom, minZoom, maxZoom);

				transform.localScale = new Vector3 (zoom, zoom, zoom);
				targetZoom = transform.localScale;
			}
		}

		// Auto-Zoom to target, if given:
		transform.localScale = Vector3.SmoothDamp(transform.localScale, targetZoom, ref zoomVelocity, scaleTime);
    }

	public void setTargetZoom( Vector3 zoom, float timeForScaling = 0.6f )
	{
		targetZoom = zoom;
		zoomVelocity = new Vector3 (0, 0, 0);
		scaleTime = timeForScaling;
	}
}


