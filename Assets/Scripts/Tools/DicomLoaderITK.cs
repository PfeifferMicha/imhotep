using System;
using System.Runtime.InteropServices;
using UnityEngine;
using itk.simple;

public class DicomLoaderITK
{
	public DicomLoaderITK ()
	{
	}

	public VectorString loadDirectory( string directory )
	{
		Debug.Log ("Looking for DICOMS in: " + directory);

		// Parse the directory and return the seriesUIDs of all the DICOM series:
		VectorString series = ImageSeriesReader.GetGDCMSeriesIDs (directory);

		string str = "\tFound " + series.Count + " series.";

		for (int i = 0; i < series.Count; i++)
			str += "\n\t\t" + series [i];

		Debug.Log (str);

		if (series.Count <= 0)
			return new VectorString();

		return series;
	}

	public DICOM load( string directory, string seriesUID )
	{
		VectorString series = loadDirectory (directory);

		// Look for the index in mSeries which loads 
		int seriesToLoad = series.Count;	// Start with invalid index
		for (int i = 0; i < series.Count; i++) {
			if (series [i] == seriesUID) {
				seriesToLoad = i;
			}
		}

		if (seriesToLoad >= series.Count) {
			throw(new System.Exception("Cannot find series " + seriesUID + " in directory " + directory + "."));
		}

		// Get the file names for the series:
		Debug.Log("\tLoading series " + series[seriesToLoad]);
		VectorString fileNames = ImageSeriesReader.GetGDCMSeriesFileNames( directory, series[seriesToLoad] );

		string str = "\tFound " + fileNames.Count + " files.";
		for (int i = 0; i < fileNames.Count; i++)
			str += "\n\t\t" + fileNames [i];
		Debug.Log (str);
		
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
		Debug.Log ("\tImage: " + image.ToString());
		Debug.Log ("\tImage Pixel Type: " + image.GetPixelID());
		Debug.Log ("\tImage size: " + origTexWidth + "x" + origTexHeight + "x" + origTexDepth );
		Debug.Log ("\tTexture size: " + texWidth + "x" + texHeight + "x" + texDepth );
		Debug.Log ("\tImage number of pixels: " + numberOfPixels);

		Image metaDataImage = SimpleITK.ReadImage( fileNames[0] );
		VectorString keys = metaDataImage.GetMetaDataKeys();
		str = "";
		for (int i = 0; i < keys.Count; i++)
			str += "\n\t" + keys [i];
		Debug.Log ("\tMetadata:\n" + str);

		string studyInstanceUID = metaDataImage.GetMetaData ("0020|000d");
		string seriesInstanceUID = metaDataImage.GetMetaData ("0020|000e");

		DICOMHeader header = new DICOMHeader (studyInstanceUID, seriesInstanceUID);

		// Some of the following tags may not be in the DICOM Header, so catch and ignore "not found" exceptions:
		try {
			header.setPatientName ( metaDataImage.GetMetaData( "0010|0010" ) );
		} catch( ApplicationException exp ) { Debug.LogWarning ("Could not find DICOM tag: (0010|0010)");}
		try {
			header.setSeriesDate( metaDataImage.GetMetaData( "0008|0021" ) );
		} catch( ApplicationException exp ) { Debug.LogWarning ("Could not find DICOM tag: (0008|0021)");}
		try {
			header.mModality = metaDataImage.GetMetaData( "0008|0060" );
		} catch( ApplicationException exp ) { Debug.LogWarning ("Could not find DICOM tag: (0008|0060)");}
		try {
			header.mInstitutionName = metaDataImage.GetMetaData( "0008|0080" );
		} catch( ApplicationException exp ) { Debug.LogWarning ("Could not find DICOM tag: (0008|0080)");}

		Debug.Log (header);
		
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

			minCol += 32768;	// Signed Int16 to unsigned Int16
			maxCol += 32768;	// Signed Int16 to unsigned Int16
		} else {
			throw(new System.Exception ("Cannot read DICOM. Unsupported pixel format: " + image.GetPixelID()));
		}

		Debug.Log ("Min, max: " + minCol + " " + maxCol);

		Texture3D tex = new Texture3D( texWidth, texHeight, texDepth, TextureFormat.RGBA32, false);
		tex.SetPixels( colors	);
		tex.Apply();

		DICOM dicom = new DICOM ();
		dicom.setTexture (tex);
		dicom.setHeader (header);
		dicom.setMaximum ((UInt32)maxCol);
		dicom.setMinimum ((UInt32)minCol);
		
		return dicom;
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
		byte[] bytes = BitConverter.GetBytes( valueUInt  );
		
		float R = (float)bytes[0];
		float G = (float)bytes[1];
		return new Color( R/255.0f, G/255.0f, 0.0f, 0.0f );
	}

}

