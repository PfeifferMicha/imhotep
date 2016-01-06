using UnityEngine;
using System.Collections;
using itk.simple;

public class DICOMLoaderDummy : MonoBehaviour {
	
	public string path;

	// Use this for initialization
	void Start () {

		// DEBUG:
		DicomLoaderITK dl = new DicomLoaderITK ();
		VectorString series = dl.loadDirectory ( path );

		if (series.Count > 0) {
			DICOM dcm;
			try {
				dcm = dl.load (path, series [0]);

				GameObject dicomViewer = GameObject.Find ("DICOM_Plane");
				if (dicomViewer) {
					Renderer dicomRenderer = dicomViewer.GetComponent<Renderer> ();
					dicomRenderer.material.mainTexture = dcm.getTexture ();
					dicomRenderer.material.SetFloat ("globalMaximum", (float)dcm.getMaximum ());
					dicomRenderer.material.SetFloat ("globalMinimum", (float)dcm.getMinimum ());
					dicomRenderer.material.SetFloat ("range", (float)(dcm.getMaximum () - dcm.getMinimum ()));
				} else {
					Debug.LogWarning ("Can't find DICOM display object.");
				}

			} catch (System.Exception exp) {
				Debug.LogWarning ("Could not load DICOM:\n" + exp.Message);
			}
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
