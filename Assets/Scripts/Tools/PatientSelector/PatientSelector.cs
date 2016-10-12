using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UI;

public class PatientSelector : MonoBehaviour {

	public Sprite OperationLiver = null;
	public Sprite OperationBoneFracture = null;
	public Sprite OperationUnknown = null;

    private GameObject mScrollView;

	private int notificationID;

    // Use this for initialization
    void Start () {

        // Get the scroll view defined for the widget:
        mScrollView = transform.Find("Background/Scroll View").gameObject;
        // Disable the default button:
        defaultPatientButton = mScrollView.transform.Find("Viewport/Content/ButtonPatient").gameObject;
        defaultPatientButton.SetActive(false);

		PatientEventSystem.startListening (
			PatientEventSystem.Event.PATIENT_NewPatientDirectoryFound,
			addPatientEntry
		);

        PatientDirectoryLoader.setPath("../Patients/");

    }
	public void OnEnable()
	{
		notificationID = UI.Core.instance.addIndication (UI.Screen.left, "Please choose patient.");
	}
	public void OnDisable()
	{
		UI.Core.instance.clearNotification (notificationID);
	}

    void ChoosePatient( int index )
    {
        // TODO make singleton? Unload any previous patient.
        //mPatientLoader.loadPatient(index);
		PatientDirectoryLoader.loadPatient(index);
        //PatientCache.instance.openPatient(index);
    }

	void addPatientEntry( object obj = null )
	{

		// Remove all entries in the list:
		foreach(Transform child in defaultPatientButton.transform.parent) {
			//Debug.Log (child.name);
			// Remove all buttons except for the default button:
			if( child.gameObject.activeSelf ) {
				Destroy (child.gameObject);
			}
		}

		// Add all new entries:
		for( int index = 0; index < PatientDirectoryLoader.getCount(); index ++ )
		{
			PatientMeta patient = PatientDirectoryLoader.getEntry(index);

			// Create a new instance of the list button:
			GameObject newButton = Instantiate(defaultPatientButton);
			newButton.SetActive(true);

			// Attach the new button to the list:
			newButton.transform.SetParent(defaultPatientButton.transform.parent, false );

			// Fill button's text object:
			Text t = newButton.transform.Find("Text").GetComponent<Text>();
			t.text = "<color=white>" + patient.name + "</color>\n" + patient.birthDate;

			newButton.transform.Find ("ImageFemale").gameObject.SetActive (false);
			newButton.transform.Find ("ImageMale").gameObject.SetActive (false);
			if (patient.sex == "f") {
				newButton.transform.Find ("ImageFemale").gameObject.SetActive (true);
			} else if (patient.sex == "m") {
				newButton.transform.Find ("ImageMale").gameObject.SetActive (true);
			}

			Text ageText = newButton.transform.Find("AgeText").GetComponent<Text>();
			if (patient.age >= 0) {
				ageText.text = patient.age + " Yrs.";
			} else {
				ageText.text = "";
			}

			Text detailsText = newButton.transform.Find("TextDetails").GetComponent<Text>();
			detailsText.text = patient.indication + "\n" + patient.details;

			Image operationTypeImage = newButton.transform.Find ("OperationTypeImage").GetComponent<Image> ();
			if (operationTypeImage != null) {
				operationTypeImage.sprite = spriteForOperationType (patient.operationType);
			} else {
				operationTypeImage.gameObject.SetActive (false);
			}

			// Set up events:
			int capturedIndex = index;
			Button b = newButton.GetComponent<Button>();
			b.onClick.AddListener(() => ChoosePatient(capturedIndex));
			b.onClick.AddListener(() => transform.GetComponent<Widget>().Close());
		}

		/*RectTransform rectTf = defaultPatientButton.transform.parent.GetComponent<RectTransform>();
		RectTransform buttonRectTF = defaultPatientButton.transform.GetComponent<RectTransform>();
		float newWidth = PatientDirectoryLoader.getCount () * (buttonRectTF.rect.width + 2.0f);
		rectTf.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, newWidth);*/

		// Set the scroll view position:
		//Vector2 currentScrollPos = mScrollView.GetComponent<ScrollRect>().normalizedPosition;
		//mScrollView.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0,currentScrollPos.y);

	}

	Sprite spriteForOperationType( PatientMeta.OperationType ot )
	{
		if (ot == PatientMeta.OperationType.Liver) {
			return OperationLiver;
		} else if (ot == PatientMeta.OperationType.BoneFracture) {
			return OperationBoneFracture;
		} else {
			return OperationUnknown;
		}
	}

	private GameObject defaultPatientButton = null;
}
