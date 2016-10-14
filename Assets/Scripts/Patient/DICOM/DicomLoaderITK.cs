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
		DICOMHeader header = new DICOMHeader (metaDataImage, fileNames);
		return header;
	}

	public DICOMLoadReturnObject load( int indexToLoad )
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
		Debug.Log("\tLoading series " + header.SeriesUID);
		VectorString fileNames = header.FileNames;	//ImageSeriesReader.GetGDCMSeriesFileNames( loadedDirectory, header.SeriesUID );

		if (fileNames.Count <= 0)
			throw(new System.Exception ("No files found for series."));

		// Read the Dicom image series
		ImageSeriesReader reader = new ImageSeriesReader ();
		reader.SetFileNames (fileNames);

		Image image = reader.Execute();

		UInt32 numberOfPixels = image.GetWidth () * image.GetHeight () * image.GetDepth ();

		int origTexWidth = (int)image.GetWidth ();
		int origTexHeight = (int)image.GetHeight ();
		int origTexDepth = (int)image.GetDepth ();
		int texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		int texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		int texDepth = Mathf.NextPowerOfTwo ((int)image.GetDepth ());
		/*Debug.Log ("\tImage: " + image.ToString());
		Debug.Log ("\tImage Pixel Type: " + image.GetPixelID());
		Debug.Log ("\tImage size: " + origTexWidth + "x" + origTexHeight + "x" + origTexDepth );
		Debug.Log ("\tTexture size: " + texWidth + "x" + texHeight + "x" + texDepth );
		Debug.Log ("\tImage number of pixels: " + numberOfPixels);*/

		//Image metaDataImage = SimpleITK.ReadImage( fileNames[0] );
		//DICOMHeader header = new DICOMHeader (metaDataImage, fileNames);
		//header.NumberOfImages = image.GetDepth ();

		// Some of the following tags may not be in the DICOM Header, so catch and ignore "not found" exceptions:

		
		Color[] colors = new Color[ texWidth*texHeight*texDepth ];		
		int maxCol = 0;
		int minCol = 65535;

		if (image.GetDimension () != 2 && image.GetDimension () != 3)
		{
			throw( new System.Exception( "Cannot read DICOM. Only 2D and 3D images are currently supported. Dimensions of image: " + image.GetDimension()));
		}

		IntPtr bufferPtr;
		if (image.GetPixelID () == PixelIDValueEnum.sitkUInt16) {
			bufferPtr = image.GetBufferAsUInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );

			int index = 0;
			for (UInt32 z = 0; z < texDepth; z++) {
				for (UInt32 y = 0; y < texHeight; y++) {
					for (UInt32 x = 0; x < texWidth; x++) {
						if( x < origTexWidth && y < origTexHeight && z < origTexDepth )
						{
							if( colorsTmp[index] > maxCol ){
								maxCol = (int)colorsTmp[index];
							}

							if (colorsTmp [index] < minCol) {
								minCol = (int)colorsTmp [index];
							}

							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							colors[ (texWidth-1-x) + y*texWidth + z*texWidth*texHeight ] = F2C( (UInt16)colorsTmp[index] );
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
							if( colorsTmp[index] > maxCol ){
								maxCol = (int)colorsTmp[index];
							}
							if( colorsTmp[index] < minCol ){
								minCol = (int)colorsTmp[index];
							}

							//colors[ z + (x + yTex*texWidth)*texDepth ] = F2C( (UInt16)colorsTmp[index] );
							// Shift the signed int into the unsigned int range by adding 32768.
							colors[ (texWidth-1-x) + y*texWidth + z*texWidth*texHeight ] = F2C( (UInt16)(colorsTmp[index]+32768) );

							index ++;
						}
					}
				}
			}
			Debug.LogError (minCol);
			Debug.LogError (maxCol);

			minCol += 32768;	// Signed Int16 to unsigned Int16
			maxCol += 32768;	// Signed Int16 to unsigned Int16
		} else {
			throw(new System.Exception ("Cannot read DICOM. Unsupported pixel format: " + image.GetPixelID()));
		}

		return new DICOMLoadReturnObject (texWidth, texHeight, texDepth, colors, header);
	}

	public DICOMLoadReturnObject loadSlice( int indexToLoad, int slice )
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
		Debug.Log("\tLoading series " + header.SeriesUID);
		VectorString fileNames = header.FileNames;	//ImageSeriesReader.GetGDCMSeriesFileNames( loadedDirectory, header.SeriesUID );

		if (fileNames.Count <= 0)
			throw(new System.Exception ("No files found for series."));

		if ( slice < 0 || slice >= fileNames.Count )
			throw(new System.Exception ("Slice " + slice + " not available in series."));

		// Read the Dicom image:
		Image image = SimpleITK.ReadImage( fileNames[slice] );

		Debug.LogWarning ("PIXEL REPRESENTATION: " + image.GetMetaData ("0028|0106"));

		int origTexWidth = (int)image.GetWidth ();
		int origTexHeight = (int)image.GetHeight ();
		int origTexDepth = (int)image.GetDepth ();
		int texWidth = Mathf.NextPowerOfTwo ((int)image.GetWidth ());
		int texHeight = Mathf.NextPowerOfTwo ((int)image.GetHeight ());
		int texDepth = 1;
		Color[] colors = new Color[ texWidth*texHeight ];		
		//int maxCol = -65535;
		//int minCol = 65535;

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
							colors[ (texWidth-1-x) + y*texWidth + z*texWidth*texHeight ] = F2C( (UInt16)(colorsTmp[index]*slope + intercept));
							index ++;
						}
					}
				}
			}

		} else if ( image.GetPixelID() == PixelIDValueEnum.sitkInt16 ) {
			bufferPtr = image.GetBufferAsInt16 ();

			Int16[] colorsTmp = new Int16[ numberOfPixels ];
			Marshal.Copy( bufferPtr, colorsTmp, 0, (int)numberOfPixels );
			UInt16 minCol = UInt16.MaxValue;
			UInt16 maxCol = UInt16.MinValue;

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
							UInt16 pixelValue = (UInt16)(colorsTmp[index]*slope - intercept);
							// Mask out unused high bits:
							pixelValue &= unchecked((UInt16)~(1 << 15 | 1 << 14 | 1 << 13 | 1 << 12));

							colors[ (texWidth-1-x) + y*texWidth + z*texWidth*texHeight ] = F2C( pixelValue );

							if (pixelValue > maxCol)
								maxCol = pixelValue;
							if (pixelValue < minCol)
								minCol = pixelValue;


							index ++;
						}
					}
				}
			}
			Debug.LogError ("Min, max pixel values: " + minCol + " " + maxCol);
		} else {
			throw(new System.Exception ("Cannot read DICOM. Unsupported pixel format: " + image.GetPixelID()));
		}

		return new DICOMLoadReturnObject (texWidth, texHeight, texDepth, colors, header);
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

	Color F2C(UInt16 value)
	{
		byte[] bytes = BitConverter.GetBytes( value );

		float R = (float)bytes[0];
		float G = (float)bytes[1];
		return new Color( R/255.0f, G/255.0f, 0.0f, 0.0f );
	}
	Color F2C(Int16 value)
	{
		UInt16 valueUInt = (UInt16)((int)value + 32768);
		return F2C (valueUInt);
	}

}

