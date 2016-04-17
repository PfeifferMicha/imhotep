using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class DicomDisplay : MonoBehaviour {


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void Awake()
	{
		Transform tf = transform.Find ("Canvas/DicomImage");
		mDicomImage = tf.gameObject.GetComponent<RawImage>();
	}

	void OnEnable()
	{
		// Register event callbacks for all DICOM events:
		DicomCache.startListening ( DicomCache.Event.NewDicomLoaded, eventDisplayCurrentDicom );
		DicomCache.startListening ( DicomCache.Event.NewDicomList, eventNewDicomList );
		DicomCache.startListening ( DicomCache.Event.AllCleared, eventClear );
		eventDisplayCurrentDicom ();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
		DicomCache.stopListening ( DicomCache.Event.NewDicomLoaded, eventDisplayCurrentDicom );
		DicomCache.stopListening ( DicomCache.Event.NewDicomList, eventNewDicomList );
		DicomCache.stopListening ( DicomCache.Event.AllCleared, eventClear );
	}

	// Called when a new DICOM was loaded:
	void eventDisplayCurrentDicom()
	{
		DICOM dicom = DicomCache.getCurrentDicom();
		if( dicom != null )
		{
			mDicomImage.material.mainTexture = dicom.getTexture();
			mDicomImage.material.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
			mDicomImage.material.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
			mDicomImage.material.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));
		}
	}

	void eventNewDicomList()
	{

	}
	void eventClear()
	{
	}

	private RawImage mDicomImage;
}
