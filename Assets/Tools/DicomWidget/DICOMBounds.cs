using UnityEngine;
using System.Collections;

/*! Displays the Bounds of the currently loaded DICOM series around the 3D Mesh.
 * Mostly used to debug DICOM positions.
 * This should be attached to a GameObject which is a child of the MeshPositionNode (i.e. it should
 * have the same parent as the organs, meaning it's also in the Patient Coordinate System). */
public class DICOMBounds : MonoBehaviour {

	// Bounding box:
	private LineRenderer Edge1;
	private LineRenderer Edge2;
	private LineRenderer Edge3;
	private LineRenderer Edge4;
	private LineRenderer Edge5;
	private LineRenderer Edge6;
	private LineRenderer Edge7;
	private LineRenderer Edge8;
	private LineRenderer Edge9;
	private LineRenderer Edge10;
	private LineRenderer Edge11;
	private LineRenderer Edge12;

	// Rectangle
	private LineRenderer RectXMin;
	private LineRenderer RectXMax;
	private LineRenderer RectYMin;
	private LineRenderer RectYMax;

	private string currentSeriesUID = "";

	private bool listeningToEvents = false;

	enum DisplayMode {
		OFF,
		SLICE_OUTLINE,
		SLICE
	}
	private DisplayMode displayMode = DisplayMode.OFF;

	void Start()
	{
		Edge1 = gameObject.transform.Find ("Edge (1)").GetComponent<LineRenderer> ();
		Edge2 = gameObject.transform.Find ("Edge (2)").GetComponent<LineRenderer> ();
		Edge3 = gameObject.transform.Find ("Edge (3)").GetComponent<LineRenderer> ();
		Edge4 = gameObject.transform.Find ("Edge (4)").GetComponent<LineRenderer> ();
		Edge5 = gameObject.transform.Find ("Edge (5)").GetComponent<LineRenderer> ();
		Edge6 = gameObject.transform.Find ("Edge (6)").GetComponent<LineRenderer> ();
		Edge7 = gameObject.transform.Find ("Edge (7)").GetComponent<LineRenderer> ();
		Edge8 = gameObject.transform.Find ("Edge (8)").GetComponent<LineRenderer> ();
		Edge9 = gameObject.transform.Find ("Edge (9)").GetComponent<LineRenderer> ();
		Edge10 = gameObject.transform.Find ("Edge (10)").GetComponent<LineRenderer> ();
		Edge11 = gameObject.transform.Find ("Edge (11)").GetComponent<LineRenderer> ();
		Edge12 = gameObject.transform.Find ("Edge (12)").GetComponent<LineRenderer> ();
		RectXMin = gameObject.transform.Find ("RectXMin").GetComponent<LineRenderer> ();
		RectXMax = gameObject.transform.Find ("RectXMax").GetComponent<LineRenderer> ();
		RectYMin = gameObject.transform.Find ("RectYMin").GetComponent<LineRenderer> ();
		RectYMax = gameObject.transform.Find ("RectYMax").GetComponent<LineRenderer> ();

		if (!listeningToEvents) {
			PatientEventSystem.startListening (PatientEventSystem.Event.DICOM_NewLoadedSlice, eventNewDICOM);
			PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, patientClosed);
			listeningToEvents = true;	// Don't call startListening again when disabling and re-enabling this object.
		}

		// In case a DICOM is already loaded:
		eventNewDICOM ();
	}
	void OnDestroy() {
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoadedSlice, eventNewDICOM );
		PatientEventSystem.stopListening( PatientEventSystem.Event.PATIENT_Closed, patientClosed );
	}

	void eventNewDICOM( object obj = null )
	{
		if (displayMode == DisplayMode.OFF) {
			gameObject.SetActive (false);
			return;
		}

		DICOM dicom = DICOMLoader.instance.currentDICOM;
		if (dicom != null) {

			// If the series has changed, modify the bounding box:
			/*if (dicom.seriesInfo.seriesUID != currentSeriesUID && dicom.seriesInfo.isConsecutiveVolume) {

				Debug.Log ("3");

				// Calculate the positions of the corners of this stack of slices:
				int lastSlice = dicom.seriesInfo.numberOfSlices - 1;
				Vector3 c1 = dicom.transformPixelToPatientPos (Vector2.zero, 0f);
				Vector3 c2 = dicom.transformPixelToPatientPos (new Vector2 (dicom.origTexWidth, 0f), 0f);
				Vector3 c3 = dicom.transformPixelToPatientPos (new Vector2 (0f, dicom.origTexHeight), 0f);
				Vector3 c4 = dicom.transformPixelToPatientPos (new Vector2 (dicom.origTexWidth, dicom.origTexHeight), 0f);
				Vector3 c5 = dicom.transformPixelToPatientPos (Vector2.zero, lastSlice);
				Vector3 c6 = dicom.transformPixelToPatientPos (new Vector2 (dicom.origTexWidth, 0f), lastSlice);
				Vector3 c7 = dicom.transformPixelToPatientPos (new Vector2 (0f, dicom.origTexHeight), lastSlice);
				Vector3 c8 = dicom.transformPixelToPatientPos (new Vector2 (dicom.origTexWidth, dicom.origTexHeight), lastSlice);

				// Display the bounding box:
				Edge1.SetPosition (0, c1);
				Edge1.SetPosition (1, c2);
				Edge2.SetPosition (0, c1);
				Edge2.SetPosition (1, c3);
				Edge3.SetPosition (0, c2);
				Edge3.SetPosition (1, c4);
				Edge4.SetPosition (0, c3);
				Edge4.SetPosition (1, c4);

				Edge5.SetPosition (0, c5);
				Edge5.SetPosition (1, c6);
				Edge6.SetPosition (0, c5);
				Edge6.SetPosition (1, c7);
				Edge7.SetPosition (0, c6);
				Edge7.SetPosition (1, c8);
				Edge8.SetPosition (0, c7);
				Edge8.SetPosition (1, c8);

				Edge9.SetPosition (0, c1);
				Edge9.SetPosition (1, c5);
				Edge10.SetPosition (0, c2);
				Edge10.SetPosition (1, c6);
				Edge11.SetPosition (0, c3);
				Edge11.SetPosition (1, c7);
				Edge12.SetPosition (0, c4);
				Edge12.SetPosition (1, c8);

				// Remember which series we're currently using:
				currentSeriesUID = dicom.seriesInfo.seriesUID;
				Edge1.enabled = true;
				Edge2.enabled = true;
				Edge3.enabled = true;
				Edge4.enabled = true;
				Edge5.enabled = true;
				Edge6.enabled = true;
				Edge7.enabled = true;
				Edge8.enabled = true;
				Edge9.enabled = true;
				Edge10.enabled = true;
				Edge11.enabled = true;
				Edge12.enabled = true;
			} else {
				Edge1.enabled = false;
				Edge2.enabled = false;
				Edge3.enabled = false;
				Edge4.enabled = false;
				Edge5.enabled = false;
				Edge6.enabled = false;
				Edge7.enabled = false;
				Edge8.enabled = false;
				Edge9.enabled = false;
				Edge10.enabled = false;
				Edge11.enabled = false;
				Edge12.enabled = false;
			}*/

			if (dicom is DICOM2D) {

				DICOM2D dicom2D = dicom as DICOM2D;
				// Display the position of the current slice:

				Vector3 p1 = dicom2D.transformPixelToPatientPos (Vector2.zero);
				Vector3 p2 = dicom2D.transformPixelToPatientPos (new Vector2 (dicom2D.origTexWidth, 0f), dicom2D.slice);
				Vector3 p3 = dicom2D.transformPixelToPatientPos (new Vector2 (dicom2D.origTexWidth, dicom2D.origTexHeight), dicom2D.slice);
				Vector3 p4 = dicom2D.transformPixelToPatientPos (new Vector2 (0, dicom2D.origTexHeight), dicom2D.slice);
				RectXMin.SetPosition (0, p1);
				RectXMin.SetPosition (1, p4);
				RectXMax.SetPosition (0, p2);
				RectXMax.SetPosition (1, p3);
				RectYMin.SetPosition (0, p1);
				RectYMin.SetPosition (1, p2);
				RectYMax.SetPosition (0, p3);
				RectYMax.SetPosition (1, p4);

				RectXMin.enabled = true;
				RectXMax.enabled = true;
				RectYMin.enabled = true;
				RectYMax.enabled = true;
				//itk.simple.VectorDouble vec = dicom2D.origin;
				//Debug.Log ("dicom2D.origin " + vec [0] + " " + vec [1] + " " + vec [2]);
			} else {
				RectXMin.enabled = false;
				RectXMax.enabled = false;
				RectYMin.enabled = false;
				RectYMax.enabled = false;
			}

			gameObject.SetActive (true);
		} else {
			gameObject.SetActive (false);
		}
	}

	public void patientClosed( object obj = null )
	{
		gameObject.SetActive (false);
	}

	/*! Cycle through the display modes:
	 * 	- Off
	 * 	- Show slice outline
	 * 	- Show slice */
	public void toggle( bool t )
	{
		Debug.Log ("Toggle " + t);
		if (t)
			displayMode = DisplayMode.SLICE;
		else
			displayMode = DisplayMode.OFF;
		eventNewDICOM ();
	}
}
