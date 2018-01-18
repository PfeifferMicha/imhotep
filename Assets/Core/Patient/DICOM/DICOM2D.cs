using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;
using System.Collections.Generic;

/*! DICOM Slice, may be part of a volume. */
public class DICOM2D : DICOM
{
	/*! The slice this DICOM image represents.*/
	public int slice { private set; get; }

	/*! Raw color values */
	public Color32[] colors;

	/*! Texture of the slice. The texture will be generated when this is first called.
	 * \note This may only be called if dimension == 2, otherwise it will throw an error.*/
	private Texture2D texture2D;

	/*! Approximate slice orientation of the slices in this series.
	 * Returns coronal, saggital or transverse depending on which way the normal of the slices
	 * is facing.
	 * \note This is only an approximation. "Intermediate" orientations are "rounded" to the nearest
	 * 		orientation.
	 *		This means that, for example, if a series is a transverse series then this function will
	 *		correctly return "transverse". However, if it is slightly tilted away from the transverse
	 *		orientation, this will still return transverse. */
	public SliceOrientation sliceOrientation { private set; get; }

	public DICOM2D( DICOMSeries seriesInfo, int slice ) : base( seriesInfo )
	{
		dimensions = 2;
		slice = Mathf.Clamp (slice, 0, seriesInfo.filenames.Count - 1);
		this.slice = slice;

		VectorString fileNames = seriesInfo.filenames;

		// Read the DICOM image:
		image = SimpleITK.ReadImage( fileNames[slice] );
		loadImageData (image);

		VectorDouble o1 = image.GetOrigin();
		if ( o1.Count < 3) {
			throw( new System.Exception ("Invalid origins found in first image."));
		}
		origin = new Vector3 ((float)o1 [0], (float)o1 [1], (float)o1 [2]);

		// Load the direction cosines:
		// ITK stores the direction cosines in a matrix with row-major-ordering. The weird indexing is because
		// we need the first and second column (0,3,6 for X and 1,4,7 for Y)
		VectorDouble direction = image.GetDirection();
		if( direction.Count < 6 )
			throw( new System.Exception ("Invalid direction cosines found in images."));
		directionCosineX = new Vector3 ((float)direction [0], (float)direction [3], (float)direction [6]);
		directionCosineY = new Vector3 ((float)direction [1], (float)direction [4], (float)direction [7]);

		sliceNormal = Vector3.Cross (directionCosineX, directionCosineY);

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

		// Load the pixel spacing:
		// NOTE: It seems that the the first value is the spacing between rows (i.e. y direction),
		//		the second value is the spacing between columns (i.e. x direction).
		//		I was not able to verify this so far, since all test dicoms we had have the same spacing in
		//		x and y direction...
		VectorDouble spacing = image.GetSpacing();
		if( spacing.Count < 2 )
			throw( new System.Exception ("Invalid pixel spacing found in images."));
		pixelSpacing = new Vector2 ((float)spacing [1], (float)spacing [0] );

		// Generate the transformation matrices which can later be used to translate pixels to
		// 3D positions and vice versa.
		setupTransformationMatrices ();
	}


	/*! Loads a single file and creates an array of colors from the pixels.
 	 * The array of colors can later be used to generate a texture, see getTexture2D() */
	private void loadImageData( Image image )
	{

		origTexWidth = (int)image.GetWidth ();
		origTexHeight = (int)image.GetHeight ();
		origTexDepth = 1;
		texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		texDepth = 1;
		texPaddingWidth = texWidth - origTexWidth;
		texPaddingHeight = texHeight - origTexHeight;
		texPaddingDepth = texDepth - origTexDepth;
		colors = new Color32[ texWidth * texHeight ];

		int intercept = 0;
		int slope = 1;
		try {
			intercept = Int32.Parse( image.GetMetaData("0028|1052") );
			slope = Int32.Parse( image.GetMetaData("0028|1053") );
		} catch {
		}

		if (image.GetDimension () != 2 && image.GetDimension () != 3)
		{
			throw( new System.Exception( "Only 2D and 3D images are currently supported. Dimensions of image: " + image.GetDimension()));
		}

		Debug.Log ("Pixel format: " + image.GetPixelID ());
		Debug.Log ("Slope, Intercept: " + slope + " " + intercept);

		UInt32 min = UInt32.MaxValue;
		UInt32 max = UInt32.MinValue;

		// TODO: Ignore slope and intercept??

		// Copy the image into a colors array:
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			IntPtr bufferPtr = image.GetBufferAsUInt16 ();

			unsafe {
				UInt16 *ptr = (UInt16 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;
				//for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if (x < origTexWidth && y < origTexHeight) {// && z < origTexDepth )
							long jumpingIndex =  x + y * texWidth;

							UInt32 pixelValue = (UInt32)((UInt16)ptr[consecutiveIndex]);
							colors [jumpingIndex] = F2C (pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							consecutiveIndex++;
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			IntPtr bufferPtr = image.GetBufferAsInt16 ();

			unsafe {
				Int16 *ptr = (Int16 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;
				//for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if (x < origTexWidth && y < origTexHeight) {// && z < origTexDepth )
							long jumpingIndex =  x + y * texWidth;

							UInt32 pixelValue = (UInt32)((Int16)ptr[consecutiveIndex] + Int16.MaxValue);
							colors [jumpingIndex] = F2C (pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							consecutiveIndex++;
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt32 ) {
			IntPtr bufferPtr = image.GetBufferAsInt32 ();

			unsafe {
				Int32 *ptr = (Int32 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;
				//for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if (x < origTexWidth && y < origTexHeight) {// && z < origTexDepth )
							long jumpingIndex =  x + y * texWidth;

							// TODO: To move from Int32 to UInt32 range, we should add Int32.MaxValue?!
							// However, when we do this, 
							UInt32 pixelValue = (UInt32)((Int32)ptr[consecutiveIndex]) + (UInt32)Int16.MaxValue;
							colors [jumpingIndex] = F2C (pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							consecutiveIndex++;
						}
					}
				}
			}
		} else {
			throw(new System.Exception ("Unsupported pixel format: " + image.GetPixelID()));
		}

		seriesInfo.setMinMaxPixelValues (min, max);
		// Make the loaded image accessable from elsewhere:
		this.image = image;
	}


	/*! Returns the image as a texture.
	 * If the texture does not already exist, this function creates it. */
	public Texture2D getTexture2D()
	{
		if (texture2D != null)
			return texture2D;

		if (dimensions != 2) {
			throw(new System.Exception ("Trying to get Texture2D from a DICOM which has " + dimensions + " dimensions!"));
		}

		texture2D = new Texture2D (texWidth, texHeight, TextureFormat.ARGB32, false, true);
		texture2D.SetPixels32 (colors);
		texture2D.Apply ();

		// Clear no longer needed data:
		colors = null;

		return texture2D;
	}
}

