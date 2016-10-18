using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
	public class Core : MonoBehaviour
	{
		//! Is set to true whenever the mouse is over a UI elemnt.
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

		public GameObject indicatorLeft;
		public GameObject indicatorRight;

		//! A bar on the lower end of the screen (for close button etc.)
		public GameObject statusBar;

		private List<GameObject> activeIndicators = new List<GameObject> ();
		private int notificationID = 0;



		public Core()
		{
			instance = this;
		}

		public void OnEnable()
		{
			layoutSystem = new LayoutSystem ();

			indicatorLeft.SetActive (false);
			indicatorRight.SetActive (false);

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

			// Adjust position/size of the statusbar:
			Rect newRect = layoutSystem.getStatusBarPosition ();
			RectTransform widgetRect = statusBar.GetComponent<RectTransform> ();
			widgetRect.localPosition = newRect.position;
			widgetRect.sizeDelta = newRect.size;
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

		public int addIndication( UI.Screen screen, string message )
		{
			if (activeIndicators.Count > 0) {
				clearNotification (notificationID);
			}

			if (screen == UI.Screen.left) {
				GameObject indicator1 = Instantiate (indicatorLeft) as GameObject;
				indicator1.GetComponent<Widget> ().layoutScreen = UI.Screen.right;
				indicator1.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator1.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator1.SetActive (true);
				Text t = indicator1.GetComponentInChildren<Text> ();
				t.text = message;
				indicator1.transform.SetParent (transform, false);
				activeIndicators.Add (indicator1);

				GameObject indicator2 = Instantiate (indicatorLeft) as GameObject;
				indicator2.GetComponent<Widget> ().layoutScreen = UI.Screen.center;
				indicator2.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator2.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator2.SetActive (true);
				t = indicator2.GetComponentInChildren<Text> ();
				t.text = message;
				indicator2.transform.SetParent (transform, false);
				activeIndicators.Add (indicator2);
			} else if (screen == UI.Screen.center) {
				GameObject indicator1 = Instantiate (indicatorLeft) as GameObject;
				indicator1.GetComponent<Widget> ().layoutScreen = UI.Screen.right;
				indicator1.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator1.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator1.SetActive (true);
				Text t = indicator1.GetComponentInChildren<Text> ();
				t.text = message;
				indicator1.transform.SetParent (transform, false);
				activeIndicators.Add (indicator1);

				GameObject indicator2 = Instantiate (indicatorRight) as GameObject;
				indicator2.GetComponent<Widget> ().layoutScreen = UI.Screen.left;
				indicator2.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator2.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator2.SetActive (true);
				t = indicator2.GetComponentInChildren<Text> ();
				t.text = message;
				indicator2.transform.SetParent (transform, false);
				activeIndicators.Add (indicator2);
			} else if (screen == UI.Screen.right) {
				GameObject indicator1 = Instantiate (indicatorRight) as GameObject;
				indicator1.GetComponent<Widget> ().layoutScreen = UI.Screen.left;
				indicator1.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator1.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator1.SetActive (true);
				Text t = indicator1.GetComponentInChildren<Text> ();
				t.text = message;
				indicator1.transform.SetParent (transform, false);
				activeIndicators.Add (indicator1);

				GameObject indicator2 = Instantiate (indicatorRight) as GameObject;
				indicator2.GetComponent<Widget> ().layoutScreen = UI.Screen.center;
				indicator2.GetComponent<Widget> ().layoutAlignHorizontal = AlignmentH.center;
				indicator2.GetComponent<Widget> ().layoutAlignVertical = AlignmentV.center;
				indicator2.SetActive (true);
				t = indicator2.GetComponentInChildren<Text> ();
				t.text = message;
				indicator2.transform.SetParent (transform, false);
				activeIndicators.Add (indicator2);
			}
			return (++notificationID);
		}

		public void clearNotification( int id )
		{
			if (notificationID != id)
				return;

			foreach (GameObject go in activeIndicators) {
				Destroy (go);
			}
			activeIndicators.Clear ();
		}
    }
}
