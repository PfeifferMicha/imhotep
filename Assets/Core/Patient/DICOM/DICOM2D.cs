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


	public DICOM2D( DICOMSeries seriesInfo, int slice ) : base( seriesInfo )
	{
		dimensions = 2;
		slice = Mathf.Clamp (slice, 0, seriesInfo.filenames.Count - 1);
		this.slice = slice;
		loadImageData (this.slice);
	}


	/*! Loads a single file and creates an array of colors from the pixels.
 	 * The array of colors can later be used to generate a texture, see getTexture2D() */
	private void loadImageData( int slice )
	{
		VectorString fileNames = seriesInfo.filenames;

		// Read the DICOM image:
		Image image = SimpleITK.ReadImage( fileNames[slice] );

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


		UInt32 min = UInt32.MaxValue;
		UInt32 max = UInt32.MinValue;

		// Copy the image into a colors array:
		IntPtr bufferPtr;
		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight ();
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			bufferPtr = image.GetBufferAsUInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );
			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 y = 0; y < texHeight; y++) {
				for (UInt32 x = 0; x < texWidth; x++) {
					if( x < origTexWidth && y < origTexHeight )// && z < origTexDepth )
					{
						UInt16 pixelValue = (UInt16)((colorsTmp [index] - intercept) / slope);
						colors [ x + y * texWidth] = F2C(pixelValue);

						if (pixelValue > max)
							max = pixelValue;
						if (pixelValue < min)
							min = pixelValue;

						index ++;
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			bufferPtr = image.GetBufferAsInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 y = 0; y < texHeight; y++) {
				for (UInt32 x = 0; x < texWidth; x++) {
					if( x < origTexWidth && y < origTexHeight )// && z < origTexDepth )
					{
						UInt16 pixelValue = (UInt16)((colorsTmp [index] - intercept) / slope);
						colors [ x + y * texWidth] = F2C(pixelValue);

						if (pixelValue > max)
							max = pixelValue;
						if (pixelValue < min)
							min = pixelValue;

						index ++;
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt32 ) {
			bufferPtr = image.GetBufferAsInt32 ();

			Int32[] colorsTmp = new Int32[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 y = 0; y < texHeight; y++) {
				for (UInt32 x = 0; x < texWidth; x++) {
					if( x < origTexWidth && y < origTexHeight )// && z < origTexDepth )
					{
						UInt32 pixelValue = (UInt32)((colorsTmp [index] - intercept) / slope);
						colors [ x + y * texWidth ] = F2C(pixelValue);

						if (pixelValue > max)
							max = pixelValue;
						if (pixelValue < min)
							min = pixelValue;

						index ++;
					}
				}
			}
		} else {
			throw(new System.Exception ("Unsupported pixel format: " + image.GetPixelID()));
		}

		// If the DICOM header did not contain info about the minimum/maximum values and no one
		// has manually set them yet, set the min/max values found for this slice:
		if (!seriesInfo.foundMinMaxPixelValues) {
			seriesInfo.setMinMaxPixelValues (min, max);
		}
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
		return texture2D;
	}
}

