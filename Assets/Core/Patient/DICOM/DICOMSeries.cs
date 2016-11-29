
using itk.simple;
using UnityEngine;
using System;

/*! Information needed for a DICOM series.
 * \note This class assumes that a single series is made up of a single volume (or single slice).
 * 		This means that the direction cosines for each image in the volume are assumed to be the same.
 * \note This class assumes that the spacing in the series is the same between each pair of adjacent slices.*/
public class DICOMSeries {

	public int numberOfSlices { private set; get; }
	public VectorString filenames { private set; get; }
	public string seriesUID { private set; get; }

	public Image firstSlice { private set; get; }
	public Image lastSlice { private set; get; }

	public Matrix4x4 pixelToPatient { private set; get; }
	public Matrix4x4 patientToPixel { private set; get; }

	public Vector3 directionCosineX { private set; get; }
	public Vector3 directionCosineY { private set; get; }

	public Vector3 origin { private set; get; }
	public Vector2 pixelSpacing { private set; get; }

	/*! minPixelValue of first slice: */
	public int minPixelValue { private set; get; }
	/*! maxPixelValue of first slice: */
	public int maxPixelValue { private set; get; }

	public bool foundMinMaxPixelValues { private set; get; }

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

		pixelToPatient = transformMatrix;

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
			transformPixelToPatientPos */
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
			transformPixelToPatientPos */
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
		return seriesUID + " (" + numberOfSlices + " images)";
	}

	public void setMinMaxPixelValues( int min, int max )
	{
		minPixelValue = min;
		maxPixelValue = max;
		foundMinMaxPixelValues = true;
	}
}
