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

	/*! The ITK image.
	 * Can be used to access the raw pixel data as it is in the file.
	 * Make sure to read the SimpleITK (or the normal ITK) documentation for details on
	 * pixel value types, number of bits etc.
	 * This can also be used to access header information through image.GetMetaData().*/
	public Image image { protected set; get; }

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

	/*! Helper function, converts UInt16 to color */
	public static Color32 F2C(UInt32 value)
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
	public static Color F2C(Int16 value)
	{
		UInt32 valueUInt = (UInt32)((int)value + 32768);
		return F2C (valueUInt);
	}
}
