using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gdcm;

/*  Possible GDCM Pixel Formats and their corresponding values in Unity:
    DICOM standard defines a "SamplesPerPixel" tag (Value Type: US) which holds the
    number of channels. The channels are either 1 or 3, other values are allowed but
    usage is undefined.
    This loader only supports 1 or 3 channels.
    By Looking at the GDCM Pixel Format and the number of channels, the Unity Texture Format
    can be determined.

    TODO: Check how/if unity can differentiate between unsigned and signed values.
        (My current guess is unity always uses unsigned).

	GDCM Format:			Unity (if 1 Channel)			Unity (if 3 Channels)
    -------------------------------------------------------------------------------
		UINT8					Alpha8							RGB24
		INT8					Alpha8	(?)						RGB24	(?)
		UINT12						- 
		INT12						-
		UINT16					R16     (?)							-
		INT16					R16									-
		UINT32						-								-
		INT32						-								-
		FLOAT16					RHalf								-
		FLOAT32					RFloat								-
		FLOAT64					-								-
		SINGLEBIT					-								-
		UNKNOWN
*/

public class DicomLoader {
    public DicomLoader( string directory )
    {
        Debug.Log("Loading Dicoms from: " + directory);
        Tag tagStudyInstanceUID = new Tag(0x20, 0x000d);
        Tag tagSeriesInstanceUID = new Tag(0x20, 0x000e);

        Directory d = new Directory();
        uint nfiles = d.Load(directory);
        if (nfiles == 0) return;
        Debug.Log( "Files:\n" + d.toString() );

        // Use a StrictScanner, need to use a reference to pass the C++ pointer to
        // MyWatcher implementation
        SmartPtrStrictScan sscan = StrictScanner.New();
        StrictScanner s = sscan.__ref__();

        s.AddTag(tagStudyInstanceUID);
        s.AddTag(tagSeriesInstanceUID);
        bool b = s.Scan(d.GetFilenames());
        if (!b) return;

        ValuesType vals = s.GetValues();

        Debug.Log(vals.ToString());
        
        for (int i = 0; i < (int)nfiles; ++i)       // Go through all file names
        {
            if ( s.IsKey(d.GetFilenames()[i]) )     // If the tags exist, this is a valid DICOM, so we can read it!
            {
                if (load2DImage(d.GetFilenames()[i]))
                    break;
                else
                    break;
            }
        }
        
    }

    bool load2DImage( string filename )
    {
        Image img = null;

        var reader = new gdcm.ImageReader();
        reader.SetFileName( filename );
        if (reader.Read())      // Do the actual loading of the DICOM file
        {
            Debug.Log("Reading: " + filename );

            var file = reader.GetFile();
            DataSet dataSet = file.GetDataSet();

            Tag tSamplesPerPixel = new Tag(0x28, 0x02);   // The samples per pixel gives the number of channels in the image.

            if (!dataSet.FindDataElement(tSamplesPerPixel))
                return false;
            
            DataElement elem = dataSet.GetDataElement(tSamplesPerPixel);
            if( elem.GetVR().toString() != "US" )   //VR.VRType.US )
            {
                Debug.LogError("Tag <0x28, 0x02> (Samples Per Pixel) is not of type 'US'.");
                return false;
            }

            byte[] buffer = new byte[elem.GetByteValue().GetLength()];
            elem.GetByteValue().GetBuffer(buffer, elem.GetByteValue().GetLength());

            //if (BitConverter.IsLittleEndian)
              //  Array.Reverse(buffer);

            ushort samplesPerPixel = BitConverter.ToUInt16(buffer, 0);
            Debug.Log(samplesPerPixel);
            Debug.Log(elem.GetByteValue().GetLength());


            img = reader.GetImage();
            PixelFormat sourceFormat = img.GetPixelFormat();

            int width = (int)img.GetDimension(0);
            int height = (int)img.GetDimension(1);

            Debug.Log("Format: " + sourceFormat.GetScalarTypeAsString());
            Debug.Log("Buffer Size: " + (int)img.GetBufferLength());
            Debug.Log("widht: " + width);
            Debug.Log("height: " + height);
            
            TextureFormat destFormat = new TextureFormat();
            if (!ConvertPixelFormat(sourceFormat, samplesPerPixel, ref destFormat))
            {
                Debug.Log("Could not convert DICOM Pixel Format to Unity Pixel Format. Cannot open file.");
                return false;
            }

            byte[] pixeBuffer = new byte[img.GetBufferLength()];
            img.GetBuffer(pixeBuffer);

            // Copy the raw buffer into a Unity Texture:
            Texture2D tex = new Texture2D(width, height, TextureFormat.R16, false);
            tex.LoadRawTextureData(pixeBuffer);
            tex.Apply();
        
            GameObject dicomViewer = GameObject.Find("DICOM_Plane");
            if (dicomViewer)
            {
                Renderer dicomRenderer = dicomViewer.GetComponent<Renderer>();
                dicomRenderer.material.mainTexture = tex;
            }
            else { Debug.Log("Can't find obj"); }

            return true;      // Only load one texture!!
        }
        return false;
    }


    bool ConvertPixelFormat(PixelFormat sourceFormat, ushort samplesPerPixel, ref TextureFormat destFormat)
    {
        if (samplesPerPixel == 1)
        {
            Debug.Log("Type: " + sourceFormat.GetScalarType());
            switch (sourceFormat.GetScalarType())
			{
				case PixelFormat.ScalarType.UINT8:
					destFormat = TextureFormat.Alpha8;
					return true;
				case PixelFormat.ScalarType.INT8:
					destFormat = TextureFormat.Alpha8;		// ?
					return true;
				case PixelFormat.ScalarType.UINT16:
					destFormat = TextureFormat.R16;
					return true;
				case PixelFormat.ScalarType.INT16:			// ?
					destFormat = TextureFormat.R16;
					return true;
				case PixelFormat.ScalarType.FLOAT16:
                    destFormat = TextureFormat.RHalf;
                    return true;
				case PixelFormat.ScalarType.FLOAT32:
					destFormat = TextureFormat.RFloat;
					return true;
				default:
                    return false;
            }
        }
        else if (samplesPerPixel == 3 )
        {
            switch (sourceFormat.GetScalarType())
            {
                case PixelFormat.ScalarType.UINT8:
                    destFormat = TextureFormat.RGB24;
                    return true;
				case PixelFormat.ScalarType.INT8:			// ?
					destFormat = TextureFormat.RGB24;
					return true;
				default:
                    return false;
            }
        }
        return false;
    }
}
