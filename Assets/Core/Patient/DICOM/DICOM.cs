﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;
using System.Collections.Generic;

/*! Represents a DICOM Image (2D, single slice) or Volume (3D, multi slice). */
public class DICOM {
	
	public DICOMSeries seriesInfo { private set; get; }
	public int slice { private set; get; }
	public Color32[] colors;
	public int dimensions { private set; get; }
	public int texWidth { private set; get; }
	public int texHeight { private set; get; }
	public int texDepth { private set; get; }
	public Texture2D texture2D { private set; get; }
	public Image image { private set; get; }

	public DICOM( DICOMSeries seriesInfo, int slice = -1 ) {
		
		// Remember, we will need it later:
		this.seriesInfo = seriesInfo;
		this.slice = slice;

		if (slice == -1) {	// load the entire volume:
			dimensions = 3;
			loadVolumeData ();
		} else {
			// Make sure that 'slice' is a valid slice number:
			slice = Mathf.Clamp (slice, 0, seriesInfo.filenames.Count - 1);
			dimensions = 2;
			loadImageData (slice);
		}
	}

	private void loadImageData( int slice )
	{
		VectorString fileNames = seriesInfo.filenames;

		// Read the DICOM image:
		Image image = SimpleITK.ReadImage( fileNames[slice] );

		int origTexWidth = (int)image.GetWidth ();
		int origTexHeight = (int)image.GetHeight ();
		//int origTexDepth = (int)image.GetDepth ();
		texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		texDepth = 1;
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


		int min = int.MaxValue;
		int max = int.MinValue;

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

	private void loadVolumeData()
	{
		// Get all file names for the series:
		VectorString fileNames = seriesInfo.filenames;

		// Create a reader which will read the whole series:
		ImageSeriesReader reader = new ImageSeriesReader ();
		reader.SetFileNames (fileNames);
		// Load the entire image into a series:
		Image image = reader.Execute();

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

	/*! Helper function, converts UInt16 to color */
	private Color32 F2C(UInt16 value)
	{
		Color32 c = new Color32 ();

		c.r = (byte)(value % 256); value /= 256;
		c.g = (byte)(value % 256); value /= 256;
		c.b = (byte)(value % 256); value /= 256;
		c.a = (byte)(value % 256);
		//Debug.LogWarning ("c: " + c.r + " " + c.g + " " + c.b + "  " + c.a);

		return c;
	}
	/*! Helper function, converts Int16 to color */
	private Color F2C(Int16 value)
	{
		UInt16 valueUInt = (UInt16)((int)value + 32768);
		return F2C (valueUInt);
	}
}
