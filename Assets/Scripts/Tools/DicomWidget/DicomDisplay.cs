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
		DicomCache.startListening ("NewDicomLoaded", eventDisplayCurrentDicom );
		DICOM dicom = DicomCache.getCurrentDicom();
		if( dicom != null )
		{
			/*DicomRenderer.material.mainTexture = dicom.getTexture ();*/

			mDicomImage.material.mainTexture = dicom.getTexture();
			mDicomImage.material.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
			mDicomImage.material.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
			mDicomImage.material.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));
		}

	}

	void OnDisable()
	{
		DicomCache.stopListening ("NewDicomLoaded", eventDisplayCurrentDicom );
	}

	void eventDisplayCurrentDicom()
	{
		DICOM dicom = DicomCache.getCurrentDicom();
		if( dicom != null )
		{
			/*DicomRenderer.material.mainTexture = dicom.getTexture ();*/

			mDicomImage.material.mainTexture = dicom.getTexture();
			mDicomImage.material.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
			mDicomImage.material.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
			mDicomImage.material.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));
		}
	}

	private RawImage mDicomImage;
}
