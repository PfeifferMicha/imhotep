using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class UICore : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		// Color which all buttons will turn when clicked:
		public Color ButtonPressedColor = Color.white;

		public bool mouseIsOverUIObject{ private set; get; }

		public static UICore instance { private set; get; }

		public Camera UICamera;
		public GameObject[] AvailableWidgets;

		public UICore()
		{
			instance = this;
		}

		public void OnEnable()
		{
			new LayoutSystem ();

			PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Loaded, showPatientDefaultUI );
			PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, hidePatientDefaultUI );
		}
		public void OnDisable()
		{
			PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Loaded, showPatientDefaultUI);
			PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Closed, hidePatientDefaultUI);
		}

		public void showPatientDefaultUI( object obj )
		{
			List<string> widgetsToEnable = new List<string> ();
			widgetsToEnable.Add ("Dicom Viewer");
			widgetsToEnable.Add ("View Control");
			widgetsToEnable.Add ("Patient Briefing");

			foreach (string name in widgetsToEnable) {
				GameObject widget = findAvailableWidget (name);
				if (widget != null) {
					Debug.Log (name);
					widget.SetActive (true);
				}
			}
		}
		public void hidePatientDefaultUI( object obj )
		{
			// TODO
		}
		private GameObject findAvailableWidget( string name )
		{
			foreach (GameObject go in AvailableWidgets) {
				if (go.name == name) {
					return go;
				}
			}
			return null;
		}

		public void OnPointerEnter(PointerEventData dataName)
		{
			mouseIsOverUIObject = true;
		}
		public void OnPointerExit(PointerEventData dataName)
		{
			mouseIsOverUIObject = false;
		}

		public Color getHighlightColorFor( Color baseCol )
		{
			return baseCol * 1.1f;
		}

		public Color getPressedColorFor( Color baseCol )
		{
			return ButtonPressedColor;
		}
    }
}
