using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;
using System.Collections.Generic;


public class DICOM3D : DICOM
{
	/*! Raw color values */
	public Color32[] colors;

	private Texture3D texture3D;

	public DICOM3D ( DICOMSeries seriesInfo ) : base( seriesInfo )
	{
		dimensions = 3;
		loadVolumeData ();
	}

	/*! Load the entire series (i.e. the entire volume).
	 * Unlike loadImageData(), this function does not create a colors array or texture. To
	 * access volume data, simply get the image (DICOM.image) and read pixel values from it. */
	private void loadVolumeData()
	{
		// Get all file names for the series:
		VectorString fileNames = seriesInfo.filenames;

		// Create a reader which will read the whole series:
		ImageSeriesReader reader = new ImageSeriesReader ();
		reader.SetFileNames (fileNames);
		// Load the entire image into a series:
		Image image = reader.Execute();

		origTexWidth = (int)image.GetWidth ();
		origTexHeight = (int)image.GetHeight ();
		origTexDepth = (int)image.GetDepth ();
		texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		texDepth = Mathf.NextPowerOfTwo ((int)image.GetDepth ());
		texPaddingWidth = texWidth - origTexWidth;
		texPaddingHeight = texHeight - origTexHeight;
		texPaddingDepth = texDepth - origTexDepth;
		Debug.Log ("Original texture dimensions: " + origTexWidth + " " + origTexHeight + " " + origTexDepth);
		Debug.Log ("Texture dimensions: " + texWidth + " " + texHeight + " " + texDepth);
		colors = new Color32[ texWidth * texHeight * texDepth ];

		int intercept = 0;
		int slope = 1;
		try {
			intercept = Int32.Parse( image.GetMetaData("0028|1052") );
			slope = Int32.Parse( image.GetMetaData("0028|1053") );
		} catch {
		}
		Debug.Log ("Slope: " + slope + " Intercept: " + intercept);

		if (image.GetDimension () != 3)
		{
			throw( new System.Exception( "Cannot load volume: Image needs to be 3D. Dimensions of image: " + image.GetDimension()));
		}

		seriesInfo.histogram = new Histogram ();

		UInt32 min = UInt32.MaxValue;
		UInt32 max = UInt32.MinValue;

		Debug.Log ("Pixel format: " + image.GetPixelID ());

		// Copy the image into a colors array:
		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight ();
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			IntPtr bufferPtr = image.GetBufferAsUInt16 ();

			unsafe {
				UInt16 *ptr = (UInt16 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;
				for (UInt32 z = 0; z < texDepth; z++) {
					for (UInt32 y = 0; y < texHeight; y++) {
						for (UInt32 x = 0; x < texWidth; x++) {
							if (x < origTexWidth && y < origTexHeight && z < origTexDepth ) {
								long jumpingIndex = x + y * texWidth + z*texWidth*texHeight;

								UInt32 pixelValue = (UInt32)((UInt16)ptr [consecutiveIndex]);
								colors [jumpingIndex] = F2C (pixelValue);

								if (pixelValue > max)
									max = pixelValue;
								if (pixelValue < min)
									min = pixelValue;
								
								seriesInfo.histogram.addValue (pixelValue);

								consecutiveIndex++;
							}
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			IntPtr bufferPtr = image.GetBufferAsInt16 ();

			unsafe {
				Int16 *ptr = (Int16 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;
				for (UInt32 z = 0; z < texDepth; z++) {
					for (UInt32 y = 0; y < texHeight; y++) {
						for (UInt32 x = 0; x < texWidth; x++) {
							if (x < origTexWidth && y < origTexHeight && z < origTexDepth ) {
								long jumpingIndex = x + y * texWidth + z*texWidth*texHeight;

								UInt32 pixelValue = (UInt32)((Int16)ptr[consecutiveIndex] + Int16.MaxValue);
								colors [jumpingIndex] = F2C (pixelValue);

								if (pixelValue > max)
									max = pixelValue;
								if (pixelValue < min)
									min = pixelValue;

								seriesInfo.histogram.addValue (pixelValue);

								consecutiveIndex++;
							}
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt32 ) {
			IntPtr bufferPtr = image.GetBufferAsInt32 ();

			unsafe {
				Int32 *ptr = (Int32 *)bufferPtr.ToPointer();

				int consecutiveIndex = 0;

				for (UInt32 z = 0; z < texDepth; z++) {
					for (UInt32 y = 0; y < texHeight; y++) {
						for (UInt32 x = 0; x < texWidth; x++) {
							if (x < origTexWidth && y < origTexHeight && z < origTexDepth ) {
								long jumpingIndex = x + y * texWidth + z*texWidth*texHeight;

								// TODO: To move from Int32 to UInt32 range, we should add Int32.MaxValue?!
								// However, when we do this, 
								UInt32 pixelValue = (UInt32)((Int32)ptr[consecutiveIndex]) + (UInt32)Int16.MaxValue;
								colors [jumpingIndex] = F2C (pixelValue);

								if (pixelValue > max)
									max = pixelValue;
								if (pixelValue < min)
									min = pixelValue;

								seriesInfo.histogram.addValue (pixelValue);

								consecutiveIndex++;
							}
						}
					}
				}
			}
		} else {
			throw(new System.Exception ("Unsupported pixel format: " + image.GetPixelID()));
		}
		/*IntPtr bufferPtr;
		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight () * image.GetDepth();
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			bufferPtr = image.GetBufferAsUInt16 ();

			UInt16[] colorsTmp = new UInt16[ numberOfPixels ];
			Int16[] tmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, tmp, 0, (int)numberOfPixels );
			System.Buffer.BlockCopy (tmp, 0, colorsTmp, 0, (int)numberOfPixels);

			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						//long consecutiveIndex = (texWidth-1-x) + y*texWidth + z*texWidth*texHeight;
						long consecutiveIndex =  x + y * texWidth + z*texWidth*texHeight;
						if (x < origTexWidth && y < origTexHeight && z < origTexDepth) {
							UInt16 pixelValue = (UInt16)((colorsTmp [index] - intercept) / slope);
							colors [consecutiveIndex] = F2C (pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							seriesInfo.histogram.addValue (pixelValue);

							index++;
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			bufferPtr = image.GetBufferAsInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						//long consecutiveIndex = (texWidth-1-x) + y*texWidth + z*texWidth*texHeight;
						long consecutiveIndex =  x + y * texWidth + z*texWidth*texHeight;
						if (x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							//Int16 pixelValueInt16 = (Int16)((colorsTmp [index] - intercept) / slope);
							//UInt32 pixelValue = (UInt32)((int)pixelValueInt16 + 32768);

							UInt16 pixelValue = (UInt16)((colorsTmp [index] - intercept) / slope);
							colors [ consecutiveIndex] = F2C(pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							seriesInfo.histogram.addValue (pixelValue);

							index++;
						}
					}
				}
			}
		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt32 ) {
			bufferPtr = image.GetBufferAsInt32 ();

			Int32[] colorsTmp = new Int32[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			//for (UInt32 z = 0; z < texDepth; z++) {
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						//long consecutiveIndex = (texWidth-1-x) + y*texWidth + z*texWidth*texHeight;
						long consecutiveIndex =  x + y * texWidth + z*texWidth*texHeight;
						if (x < origTexWidth && y < origTexHeight && z < origTexDepth) {
							Int32 pixelValueInt32 = (Int32)((colorsTmp [index] - intercept) / slope);

							UInt32 pixelValue = (UInt32)((Int64)pixelValueInt32 + Int32.MaxValue);
							colors [ consecutiveIndex ] = F2C(pixelValue);

							if (pixelValue > max)
								max = pixelValue;
							if (pixelValue < min)
								min = pixelValue;

							seriesInfo.histogram.addValue (pixelValue);

							index++;
						}
					}
				}
			}
		} else {
			throw(new System.Exception ("Unsupported pixel format: " + image.GetPixelID()));
		}*/

		// Manually set the min and max values, because we just caculated them for the whole volume and
		// can thus be sure that we have the correct values:
		seriesInfo.setMinMaxPixelValues (min, max);


		seriesInfo.histogram.setMinMaxPixelValues (min, max);

		// Make the loaded image accessable from elsewhere:
		this.image = image;


		Debug.Log ("Loaded.");
	}

	public Texture3D getTexture3D()
	{
		if( texture3D != null )
			return texture3D;



		Debug.Log ("Generating DICOM texture:");
		texture3D = new Texture3D( texWidth, texHeight, texDepth, TextureFormat.ARGB32, false);
		texture3D.SetPixels32(colors); //needs around 0.15 sec for a small DICOM, TODO coroutine?
		texture3D.Apply();
		texture3D.wrapMode = TextureWrapMode.Clamp;

		Debug.Log ("\tGenerated DICOM texture.");
		return texture3D;
	}
}

