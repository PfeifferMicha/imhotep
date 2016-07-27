// Model Zoomer 
//

using System;
using UnityEngine;

public class ModelZoomer : MonoBehaviour
{


    public float zoomingSpeed = 1;
    public float maxZoom = 2f;
    public float minZoom = 0.2f;


    private float m_OriginalZoom;

	private Vector3 m_TargetZoom;
	private Vector3 zoomVelocity;

	public float autoZoomSpeed = 0.5f;
	private float scaleTime = 0.3f;

    private void Start()
    {
        m_OriginalZoom = transform.localScale.x;
		m_TargetZoom = transform.localScale;
    }


    private void Update()
    {
		if (UI.UICore.instance.mouseIsOverUIObject == false) {
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {

				//TODO make movement smooth

				float inputScroll = Input.GetAxis ("Mouse ScrollWheel");

				m_OriginalZoom = transform.localScale.x;

				float zoom = m_OriginalZoom + inputScroll / (1 / zoomingSpeed);

				zoom = Mathf.Clamp (zoom, minZoom, maxZoom);

				transform.localScale = new Vector3 (zoom, zoom, zoom);
				m_TargetZoom = transform.localScale;
			}
		}

		// Auto-Zoom to target, if given:
		//float step = autoZoomSpeed*Time.deltaTime;
		//transform.localScale = Vector3.MoveTowards( transform.localScale, m_TargetZoom, step );
		transform.localScale = Vector3.SmoothDamp(transform.localScale, m_TargetZoom, ref zoomVelocity, scaleTime);
    }

	public void setTargetZoom( Vector3 zoom, float timeForScaling = 0.6f )
	{
		m_TargetZoom = zoom;
		zoomVelocity = new Vector3 (0, 0, 0);
		scaleTime = timeForScaling;
	}
}


