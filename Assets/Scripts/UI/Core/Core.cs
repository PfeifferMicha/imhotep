using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
	public class Core : MonoBehaviour
	{
		public bool pointerIsOverUIObject{ private set; get; }

		public static Core instance { private set; get; }

		public Camera UICamera;

		public LayoutSystem layoutSystem { private set; get; }

		//! The scale with which all the UI elements are scaled (i.e. pixels to meters)
		public float UIScale = 0.0025f;
		public float pixelsPerMeter = 500f;
		public float aspectRatio = 1f;

		public Color TabHighlightColor;
		public Color ButtonBaseColor;

		public Sprite normalTabImage;
		public Sprite selectedTabImage;

		public Core()
		{
			instance = this;
		}

		public void OnEnable()
		{
			layoutSystem = new LayoutSystem ();

			//PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Loaded, showPatientDefaultUI );
			//PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, hidePatientDefaultUI );
		}
		public void OnDisable()
		{
			//PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Loaded, showPatientDefaultUI);
			//PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Closed, hidePatientDefaultUI);
		}

		public void setPointerIsOnUI( bool onUI )
		{
			pointerIsOverUIObject = onUI;
		}

		public void setCamera( Camera cam )
		{
			UICamera = cam;
			aspectRatio = (float)UICamera.targetTexture.width / (float)UICamera.targetTexture.height;
			layoutSystem.setCamera (cam);
		}

		public Widget getWidgetByName( string name )
		{
			foreach (Transform child in transform) {
				if (child.name == name) {
					if (child.GetComponent<Widget> () != null) {
						return child.GetComponent<Widget> ();
					}
				}
			}
			return null;
		}

		public void selectTab( Button b )
		{
			ColorBlock colors = b.colors;
			colors.normalColor = TabHighlightColor;
			b.colors = colors;
			b.image.sprite = selectedTabImage;
		}

		public void unselectTab( Button b )
		{
			ColorBlock colors = b.colors;
			colors.normalColor = ButtonBaseColor;
			b.colors = colors;
			b.image.sprite = normalTabImage;
		}
    }
}
