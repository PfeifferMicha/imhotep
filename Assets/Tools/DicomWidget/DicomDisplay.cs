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
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();        
        DICOMSlice dicom = mPatientDICOMLoader.getCurrentDicom();
		if( dicom != null )
		{
			DicomImage.gameObject.SetActive (true);
			DicomImage.SetDicom (dicom);
			StatusText.gameObject.SetActive (false);
			ImageScreen.SetActive (true);
			ListScreen.SetActive (false);
		}
	}

	void eventNewDicomList( object obj = null )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
		eventClear ();	// Clear the list

		List<DICOMHeader> series = mPatientDICOMLoader.getAvailableSeries ();
		Patient p = Patient.getLoadedPatient ();
		if( p != null )
		{
			int i = 0;
			foreach (DICOMHeader header in series) {
				//customNames.Add (p.getDICOMNameForSeriesUID (uid));
				GameObject newEntry = Instantiate (ListEntryButton) as GameObject;
				newEntry.SetActive (true);

				Text newEntryText = newEntry.transform.GetComponentInChildren<Text> ();
				newEntryText.text = header.toDescription ();
				newEntry.transform.SetParent (ListEntryButton.transform.parent, false);

				// Make the button load the DICOM:
				Button newButton = newEntry.GetComponent<Button> ();
				int capturedID = i;
				newButton.onClick.AddListener(() => selectedNewDicom( capturedID ));
				i++;
			}
		}
		if (series.Count == 0) {
			Text newEntryText = ListEntryButton.transform.GetComponentInChildren<Text> ();
			newEntryText.text = "No series found.";
			ListEntryButton.SetActive (true);
		} else {
			ListEntryButton.SetActive (false);
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
	public void selectedNewDicom( int id )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
		mPatientDICOMLoader.loadDicomSlice ( id, 5 );

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
