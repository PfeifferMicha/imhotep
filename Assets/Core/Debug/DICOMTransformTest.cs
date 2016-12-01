using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DICOMTransformTest : MonoBehaviour, IPointerClickHandler {

	public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData )
	{

		// Get the currently loaded DICOM:
		DICOM dicom = DICOMLoader.instance.currentDICOM;
		if( dicom != null )
		{
			// The world position which was clicked:
			Vector3 worldPos = eventData.pointerCurrentRaycast.worldPosition;
			// Transform the position to the patient coordinate system (i.e. in mm)
			Vector3 localPos = transform.InverseTransformPoint( worldPos );

			Debug.Log ("Clicked: " + worldPos + " (local: " + localPos + ")" );

			// Now we can either transform the position to a continuous pixel position...
			// (to do so, we use the series Info associated with the DICOM:
			Vector3 pixel = dicom.seriesInfo.transformPatientPosToPixel ( localPos );
			Debug.Log ("Pixel: " + pixel.x + "," + pixel.y + " on layer " + pixel.z);

			// ... or we can transform it to a discrete pixel position:
			Vector3 pixelRounded = dicom.seriesInfo.transformPatientPosToDiscretePixel ( localPos );
			Debug.Log ("Rounded: " + pixelRounded.x + "," + pixelRounded.y + " on layer " + pixelRounded.z);
		} else {
			Debug.Log ( "No DICOM loaded." );
		}
	}
}
