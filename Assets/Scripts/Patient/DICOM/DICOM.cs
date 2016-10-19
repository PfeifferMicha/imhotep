using System;
using itk.simple;
using UnityEngine;

public class DICOM
{
	public DICOM ()
	{
	}

	public DICOMHeader getHeader()
	{
		return mHeader;
	}
	public Texture2D getTexture2D()
	{
		return mTexture2D;
	}
	public Texture3D getTexture3D()
	{
		return mTexture3D;
	}
	public void setHeader( DICOMHeader hdr )
	{
		mHeader = hdr;
	}
	public void setTexture2D( Texture2D tex )
	{
		mTexture2D = tex;
	}
	public void setTexture3D( Texture3D tex )
	{
		mTexture3D = tex;
	}
	public UInt32 getMaximum() {
		return (UInt32)mHeader.MaxPixelValue;
	}
	public UInt32 getMinimum() {
		return (UInt32)mHeader.MinPixelValue;;
	}
	public bool is2DImage()
	{
		return (mTexture2D != null);
	}
	public int slice;

	private DICOMHeader mHeader;
	private Texture3D mTexture3D;
	private Texture2D mTexture2D;
}

