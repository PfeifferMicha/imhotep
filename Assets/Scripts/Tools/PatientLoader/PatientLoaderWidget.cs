using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UI;

public class PatientLoaderWidget : Widget {

    GameObject mScrollView;

    // Use this for initialization
    void Start () {

        // Get the scroll view defined for the widget:
        mScrollView = transform.Find("Canvas/Background/Scroll View").gameObject;
        // Disable the default button:
        GameObject defaultPatientButton = mScrollView.transform.Find("Viewport/Content/ButtonPatient").gameObject;
        defaultPatientButton.SetActive(false);

        mPatientLoader = new PatientLoader();
        mPatientLoader.setPath("../Patients/");

        for( int index = 0; index < mPatientLoader.getCount(); index ++ )
        {
            PatientEntry patient = mPatientLoader.getEntry(index);

            // Create a new instance of the list button:
            GameObject newButton = Instantiate(defaultPatientButton);
            newButton.SetActive(true);

            // Attach the new button to the list:
            newButton.transform.SetParent(defaultPatientButton.transform.parent);

            // Fill button's text object:
            Text t = newButton.transform.Find("OverlayImage/Text").GetComponent<Text>();
            t.text = patient.name + "\n" + patient.birthDate + "\nOperation: " + patient.operationDate;

            // Set up events:
            int capturedIndex = index;
            Button b = newButton.GetComponent<Button>();
            b.onClick.AddListener(() => ChoosePatient(capturedIndex));
        }

        RectTransform rectTf = defaultPatientButton.transform.parent.GetComponent<RectTransform>();
        rectTf.sizeDelta = new Vector2(mPatientLoader.getCount()*202+2, rectTf.rect.height);
    }
	
	// Update is called once per frame
	void Update () {

    }

    void ChoosePatient( int index )
    {
        // TODO make singleton? Unload any previous patient.
        mPatientLoader.loadPatient(index);
    }

    PatientLoader mPatientLoader;
}
