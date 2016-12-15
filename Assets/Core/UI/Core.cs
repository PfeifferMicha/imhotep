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
		/*! Is set to true whenever the mouse is over a UI element on the platform
		 * (but ignores UI elments which are beyond the UIMesh, like things that are attached to the organs).*/
		public bool pointerIsOverPlatformUIObject{ private set; get; }

		public static Core instance { private set; get; }

		public Camera UICamera;

		public LayoutSystem layoutSystem { private set; get; }

		//! The scale with which all the UI elements are scaled (i.e. pixels to meters)
		public float UIScale = 0.0025f;
		public float pixelsPerMeter = 500f;
		public float aspectRatio = 1f;

		public Color TabHighlightColor;
		public Color ButtonBaseColor;
		public Color ButtonHighlightColor;

		public Sprite normalTabImage;
		public Sprite selectedTabImage;

		public GameObject indicatorLeft;
		public GameObject indicatorRight;

		//! A bar on the lower end of the screen (for close button etc.)
		public GameObject statusBar;
		private GameObject closePatientButton;
		private GameObject savePatientButton;

		private List<GameObject> activeIndicators = new List<GameObject> ();
		private int indicationID = 0;

		public GameObject PatientSelector;


		public Core()
		{
			instance = this;
		}

		public void OnEnable()
		{
			layoutSystem = new LayoutSystem ();

			indicatorLeft.SetActive (false);
			indicatorRight.SetActive (false);
			statusBar.SetActive (true);

			Transform tf = statusBar.transform.Find ("ButtonClose");
			if (tf != null) {
				closePatientButton = tf.gameObject;
				closePatientButton.GetComponent<Button> ().onClick.AddListener (() => closePatient ());
				closePatientButton.SetActive (false);
			} else {
				Debug.LogWarning ("ButtonClose not found on Status Bar!");
			}

			tf = statusBar.transform.Find ("ButtonSave");
			if (tf != null) {
				savePatientButton = tf.gameObject;
				savePatientButton.GetComponent<Button> ().onClick.AddListener (() => savePatient ());
				savePatientButton.SetActive (false);
			} else {
				Debug.LogWarning ("ButtonSave not found on Status Bar!");
			}

			PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_FinishedLoading, patientLoaded );
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
		public void setPointerIsOnPlatformUI( bool onUI )
		{
			pointerIsOverPlatformUIObject = onUI;
		}

		public void setCamera( Camera cam )
		{
			UICamera = cam;
			aspectRatio = (float)UICamera.targetTexture.width / (float)UICamera.targetTexture.height;
			layoutSystem.updateDimensions ();

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
			colors.highlightedColor = TabHighlightColor;
			b.colors = colors;
			b.image.sprite = selectedTabImage;
		}

		public void unselectTab( Button b )
		{
			ColorBlock colors = b.colors;
			colors.normalColor = ButtonBaseColor;
			colors.highlightedColor = ButtonBaseColor;
			b.colors = colors;
			b.image.sprite = normalTabImage;
		}

		public int addIndication( UI.Screen screen, string message )
		{
			if (activeIndicators.Count > 0) {
				clearIndication (indicationID);
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
			return (++indicationID);
		}

		public void clearIndication( int id )
		{
			if (indicationID != id)
				return;

			foreach (GameObject go in activeIndicators) {
				Destroy (go);
			}
			activeIndicators.Clear ();
		}

		public void closePatient()
		{
			Patient.close ();
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.PATIENT_Closed);
			layoutSystem.closeAllWidgets ();
			closePatientButton.SetActive (false);
			savePatientButton.SetActive (false);
			PatientSelector.SetActive (true);
		}
		public void savePatient()
		{
			if (Patient.getLoadedPatient () != null) {
				Patient.getLoadedPatient ().save ();
				NotificationControl.instance.createNotification ("Patient saved.", new System.TimeSpan (0, 0, 5));
			}
		}

		public void patientLoaded( object obj = null )
		{
			closePatientButton.SetActive (true);
			savePatientButton.SetActive (true);
		}
    }
}
