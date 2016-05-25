// Model Zoomer 
//

using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class ModelZoomer : MonoBehaviour
    {


        public float zoomingSpeed = 1;
        public float maxZoom = 2f;
        public float minZoom = 0.2f;


        private float m_TargetZoom;
        private float m_OriginalZoom;
       

        private void Start()
        {
            m_OriginalZoom = transform.localScale.x;
        }


        private void Update()
        {

			if (UI.UICore.instance.mouseIsOverUIObject == false) {
				if (Input.GetAxis ("Mouse ScrollWheel") != 0) {

					//TODO make movement smooth

					float inputScroll = Input.GetAxis ("Mouse ScrollWheel");

					m_OriginalZoom = transform.localScale.x;

					m_TargetZoom = m_OriginalZoom + inputScroll / (1 / zoomingSpeed);

					if (m_TargetZoom > maxZoom) {
						m_TargetZoom = maxZoom;
					}
					if (m_TargetZoom < minZoom) {
						m_TargetZoom = minZoom;
					}

					transform.localScale = new Vector3 (m_TargetZoom, m_TargetZoom, m_TargetZoom);

             
				}
			}
        }
    }
}


