using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DicomDisplay : MonoBehaviour {

	public GameObject ListScreen;
	public GameObject ImageScreen;
	public GameObject ListEntry;
	//public Image HistogramImage;

	public DicomDisplayImage DicomImage;
	//private Dropdown mDicomList;

	public Text StatusText;

	bool UserIsLookingAtMe = false;

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
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoadedSlice, eventDisplayCurrentDicom );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
		//PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventDisplayHistogram );
		eventClear ();
		eventNewDicomList ();
		//eventDisplayCurrentDicom ();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receive events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoadedSlice, eventDisplayCurrentDicom );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.stopListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
		//PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventDisplayHistogram );
	}

	//! Called when a new DICOM was loaded:
	void eventDisplayCurrentDicom( object obj = null )
	{
		DICOM dicom = DICOMLoader.instance.currentDICOM;
		if( dicom != null && dicom is DICOM2D )
		{
			DicomImage.SetDicom (dicom as DICOM2D);
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
			Text newEntryText = ListEntry.transform.GetComponentInChildren<Text> ();
			newEntryText.text = "No patient loaded.";
			ListEntry.SetActive (true);
			return;
		}
		// Make sure at least one series was found:
		List<DICOMSeries> series = DICOMLoader.instance.availableSeries;
		if (series.Count <= 0) {
			Text newEntryText = ListEntry.transform.GetComponentInChildren<Text> ();
			newEntryText.text = "No series found.";
			ListEntry.SetActive (true);
			return;
		}

		// Deactivate default button:
		ListEntry.SetActive (false);

		foreach (DICOMSeries s in series) {
			//customNames.Add (p.getDICOMNameForSeriesUID (uid));
			GameObject newEntry = Instantiate (ListEntry) as GameObject;
			newEntry.SetActive (true);

			Text newEntryText = newEntry.transform.GetComponentInChildren<Text> ();
			newEntryText.text = s.getDescription ();
			newEntry.transform.SetParent (ListEntry.transform.parent, false);

			// Keep a reference to this series:
			DICOMSeries captured = s;

			// Make the button load the DICOM:
			GameObject newButton = newEntry.transform.Find("EntryButton").gameObject;
			newButton.GetComponent<Button> ().onClick.AddListener(() => selectedNewDicom( captured ));

			// Make the volume button display the DICOM as volume:
			GameObject volumetricButton = newEntry.transform.Find ("VolumetricButton").gameObject;
			// For now, only allow Transverse volumes:
			if (captured.sliceOrientation == SliceOrientation.Transverse && captured.isConsecutiveVolume) {
				volumetricButton.GetComponent<Button> ().onClick.AddListener (() => selectedNewVolumetric (captured));
				volumetricButton.SetActive (true);
			} else {
				volumetricButton.SetActive (false);	
			}
		}

		DicomImage.gameObject.SetActive (false);
	}

	void eventClear( object obj = null )
	{
		foreach (Transform tf in ListEntry.transform.parent) {
			if (tf != ListEntry.transform) {
				Destroy (tf.gameObject);
			}
		}
		Text newEntryText = ListEntry.transform.GetComponentInChildren<Text> ();
		newEntryText.text = "No series loaded.";
		ListEntry.SetActive (true);
		backToList ();
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
	public void selectedNewVolumetric( DICOMSeries series )
	{
		if (DICOMLoader.instance.currentDICOMVolume != null
			&& DICOMLoader.instance.currentDICOMVolume.seriesInfo == series) {
			DICOMLoader.instance.unloadVolume ();
		} else {
			DICOMLoader.instance.startLoadingVolume (series);
		}
	}

	public void backToList()
	{
		ImageScreen.SetActive (false);
		DicomImage.gameObject.SetActive (false);
		ListScreen.SetActive (true);
	}


	public void Update()
	{
		/*UI.Screen myScreen = GetComponent<UI.Widget> ().layoutPosition.screen;
		UI.Screen activeScreen = UI.Core.instance.layoutSystem.activeScreen;
		if (myScreen == activeScreen) {
			if (!UserIsLookingAtMe) {
				UserIsLookingAtMe = true;
				ToolControl.instance.overrideTool ("DICOM");
			}
		} else {
			if (UserIsLookingAtMe) {
				UserIsLookingAtMe = false;
				ToolControl.instance.unoverrideTool ("DICOM");
			}
		}*/
	}

	/*public void eventDisplayHistogram( object obj = null )
	{
		Histogram hist = DICOMLoader.instance.currentDICOMSeries.histogram;
		Texture2D tex = hist.asTexture ();
		Sprite sprite = Sprite.Create(tex, new Rect(0,0,tex.width, tex.height), new Vector2(0.5f,0.5f));
		HistogramImage.sprite = sprite;
	}*/
}
