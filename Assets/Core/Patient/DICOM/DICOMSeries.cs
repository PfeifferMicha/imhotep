
using itk.simple;
using UnityEngine;
using System;

/*! Information needed for a DICOM series.
 * \note This class assumes that a single series is made up of a single volume (or single slice).
 * 		This means that the direction cosines for each image in the volume are assumed to be the same.
 * \note This class assumes that the spacing in the series is the same between each pair of adjacent slices.*/
public class DICOMSeries {

	public enum SliceOrientation
	{
		Transverse,
		Saggital,
		Coronal,
		Unknown
	}

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

	/*! Matrix to transform from a pixel/layer coordinate to the patient coordinate system.
	 * \sa patientToPixel */
	public Matrix4x4 pixelToPatient { private set; get; }

	/*! Matrix to transform from the patient coordinate system to a pixel/layer coordinate.
	 * Inverse of pixelToPatient.
	 * \sa pixelToPatient */
	public Matrix4x4 patientToPixel { private set; get; }

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

	/*! The plane normal of the slices in this series (result of cross vector of the direction cosines). */
	public Vector3 sliceNormal { private set; get; }

	/*! Approximate slice orientation of the slices in this series.
	 * Returns coronal, saggital or transverse depending on which way the normal of the slices
	 * is facing.
	 * \note This is only an approximation. "Intermediate" orientations are "rounded" to the nearest
	 * 		orientation.
	 *		This means that, for example, if a series is a transverse series then this function will
	 *		correctly return "transverse". However, if it is slightly tilted away from the transverse
	 *		orientation, this will still return transverse. */
	public SliceOrientation sliceOrientation { private set; get; }

	/*! Position of center of first voxel in this series*/
	public Vector3 origin { private set; get; }
	/*! Distance between rows and columns in images of this series.*/
	public Vector2 pixelSpacing { private set; get; }

	/*! Minimal pixel value of first slice
	 * \note If possible, this is read from the DICOM header of the first slice.
	 *		However, this value may not be present, in which case this is filled when a slice is
	 *		first loaded (by checking all pixel values and finding the minimum).*/
	public int minPixelValue { private set; get; }
	/*! Maximal pixel value of first slice
	 * \note If possible, this is read from the DICOM header of the first slice.
	 *		However, this value may not be present, in which case this is filled when a slice is
	 *		first loaded (by checking all pixel values and finding the maximum).*/
	public int maxPixelValue { private set; get; }

	/*! True if the minPixelValue and maxPixelValue have been found or calculated, false otherwise. */
	public bool foundMinMaxPixelValues { private set; get; }

	/*! Cached human-readable description string.*/
	private string description = null;

	/*! Constructor, fills most of the attributes of the DICOMSeries class.
	 * \note This does some heavy file/directory parsing to determine the files which are part of
	 * 		this series and their order. This is why the DICOMSeries should be constructed in a
	 * 		background thread and then passed to the main thread. */
	public DICOMSeries( string directory, string seriesUID )
	{
		// Get the file names for the series:
		filenames = ImageSeriesReader.GetGDCMSeriesFileNames( directory, seriesUID );
		if (filenames.Count <= 0) {
			throw( new System.Exception ("No files found for series " + seriesUID + "."));
		}
		this.seriesUID = seriesUID;

		// Load the first slice in volume to get meta information:
		firstSlice = SimpleITK.ReadImage( filenames[0] );
		VectorDouble o1 = firstSlice.GetOrigin();
		if ( o1.Count < 3) {
			throw( new System.Exception ("Invalid origins found in first image."));
		}
		origin = new Vector3 ((float)o1 [0], (float)o1 [1], (float)o1 [2]);

		numberOfSlices = filenames.Count;

		// Offset between two adjacent slices. If only one slice is present,
		// this defaults to zero.
		Vector3 sliceOffset = Vector3.zero;

		// If we have more than one slice, also load the last slice to be able to determine the slice spacing:
		if( filenames.Count > 1 )
		{
			lastSlice = SimpleITK.ReadImage( filenames[filenames.Count - 1] );

			// Get the origins of the two images:
			VectorDouble o2 = lastSlice.GetOrigin ();
			if ( o2.Count < 3) {
				throw( new System.Exception ("Invalid origins found in last image."));
			}
			Vector3 lastOrigin = new Vector3 ((float)o2 [0], (float)o2 [1], (float)o2 [2]);

			// Calculate offset between two adjacent slices (assuming all neighbours are the same distance apart):
			// Note: I expect sliceOffset.x and sliceOffset.y to be zero most of the time.
			//		Using a Vector just for completeness.
			sliceOffset = (lastOrigin - origin) / (filenames.Count - 1);
		}

		// Load the direction cosines:
		VectorDouble direction = firstSlice.GetDirection();
		if( direction.Count < 6 )
			throw( new System.Exception ("Invalid direction cosines found in images."));
		directionCosineX = new Vector3 ((float)direction [0], (float)direction [1], (float)direction [2]);
		directionCosineY = new Vector3 ((float)direction [3], (float)direction [4], (float)direction [5]);

		sliceNormal = Vector3.Cross (directionCosineX, directionCosineY);

		// Calculate the which direction the normal is facing to determine the orienation (Transverse,
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

		// Load the direction cosines:
		// NOTE: It seems that the the first value is the spacing between rows (i.e. y direction),
		//		the second value is the spacing between columns (i.e. x direction).
		//		I was not able to verify this so far, since all test dicoms we had have the same spacing in
		//		x and y direction...
		VectorDouble spacing = firstSlice.GetSpacing();
		if( spacing.Count < 2 )
			throw( new System.Exception ("Invalid direction cosines found in images."));
		pixelSpacing = new Vector2 ((float)spacing [0], (float)spacing [1] );

		// Set up the transformation matrix:
		Matrix4x4 transformMatrix = new Matrix4x4 ();
		// Column 1:
		transformMatrix [0, 0] = directionCosineX.x * pixelSpacing.x;
		transformMatrix [1, 0] = directionCosineX.y * pixelSpacing.x;
		transformMatrix [2, 0] = directionCosineX.z * pixelSpacing.x;
		transformMatrix [3, 0] = 0f;
		// Column 2:
		transformMatrix [0, 1] = directionCosineY.x * pixelSpacing.y;
		transformMatrix [1, 1] = directionCosineY.y * pixelSpacing.y;
		transformMatrix [2, 1] = directionCosineY.z * pixelSpacing.y;
		transformMatrix [3, 1] = 0f;
		// Column 3:
		transformMatrix [0, 2] = sliceOffset.x;
		transformMatrix [1, 2] = sliceOffset.y;
		transformMatrix [2, 2] = sliceOffset.z;
		transformMatrix [3, 2] = 0f;
		// Column 4:
		transformMatrix [0, 3] = origin.x;
		transformMatrix [1, 3] = origin.y;
		transformMatrix [2, 3] = origin.z;
		transformMatrix [3, 3] = 1f;

		// Convert to a the left-hand-side coordinate system which Unity uses:
		Matrix4x4 rightHandToLeftHand = new Matrix4x4 ();
		rightHandToLeftHand [0, 0] = -1f;
		rightHandToLeftHand [1, 1] = -1f;
		rightHandToLeftHand [2, 2] = -1f;
		rightHandToLeftHand [3, 3] = 1f;

		pixelToPatient = rightHandToLeftHand*transformMatrix;

		// Inverse transformation:
		patientToPixel = pixelToPatient.inverse;


		// Read the minimum and maximum values which are stored in this image:
		minPixelValue = UInt16.MinValue;
		maxPixelValue = UInt16.MaxValue;
		foundMinMaxPixelValues = false;
		try {
			minPixelValue = Int32.Parse( firstSlice.GetMetaData("0028|0106") );
			maxPixelValue = Int32.Parse( firstSlice.GetMetaData("0028|0107") );
			foundMinMaxPixelValues = true;
		} catch {
		}
	}

	/*! Transforms a 2D pixel on a given layer to the 3D patient coordinate system.
	 * \note Both pixel and layer may be continuous, i.e. positions between pixels or
	 * 		layers can be given as well as exact pixel/layer values. */
	public Vector3 transformPixelToPatientPos( Vector2 pixel, float layer )
	{
		Vector4 p = new Vector4 (pixel.x, pixel.y, layer, 1f);
		Vector4 pos = pixelToPatient * p;
		return new Vector3 (pos.x, pos.y, pos.z);
	}

	/*! Transform a 3D position in the patient coordinate system to a pixel.
	 * The z component of the returned vector is the slice number.
	 * \note The returned position is continuous, i.e. to get an actual pixel value,
	 * 		one must round the result.
	 * \sa transformPatientPosToDiscretePixel
	 *		transformPixelToPatientPos */
	public Vector3 transformPatientPosToPixel( Vector3 pos )
	{
		Vector4 p = new Vector4 (pos.x, pos.y, pos.z, 1f);
		Vector4 pixel = patientToPixel * p;
		return new Vector3 (pixel.x, pixel.y, pixel.z);
	}

	/*! Transform a 3D position in the patient coordinate system to a pixel.
	 * The z component of the returned vector is the slice number.
	 * \note The returned position is rounded to the nearest pixel/slice.
	 * \sa transformPatientPosToPixel
	 *		transformPixelToPatientPos */
	public Vector3 transformPatientPosToDiscretePixel( Vector3 pos )
	{
		Vector3 pixel = transformPatientPosToPixel( pos );
		Vector3 rounded = new Vector3 (Mathf.Round (pixel.x),
			Mathf.Round (pixel.y),
			Mathf.Round (pixel.z));
		return rounded;
	}


	/*! Get a human readable description of this series */
	public string getDescription()
	{
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

		try{
			modality = firstSlice.GetMetaData("0008|0060");
		} catch {}
		try{
			acquisitionContextDescription = firstSlice.GetMetaData("0040|0556");
		} catch {}
		try{
			seriesDescription = firstSlice.GetMetaData("0008|103E");
		} catch {}
		try{
			imageComment = firstSlice.GetMetaData("0020|4000");
		} catch {}
		try{
			bodyPartExamined = firstSlice.GetMetaData("0018|0015");
		} catch {}
		try{
			acquisitionDate = firstSlice.GetMetaData("0008|0022");
			if( acquisitionDate != null )
			{
				DateTime time = DateTime.ParseExact(acquisitionDate, "yyyyMMdd",
					System.Globalization.CultureInfo.InvariantCulture);
				acquisitionDate = time.ToString("dd MMMM yyyy");
			}
		} catch {}


		if( modality.Length > 0 )
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


		return description; 
	}

	/*! If header did not contain information about minimum/maximum pixel values, this can be used to set them.
	 * \note Should only be called once. Will be called when the first slice of this series is loaded. */
	public void setMinMaxPixelValues( int min, int max )
	{
		minPixelValue = min;
		maxPixelValue = max;
		foundMinMaxPixelValues = true;
	}
}
