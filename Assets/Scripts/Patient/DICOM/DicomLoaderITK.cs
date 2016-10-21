using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;
using System.Collections.Generic;

public class DicomLoaderITK
{
	public DicomLoaderITK ()
	{
	}

	//! List of Headers of all series which were found in the directory:
	private List<DICOMHeader> availableSeries = new List<DICOMHeader>();

	//! Holds the directory in which the series are (if series were found):
	private string loadedDirectory = null;

	public void setDirectory( string directory )
	{
		Debug.Log ("Looking for DICOMS in: " + directory);

		// Parse the directory and return the seriesUIDs of all the DICOM series:
		VectorString series = ImageSeriesReader.GetGDCMSeriesIDs (directory);
		if (series.Count > 0) {
			loadedDirectory = directory;
			availableSeries = new List<DICOMHeader>();
			// Load the headers of all available series:
			for (int i = 0; i < series.Count; i++) {
				DICOMHeader newHeader = loadDICOMHeader ( series[i] );
				if( newHeader != null )
					availableSeries.Add (newHeader);
			}
		} else {
			loadedDirectory = null;
		}

		// Debug log:
		string str = "\tFound " + availableSeries.Count + " series.";
		for (int i = 0; i < availableSeries.Count; i++)
			str += "\n\t\t" + availableSeries [i];
		Debug.Log (str);


	}

	private bool existsInCurrentDirectory( string seriesUID )
	{
		if (loadedDirectory == null) {
			return false;
		}
		int index = indexForSeriesUID (seriesUID);
		if (index < 0 || index >= availableSeries.Count) {
			return false;
		}
		return true;
	}

	private DICOMHeader loadDICOMHeader( string seriesUID )
	{
		if (loadedDirectory == null) {
			Debug.LogWarning ("Set the directory before trying to load a series!");
			return null;
		}

		// Get the file names for the series:
		VectorString fileNames = ImageSeriesReader.GetGDCMSeriesFileNames( loadedDirectory, seriesUID );
		if (fileNames.Count <= 0) {
			Debug.LogWarning ("No files found for series " + seriesUID + ".");
			return null;
		}

		// Read the Dicom image series
		//ImageSeriesReader reader = new ImageSeriesReader ();
		//reader.SetFileNames (fileNames);

		Image metaDataImage = SimpleITK.ReadImage( fileNames[0] );
		Debug.Log ("Loading Header: " + fileNames[0]);
		try{
			DICOMHeader header = new DICOMHeader (metaDataImage, fileNames);
			return header;
		} catch( System.Exception e ) {
			Debug.LogError ("Something went wrong while loading: " + fileNames [0] + ". Exception was: " + e.Message + ")");
			return null;
		}
	}

	public DICOMLoadReturnObjectVolume load( int indexToLoad )
	{
		if (loadedDirectory == null) {
			Debug.LogWarning ("Set the directory before trying to load a series!");
			return null;
		}
		if (indexToLoad < 0 || indexToLoad >= availableSeries.Count) {
			Debug.LogWarning ("Could not find series.");
			return null;
		}
		DICOMHeader header = availableSeries [indexToLoad];

		// Get the file names for the series:
		VectorString fileNames = header.FileNames;	//ImageSeriesReader.GetGDCMSeriesFileNames( loadedDirectory, header.SeriesUID );

		if (fileNames.Count <= 0)
			throw(new System.Exception ("No files found for series."));

		// Create a reader which will read the whole series:
		ImageSeriesReader reader = new ImageSeriesReader ();
		reader.SetFileNames (fileNames);
		// Load the entire image into a series:
		Image image = reader.Execute();

		int origTexWidth = (int)image.GetWidth ();
		int origTexHeight = (int)image.GetHeight ();
		int origTexDepth = (int)image.GetDepth ();
		int texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		int texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		int texDepth = 1;
		Color32[] colors = new Color32[ texWidth*texHeight ];		

		//UInt16 minCol = UInt16.MaxValue;
		//UInt16 maxCol = UInt16.MinValue;

		int slope = header.RescaleSlope;
		int intercept = header.RescaleIntercept;


		if (image.GetDimension () != 2 && image.GetDimension () != 3)
		{
			throw( new System.Exception( "Cannot read DICOM. Only 2D and 3D images are currently supported. Dimensions of image: " + image.GetDimension()));
		}

		IntPtr bufferPtr;
		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight ();
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			bufferPtr = image.GetBufferAsUInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );
			Debug.Log ("Slope, Intercept: " + slope + " " + intercept);
			int index = 0;
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if( x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							colors[ x + y*texWidth + z*texWidth*texHeight ] = F2C( (UInt16)(colorsTmp[index]*slope + intercept));
							index ++;
						}
					}
				}
			}

		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			bufferPtr = image.GetBufferAsInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if( x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							// Shift the signed int into the unsigned int range by adding 32768.
							//Debug.Log("val: " + (colorsTmp[index]*slope - intercept));
							UInt16 pixelValue = (UInt16)((colorsTmp[index] - intercept)/slope);
							// Mask out unused high bits:
							//pixelValue &= unchecked((UInt16)~(1 << 15 | 1 << 14 | 1 << 13 | 1 << 12));

							colors[ x + y*texWidth + z*texWidth*texHeight ] = F2C( pixelValue );

							/*if (pixelValue > maxCol)
								maxCol = pixelValue;
							if (pixelValue < minCol)
								minCol = pixelValue;*/


							index ++;
						}
					}
				}
			}
			//Debug.LogError ("Min, max pixel values: " + minCol + " " + maxCol);
		} else {
			throw(new System.Exception ("Cannot read DICOM. Unsupported pixel format: " + image.GetPixelID()));
		}

		return new DICOMLoadReturnObjectVolume (image, header);
	}

	public DICOMLoadReturnObjectSlice loadSlice( int indexToLoad, int slice )
	{
		if (loadedDirectory == null) {
			Debug.LogWarning ("Set the directory before trying to load a series!");
			return null;
		}
		if (indexToLoad < 0 || indexToLoad >= availableSeries.Count) {
			Debug.LogWarning ("Could not find series.");
			return null;
		}
		DICOMHeader header = availableSeries [indexToLoad];

		// Get the file names for the series:
		//Debug.Log("\tLoading series " + header.SeriesUID);
		VectorString fileNames = header.FileNames;	//ImageSeriesReader.GetGDCMSeriesFileNames( loadedDirectory, header.SeriesUID );

		if (fileNames.Count <= 0)
			throw(new System.Exception ("No files found for series."));

		if ( slice < 0 || slice >= fileNames.Count )
			throw(new System.Exception ("Slice " + slice + " not available in series."));

		// Read the Dicom image:
		Image image = SimpleITK.ReadImage( fileNames[slice] );

		int origTexWidth = (int)image.GetWidth ();
		int origTexHeight = (int)image.GetHeight ();
		int origTexDepth = (int)image.GetDepth ();
		int texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		int texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		int texDepth = 1;
		Color32[] colors = new Color32[ texWidth*texHeight ];		

		//UInt16 minCol = UInt16.MaxValue;
		//UInt16 maxCol = UInt16.MinValue;

		int slope = header.RescaleSlope;
		int intercept = header.RescaleIntercept;


		if (image.GetDimension () != 2 && image.GetDimension () != 3)
		{
			throw( new System.Exception( "Cannot read DICOM. Only 2D and 3D images are currently supported. Dimensions of image: " + image.GetDimension()));
		}

		IntPtr bufferPtr;
		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight ();
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			bufferPtr = image.GetBufferAsUInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );
			Debug.Log ("Slope, Intercept: " + slope + " " + intercept);
			int index = 0;
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if( x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							/*if( colorsTmp[index] > maxCol ){
								maxCol = (int)colorsTmp[index];
							}

							if (colorsTmp [index] < minCol) {
								minCol = (int)colorsTmp [index];
							}*/

							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							colors[ x + y*texWidth + z*texWidth*texHeight ] = F2C( (UInt16)(colorsTmp[index]*slope + intercept));
							index ++;
						}
					}
				}
			}

		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			bufferPtr = image.GetBufferAsInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if( x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							/*if( colorsTmp[index] > maxCol ){
								maxCol = (int)colorsTmp[index];
							}
							if( colorsTmp[index] < minCol ){
								minCol = (int)colorsTmp[index];
							}*/

							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							// Shift the signed int into the unsigned int range by adding 32768.
							//Debug.Log("val: " + (colorsTmp[index]*slope - intercept));
							UInt16 pixelValue = (UInt16)((colorsTmp[index] - intercept)/slope);
							// Mask out unused high bits:
							//pixelValue &= unchecked((UInt16)~(1 << 15 | 1 << 14 | 1 << 13 | 1 << 12));

							colors[ x + y*texWidth + z*texWidth*texHeight ] = F2C( pixelValue );

							/*if (pixelValue > maxCol)
								maxCol = pixelValue;
							if (pixelValue < minCol)
								minCol = pixelValue;*/


							index ++;
						}
					}
				}
			}
			//Debug.LogError ("Min, max pixel values: " + minCol + " " + maxCol);
		} else {
			throw(new System.Exception ("Cannot read DICOM. Unsupported pixel format: " + image.GetPixelID()));
		}

		return new DICOMLoadReturnObjectSlice (texWidth, texHeight, texDepth, colors, header, slice);
	}

	private int indexForSeriesUID( string seriesUID )
	{
		if (loadedDirectory != null) {
			// Look for the index in the series list with the correct UID:
			for (int i = 0; i < availableSeries.Count; i++) {
				Debug.LogWarning (availableSeries [i].SeriesUID + " " + seriesUID);
				if (availableSeries [i].SeriesUID == seriesUID) {
					return i;
				}
			}
		}
		// Return invalid index:
		return -1;
	}

	public List<DICOMHeader> getAvailableSeries()
	{
		// Deep copy the list and return that:
		List<DICOMHeader> newlist = new List<DICOMHeader> ();
		foreach (DICOMHeader h in availableSeries) {
			newlist.Add ((DICOMHeader)h.Clone ());
		}
		return newlist;
	}

	float frac( float val )
	{
		return val - Mathf.Floor(val);
	}

	Color32 F2C(UInt16 value)
	{
		/*byte[] bytes = BitConverter.GetBytes( 1000 );

		float R = (float)bytes[0];
		float G = (float)bytes[1];
		//return new Color( R/255.0f, G/255.0f, 0.0f, 1.0f );
		return new Color( R, G, 0.0f, 1.0f );*/
		//value = 1000;

		Color32 c = new Color32 ();

		c.r = (byte)(value % 256); value /= 256;
		c.g = (byte)(value % 256); value /= 256;
		c.b = (byte)(value % 256); value /= 256;
		c.a = (byte)(value % 256);
		//Debug.LogWarning ("c: " + c.r + " " + c.g + " " + c.b + "  " + c.a);

		return c;
	}
	Color F2C(Int16 value)
	{
		UInt16 valueUInt = (UInt16)((int)value + 32768);
		return F2C (valueUInt);
	}

}

