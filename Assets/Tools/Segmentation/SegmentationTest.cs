using UnityEngine;
using System.Collections;
using itk.simple;

public class SegmentationTest : MonoBehaviour {
	void OnEnable() {
		PatientEventSystem.startListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );

		DICOMLoader.instance.startLoadingVolume (DICOMLoader.instance.availableSeries [0]);
	}

	void OnDisable() {
		PatientEventSystem.stopListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );
	}

	public void OnDICOMLoaded( object obj = null )
	{
		DICOM2D dicom = obj as DICOM2D;
		if (dicom == null)
			return;
		if (dicom.dimensions != 3)
			return;

		Image volume = dicom.image;
		Debug.Log ("Width: " + volume.GetWidth () + ", height: " + volume.GetHeight () + ", depth: " + volume.GetDepth ());

		VectorUInt32 position = new VectorUInt32 {
			volume.GetWidth() / 2,
			volume.GetHeight() / 2,
			volume.GetDepth() / 2
		};
		// Asumes that the pixel type stored in the image is grayscale int32:
		int value = volume.GetPixelAsInt16 (position);
		Debug.Log ("Value of center pixel: " + value);
	}
}
