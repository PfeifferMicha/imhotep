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
	/*! The slice this DICOM image represents. Negative if this is a volume. */
	public int slice { private set; get; }
	/*! Raw color values (in case of a 2D DICOM). */
	public Color32[] colors;
	/*! Number of dimensions in this DICOM (2 for slice, 3 for volume).
	 * \note If dimensions == 2, this does not mean that there aren't any other
	 * 		slices, it only means that this image represents a single slice.
	 * 		To get access to the number of slices in the series, see the seriesInfo.*/
	public int dimensions { private set; get; }
	public int texWidth { private set; get; }
	public int texHeight { private set; get; }
	public int texDepth { private set; get; }
	/*! Texture of the slice. The texture will be generated when this is first called.
	 * \note This may only be called if dimension == 2, otherwise it will throw an error.*/
	public Texture2D texture2D { private set; get; }
	/*! The ITK image.
	 * Can be used to access the raw pixel data as it is in the file.
	 * Make sure to read the SimpleITK (or the normal ITK) documentation for details on
	 * pixel value types, number of bits etc.
	 * This can also be used to access header information through image.GetMetaData().*/
	public Image image { private set; get; }

	/*! Constructor, loads the DICOM image data from file.
	 * The constructor starts the loading of pixel data from the files (filenames are
	 * taken from the seriesInfo). If slice is zero or positive, only the single file
	 * will be read. If slice is negative, the entire volume (i.e. all files - and thus
	 * all slices) will be read.
	 * \note Since the constructor does so much work, the Object should be created in
	 * 		a background thread and then passed to the main thread. */
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

	/*! Loads a single file and creates an array of colors from the pixels.
	 * The array of colors can later be used to generate a texture, see getTexture2D() */
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


		Int64 min = int.MaxValue;
		Int64 max = int.MinValue;

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
						colors [ x + y * texWidth] = F2C(pixelValue);

						if (pixelValue > max)
							max = (Int64)pixelValue;
						if (pixelValue < min)
							min = (Int64)pixelValue;

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
			seriesInfo.setMinMaxPixelValues ((int)min, (int)max);
		}
		// Make the loaded image accessable from elsewhere:
		this.image = image;
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
	private Color32 F2C(UInt32 value)
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
		UInt32 valueUInt = (UInt32)((int)value + 32768);
		return F2C (valueUInt);
	}
}
