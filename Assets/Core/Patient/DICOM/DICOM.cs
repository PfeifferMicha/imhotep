using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;
using System.Collections.Generic;

/*! Represents a DICOM Image (2D, single slice) or Volume (3D, multi slice). */
public class DICOM {

	/*! A reference to the DICOM series this DICOM is part of.
	 * This can be thought of as a header for the entire series and can be
	 * used to transform from 2D to 3D positions and vice versa. */
	public DICOMSeries seriesInfo { private set; get; }
	/*! Number of dimensions in this DICOM (2 for slice, 3 for volume).
	 * \note If dimensions == 2, this does not mean that there aren't any other
	 * 		slices, it only means that this image represents a single slice.
	 * 		To get access to the number of slices in the series, see the seriesInfo.*/
	public int dimensions { protected set; get; }
	public int texWidth { protected set; get; }
	public int texHeight { protected set; get; }
	public int texDepth { protected set; get; }

	/*! Width (in Pixels) in DICOM slice*/
	public int origTexWidth { protected set; get; }
	/*! Height (in Pixels) in DICOM slice*/
	public int origTexHeight { protected set; get; }
	/*! Depth (in Pixels) in DICOM volume*/
	public int origTexDepth { protected set; get; }

	/*! Unused texture width which results from unity needing Power-Of-Two texture sizes */
	public int texPaddingWidth { protected set; get; }
	/*! Unused texture height which results from unity needing Power-Of-Two texture sizes */
	public int texPaddingHeight { protected set; get; }
	/*! Unused texture depth which results from unity needing Power-Of-Two texture sizes */
	public int texPaddingDepth { protected set; get; }

	/*! The ITK image.
	 * Can be used to access the raw pixel data as it is in the file.
	 * Make sure to read the SimpleITK (or the normal ITK) documentation for details on
	 * pixel value types, number of bits etc.
	 * This can also be used to access header information through image.GetMetaData().*/
	public Image image { protected set; get; }

	/*! Position of center of first voxel in this series*/
	public Vector3 origin { protected set; get; }
	/*! Distance between rows and columns in images of this series.*/
	public Vector2 pixelSpacing { protected set; get; }

	/*! Assuming this series is a volume of consecutive slices, where neighbouring slices always have
	 * the same offset between each other, this offset is stored in sliceOffset.
	 * For 2D slices, this is zero. */
	public Vector3 sliceOffset { protected set; get; }

	/*! The direction cosine of a row of this image.
	 * This can be thought of as a unit-length vector pointing into the direction in which the 
	 * row lies inside the patient coordinate system (i.e. when you walk along the row in 2D,
	 * in which direction would you walk in the patient coordinate system).
	 * See the DICOM standard for more information, or search online for "direction cosine". */
	public Vector3 directionCosineX { protected set; get; }
	/*! The direction cosine of a column of this image.
	 * This can be thought of as a unit-length vector pointing into the direction in which the 
	 * column lies inside the patient coordinate system (i.e. when you walk along the column in 2D,
	 * in which direction would you walk in the patient coordinate system).
	 * See the DICOM standard for more information, or search online for "direction cosine". */
	public Vector3 directionCosineY { protected set; get; }

	/*! The plane normal of the slices in this series (result of cross vector of the direction cosines). */
	public Vector3 sliceNormal { protected set; get; }

	/*! Matrix to transform from a pixel/layer coordinate to the patient coordinate system.
	 * \sa patientToPixel */
	public Matrix4x4 pixelToPatient { protected set; get; }

	/*! Matrix to transform from the patient coordinate system to a pixel/layer coordinate.
	 * Inverse of pixelToPatient.
	 * \sa pixelToPatient */
	public Matrix4x4 patientToPixel { protected set; get; }

	/*! Constructor, loads the DICOM image data from file.
	 * The constructor starts the loading of pixel data from the files (filenames are
	 * taken from the seriesInfo). If slice is zero or positive, only the single file
	 * will be read. If slice is negative, the entire volume (i.e. all files - and thus
	 * all slices) will be read.
	 * \note Since the constructor does so much work, the Object should be created in
	 * 		a background thread and then passed to the main thread. */
	public DICOM( DICOMSeries seriesInfo ) {
		
		// Remember, we will need it later:
		this.seriesInfo = seriesInfo;
	}

	// ===============================================================
	// Transformations:

	/*! Calculate the transformation matrices which can later be used to convert
	 * pixels to 3D positions and vice versa.
	 * \sa transformPixelToPatientPos
	 * \sa transformPatientPosToPixel
	 * \sa transformPatientPosToDiscretePixel */
	public void setupTransformationMatrices()
	{
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
		rightHandToLeftHand [0, 0] = 1f;
		rightHandToLeftHand [1, 1] = 1f;
		rightHandToLeftHand [2, 2] = -1f;
		rightHandToLeftHand [3, 3] = 1f;

		pixelToPatient = rightHandToLeftHand*transformMatrix;

		// Inverse transformation:
		patientToPixel = pixelToPatient.inverse;
	}
	/*! Transforms a 2D pixel on a given layer to the 3D patient coordinate system.
	 * \note Both pixel and layer may be continuous, i.e. positions between pixels or
	 * 		layers can be given as well as exact pixel/layer values. */
	public Vector3 transformPixelToPatientPos( Vector2 pixel, float layer = 0 )
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

	/*! Helper function, converts UInt16 to color */
	public static Color32 F2C(UInt32 value)
	{
		Color32 c = new Color32 ();

		/*c.r = (byte)(value % 256); value /= 256;
		c.g = (byte)(value % 256); value /= 256;
		c.b = (byte)(value % 256); value /= 256;
		c.a = (byte)(value % 256);*/

		/*c.r = (byte)(value % 256); value = value >> 8;
		c.g = (byte)(value % 256); value = value >> 8;
		c.b = (byte)(value % 256); value = value >> 8;
		c.a = (byte)(value % 256);*/
		c.a = (byte)(value >> 24);
		c.b = (byte)(value >> 16);
		c.g = (byte)(value >> 8);
		c.r = (byte)(value);
		//Debug.LogWarning ("c: " + c.r + " " + c.g + " " + c.b + "  " + c.a);

		return c;
	}
	/*! Helper function, converts Int16 to color */
	/*public static Color F2C(Int16 value)
	{
		UInt32 valueUInt = (UInt32)((int)value + 32768);
		return F2C (valueUInt);
	}*/
	/*public static Color F2C(UInt16 value)
	{
		UInt32 valueUInt = (UInt32)(value);
		return F2C (valueUInt);
	}*/
}
