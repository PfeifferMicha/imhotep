
using itk.simple;
using UnityEngine;
using System;


public enum SliceOrientation
{
	Transverse,
	Saggital,
	Coronal,
	Unknown
};


/*! Information needed for a DICOM series.
 * \note This class assumes that a single series is made up of a single volume (or single slice).
 * 		This means that the direction cosines for each image in the volume are assumed to be the same.
 * \note This class assumes that the spacing in the series is the same between each pair of adjacent slices.*/
public class DICOMSeries {

	/*! The number of slices (i.e. number of files) for this series.*/
	public int numberOfSlices { private set; get; }
	/*! All files associated with this series. */
	public VectorString filenames { private set; get; }
	/*! A unique ID (see the DICOM standard) which identifies this series.*/
	public string seriesUID { private set; get; }

	/*! The first image in the series.*/
	public Image firstSlice { private set; get; }
	/*! The last image in the series.*/
	public Image lastSlice { private set; get; }

	/*! Approximate slice orientation of the slices in this series.
	 * Returns coronal, saggital or transverse depending on which way the normal of the slices
	 * is facing.
	 * \note This is only an approximation. "Intermediate" orientations are "rounded" to the nearest
	 * 		orientation.
	 *		This means that, for example, if a series is a transverse series then this function will
	 *		correctly return "transverse". However, if it is slightly tilted away from the transverse
	 *		orientation, this will still return transverse. */
	public SliceOrientation sliceOrientation { private set; get; }

	/*! Assuming this series is a volume of consecutive slices, where neighbouring slices always have
	 * the same offset between each other, this offset is stored in sliceOffset.
	 * For 2D slices, this is zero. */
	public Vector3 sliceOffset { private set; get; }

	/*! Minimal pixel value of first slice
	 * \note If possible, this is read from the DICOM header of the first slice.
	 *		However, this value may not be present, in which case this is filled when a slice/volume is
	 *		first loaded (by checking all pixel values and finding the minimum).*/
	public UInt32 minPixelValue { private set; get; }
	/*! Maximal pixel value of first slice
	 * \note If possible, this is read from the DICOM header of the first slice.
	 *		However, this value may not be present, in which case this is filled when a slice/volume is
	 *		first loaded (by checking all pixel values and finding the maximum).*/
	public UInt32 maxPixelValue { private set; get; }

	/*! True if the minPixelValue and maxPixelValue have been found or calculated, false otherwise. */
	public bool foundMinMaxPixelValues { private set; get; }

	/*! Cached human-readable description string.*/
	private string description = null;

	/*! The direction cosine of a row of this image.
	 * This can be thought of as a unit-length vector pointing into the direction in which the 
	 * row lies inside the patient coordinate system (i.e. when you walk along the row in 2D,
	 * in which direction would you walk in the patient coordinate system).
	 * See the DICOM standard for more information, or search online for "direction cosine". */
	public Vector3 directionCosineX { private set; get; }
	/*! The direction cosine of a column of this image.
	 * This can be thought of as a unit-length vector pointing into the direction in which the 
	 * column lies inside the patient coordinate system (i.e. when you walk along the column in 2D,
	 * in which direction would you walk in the patient coordinate system).
	 * See the DICOM standard for more information, or search online for "direction cosine". */
	public Vector3 directionCosineY { private set; get; }

	/*! True if there are multiple slices and the first and last slice have the same orientation, false otherwise. */
	public bool isConsecutiveVolume { private set; get; }


	/*! The plane normal of the slices in this series (result of cross vector of the direction cosines). */
	public Vector3 sliceNormal { private set; get; }

	/*! Constructor, fills most of the attributes of the DICOMSeries class.
	 * \note This does some heavy file/directory parsing to determine the files which are part of
	 * 		this series and their order. This is why the DICOMSeries should be constructed in a
	 * 		background thread and then passed to the main thread. */
	public DICOMSeries( string directory, string seriesUID )
	{
		Debug.Log ("Loading Meta Data for Series: " + seriesUID);
		// Get the file names for the series:
		filenames = ImageSeriesReader.GetGDCMSeriesFileNames( directory, seriesUID );
		if (filenames.Count <= 0) {
			throw( new System.Exception ("No files found for series " + seriesUID + "."));
		}
		this.seriesUID = seriesUID;

		// Load the first slice in volume to get meta information:
		firstSlice = SimpleITK.ReadImage( filenames[0] );
		lastSlice = SimpleITK.ReadImage( filenames[Math.Max(filenames.Count-1,0)] );

		numberOfSlices = filenames.Count;

		// Load the direction cosines:
		// ITK stores the direction cosines in a matrix with row-major-ordering. The weird indexing is because
		// we need the first and second column (0,3,6 for X and 1,4,7 for Y)
		VectorDouble direction = firstSlice.GetDirection();
		if( direction.Count < 6 )
			throw( new System.Exception ("Invalid direction cosines found in images."));
		directionCosineX = new Vector3 ((float)direction [0], (float)direction [3], (float)direction [6]);
		directionCosineY = new Vector3 ((float)direction [1], (float)direction [4], (float)direction [7]);

		sliceNormal = Vector3.Cross (directionCosineX, directionCosineY);

		if (lastSlice != null) {

			// Get the origins of the two images:
			VectorDouble o1 = firstSlice.GetOrigin();
			if ( o1.Count < 3) {
				throw( new System.Exception ("Invalid origins found in first image."));
			}
			Vector3 origin = new Vector3 ((float)o1 [0], (float)o1 [1], (float)o1 [2]);
			VectorDouble o2 = lastSlice.GetOrigin ();
			if ( o2.Count < 3) {
				throw( new System.Exception ("Invalid origins found in last image."));
			}
			Vector3 lastOrigin = new Vector3 ((float)o2 [0], (float)o2 [1], (float)o2 [2]);

			// Calculate offset between two adjacent slices (assuming all neighbours are the same distance apart):
			// Note: I expect sliceOffset.x and sliceOffset.y to be zero most of the time.
			//              Using a Vector just for completeness.
			sliceOffset = (lastOrigin - origin) / (numberOfSlices - 1);
		}

		if (lastSlice != null && numberOfSlices > 1) {
			// Load the direction cosines:
			// ITK stores the direction cosines in a matrix with row-major-ordering. The weird indexing is because
			// we need the first and second column (0,3,6 for X and 1,4,7 for Y)
			VectorDouble directionLast = lastSlice.GetDirection ();
			if (directionLast.Count < 6)
				throw(new System.Exception ("Invalid direction cosines found in images."));
			Vector3 directionCosineXLast = new Vector3 ((float)directionLast [0], (float)directionLast [3], (float)directionLast [6]);
			Vector3 directionCosineYLast = new Vector3 ((float)directionLast [1], (float)directionLast [4], (float)directionLast [7]);

			Vector3 sliceNormalLast = Vector3.Cross (directionCosineXLast, directionCosineYLast);

			// If the first and last slice have the same orientation, then consider this series to be a volume.
			// TODO: Better check?
			if ((sliceNormal == sliceNormalLast)) {
				isConsecutiveVolume = true;
			} else {
				Debug.LogWarning ("First and last slice of the series do not have the same orientation. This will not be considered a volume.\n" +
				"\tNormal first slice, Normal last slice: " + sliceNormal + " " + sliceNormalLast);
			}

		} else {
			isConsecutiveVolume = false;
		}

		if( isConsecutiveVolume ) {
			// Calculate which direction the normal is facing to determine the orienation (Transverse,
			// Coronal or Saggital).
			float absX = Mathf.Abs (sliceNormal.x);
			float absY = Mathf.Abs (sliceNormal.y);
			float absZ = Mathf.Abs (sliceNormal.z);
			if (absX > absY && absX > absZ) {
				sliceOrientation = SliceOrientation.Saggital;
			} else if (absY > absX && absY > absZ) {
				sliceOrientation = SliceOrientation.Coronal;
			} else if (absZ > absX && absZ > absY) {
				sliceOrientation = SliceOrientation.Transverse;
			} else {
				sliceOrientation = SliceOrientation.Unknown;
			}
		} else {
			sliceOrientation = SliceOrientation.Unknown;	// Can't know what the orientation is if the first and last slice have different normals
		}

		// Read the minimum and maximum values which are stored in this image:
		minPixelValue = UInt16.MinValue;
		maxPixelValue = UInt16.MaxValue;
		foundMinMaxPixelValues = false;
		try {
			minPixelValue = UInt32.Parse( firstSlice.GetMetaData("0028|0106") );
			maxPixelValue = UInt32.Parse( firstSlice.GetMetaData("0028|0107") );
			foundMinMaxPixelValues = true;
		} catch {
		}
	}

	/*! Get a human readable description of this series */
	public string getDescription()
	{
		try{
			// If the description was already generated earlier, re-use it:
			if (description != null && description.Length > 0)
				return description;
			description = "";

			string modality = "";
			string acquisitionContextDescription = "";
			string seriesDescription = "";
			string imageComment = "";
			string bodyPartExamined = "";
			string acquisitionDate = "";

			try {
				modality = firstSlice.GetMetaData ("0008|0060");
			} catch {
			}
			try {
				acquisitionContextDescription = firstSlice.GetMetaData ("0040|0556");
			} catch {
			}
			try {
				seriesDescription = firstSlice.GetMetaData ("0008|103E");
			} catch {
			}
			try {
				imageComment = firstSlice.GetMetaData ("0020|4000");
			} catch {
			}
			try {
				bodyPartExamined = firstSlice.GetMetaData ("0018|0015");
			} catch {
			}
			try {
				acquisitionDate = firstSlice.GetMetaData ("0008|0022");
				if (acquisitionDate != null) {
					DateTime time = DateTime.ParseExact (acquisitionDate, "yyyyMMdd",
						                System.Globalization.CultureInfo.InvariantCulture);
					acquisitionDate = time.ToString ("dd MMMM yyyy");
				}
			} catch {
			}


			if (modality.Length > 0)
				description += "[" + modality + "]";

			if (bodyPartExamined.Length > 0)
				description += " " + bodyPartExamined;
			description += " " + sliceOrientation;
			if (acquisitionDate != null && acquisitionDate.Length > 0)
				description += ", " + acquisitionDate;
			description += " (" + numberOfSlices + " images)";

			if (imageComment != null && imageComment.Length > 0)
				description += "\n<color=#dddddd>\t" + imageComment + "</color>";
			else if (seriesDescription != null && seriesDescription.Length > 0)
				description += "\n<color=#dddddd>\t" + seriesDescription + "</color>";
			else if (acquisitionContextDescription != null && acquisitionContextDescription.Length > 0)
				description += "\n<color=#dddddd>\t" + acquisitionContextDescription + "</color>";

		} catch {
			description = "Failed to generate DICOM description (valid DICOM?).";
		}
		return description;
	}

	/*! If header did not contain information about minimum/maximum pixel values, this can be used to set them.
	 * \note Should only be called once. Will be called when the first slice of this series is loaded. */
	public void setMinMaxPixelValues( UInt32 min, UInt32 max )
	{
		minPixelValue = min;
		maxPixelValue = max;
		foundMinMaxPixelValues = true;
	}
}
