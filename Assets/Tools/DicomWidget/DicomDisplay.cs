using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DicomDisplay : MonoBehaviour {

	public GameObject ListScreen;
	public GameObject ImageScreen;
	public GameObject ListEntryButton;

	public DicomDisplayImage DicomImage;
	//private Dropdown mDicomList;

	public Text StatusText;

	void Awake()
	{
		//mDicomList = transform.Find ("Canvas/DicomList").GetComponent<Dropdown>();
		//DicomImage = transform.Find ("Canvas/DicomImage").gameObject.GetComponent<DicomDisplayImage>();

		StatusText.text = "Searching for DICOMs ...";
		eventClear ();
		DicomImage.widget = GetComponent<UI.Widget> ();
	}

	void OnEnable()
	{
		// Register event callbacks for all DICOM events:
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_StartLoading, eventHideDICOM );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
		eventClear ();
		eventNewDicomList ();
		//eventDisplayCurrentDicom ();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receive events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_StartLoading, eventHideDICOM );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.stopListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
	}

	//! Called when a new DICOM was loaded:
	void eventDisplayCurrentDicom( object obj = null )
	{
		DICOM dicom = DICOMLoader.instance.currentDICOM;
		if( dicom != null )
		{
			DicomImage.SetDicom (dicom);
			DicomImage.gameObject.SetActive (true);
			StatusText.gameObject.SetActive (false);
			ImageScreen.SetActive (true);
			ListScreen.SetActive (false);
		}
	}

	void eventNewDicomList( object obj = null )
	{
		eventClear ();	// Clear the list

		// Make sure a patient is loaded:
		Patient p = Patient.getLoadedPatient ();
		if (p == null) {
			Text newEntryText = ListEntryButton.transform.GetComponentInChildren<Text> ();
			newEntryText.text = "No patient loaded.";
			ListEntryButton.SetActive (true);
			return;
		}
		// Make sure at least one series was found:
		List<DICOMSeries> series = DICOMLoader.instance.availableSeries;
		if (series.Count <= 0) {
			Text newEntryText = ListEntryButton.transform.GetComponentInChildren<Text> ();
			newEntryText.text = "No series found.";
			ListEntryButton.SetActive (true);
			return;
		}

		// Deactivate default button:
		ListEntryButton.SetActive (false);

		foreach (DICOMSeries s in series) {
			//customNames.Add (p.getDICOMNameForSeriesUID (uid));
			GameObject newEntry = Instantiate (ListEntryButton) as GameObject;
			newEntry.SetActive (true);

			Text newEntryText = newEntry.transform.GetComponentInChildren<Text> ();
			newEntryText.text = s.getDescription ();
			newEntry.transform.SetParent (ListEntryButton.transform.parent, false);

			// Make the button load the DICOM:
			Button newButton = newEntry.GetComponent<Button> ();
			DICOMSeries captured = s;
			newButton.onClick.AddListener(() => selectedNewDicom( captured ));
		}

		DicomImage.gameObject.SetActive (false);
	}

	void eventClear( object obj = null )
	{
		foreach (Transform tf in ListEntryButton.transform.parent) {
			if (tf != ListEntryButton.transform) {
				Destroy (tf.gameObject);
			}
		}
		Text newEntryText = ListEntryButton.transform.GetComponentInChildren<Text> ();
		newEntryText.text = "No series loaded.";
		ListEntryButton.SetActive (true);
		backToList ();
	}
	void eventHideDICOM( object obj = null )
	{
		//DicomImage.gameObject.SetActive (false);
	}
	public void selectedNewDicom( DICOMSeries series )
	{
		// Check if we previously loaded an image from this series. If so, figure out what
		// the last shown layer was:
		int previousLayer = DicomImage.GetComponent<DicomDisplayImage> ().savedLayerForSeriesUID (series.seriesUID);
		// Load this layer:
		DICOMLoader.instance.startLoading (series, previousLayer);

		StatusText.gameObject.SetActive (true);
		StatusText.text = "Loading DICOM ...";
		ImageScreen.SetActive (true);
		ListScreen.SetActive (false);
		//DicomImage.gameObject.SetActive (false);
	}

	public void backToList()
	{
		ImageScreen.SetActive (false);
		DicomImage.gameObject.SetActive (false);
		ListScreen.SetActive (true);
	}

}
