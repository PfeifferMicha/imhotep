using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    public class WidgetControl : MonoBehaviour
    {

		public GameObject PatientLoaderButton;		// Button which starts the patient loader
		public GameObject ToolLoaderButton;			// Button which starts a tool
		public GameObject PatientCloseButton;			// Button which starts a tool

		// Widgets:
		public GameObject PatientLoaderWidget;
        public GameObject[] AvailableWidgets;

		private List<GameObject> ToolButtonList = new List<GameObject> ();

		private Camera UICamera;

        // Use this for initialization
        void Start()
		{
			ToolLoaderButton.SetActive (false);
			PatientCloseButton.SetActive (false);

			UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();

			// Whenever a Patient has been loaded, display the tool list:
			PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_FinishedLoading, ShowWidgetList);
        }

        // Update is called once per frame
        void Update()
        {
        }

        void StartWidget(GameObject widget)
		{
			Debug.Log("Creating: " + widget.name);
            GameObject newWidget = Instantiate(widget);
            newWidget.SetActive(true);
			newWidget.transform.SetParent (transform.parent, false);
			//newWidget.transform.localPosition = new Vector3 (0, 0, 0);
			newWidget.transform.localScale = transform.localScale;
			Canvas cv = newWidget.transform.FindChild("Canvas").GetComponent<Canvas> ();
			cv.worldCamera = UICamera;

			UI.UICore.HighlightSelectedWidget (newWidget.transform);
        }

		public void ShowPatientLoader()
		{
			PatientLoaderButton.SetActive (false);
			StartWidget (PatientLoaderWidget);
		}

		public void ClosePatient()
		{
			//ToolLoaderButton.
			HideWidgetList();
			PatientLoaderButton.SetActive (true);
		}

		public void ShowWidgetList( object obj = null )
        {
			ToolButtonList.Clear ();
			PatientLoaderButton.SetActive (false);

			float width = ToolLoaderButton.GetComponent<RectTransform>().rect.width;

			int numOfToolButtons = AvailableWidgets.Length + 1;
			int iter = 0;
			foreach (GameObject widget in AvailableWidgets)
			{
				// Create a new instance of the list button:
				GameObject newButton = Instantiate(ToolLoaderButton);
				newButton.SetActive(true);

				// Attach the new button to the list:
				newButton.transform.SetParent(ToolLoaderButton.transform.parent, false);

				Vector3 pos = newButton.transform.localPosition;
				newButton.transform.localPosition = new Vector3 (
					-(numOfToolButtons - 1)*width*0.5f + iter*width, pos.y, pos.z);

				// Change button text to name of tool:
				GameObject textObject = newButton.transform.Find("OverlayImage/Text").gameObject;
				Text t = textObject.GetComponent<Text>();
				t.text = widget.name;

				GameObject imgObject = newButton.transform.Find("OverlayImage/Image").gameObject;
				Image im = imgObject.GetComponent<Image> ();
				Widget widg = widget.GetComponent<Widget>();
				if (widg != null && widg.ToolIcon != null) {
					im.sprite = widg.ToolIcon;
				}

				GameObject captured = widget;       // needed, because otherwise the 'widget' variable is changed every iteration

				// Set the button actions:
				Button b = newButton.GetComponent<Button>();
				b.onClick.AddListener(() => StartWidget(captured));

				ToolButtonList.Add (newButton);

				iter++;
			}

			Vector3 position = PatientCloseButton.transform.localPosition;
			PatientCloseButton.transform.localPosition = new Vector3 (
				-(numOfToolButtons - 1)*width*0.5f + iter*width, position.y, position.z);
			PatientCloseButton.SetActive (true);
        }

        public void HideWidgetList()
        {
			foreach (GameObject toolButton in ToolButtonList) {
				Destroy (toolButton);
			}
			ToolButtonList.Clear ();
			PatientCloseButton.SetActive (false);
        }
        
    }
}