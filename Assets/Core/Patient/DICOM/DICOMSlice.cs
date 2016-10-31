using System;
using itk.simple;
using UnityEngine;

public class DICOMSlice
{
	private DICOMHeader mHeader;
	private Texture2D mTexture2D;
	public int slice;

	public DICOMSlice ()
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
	public void setHeader( DICOMHeader hdr )
	{
		mHeader = hdr;
	}
	public void setTexture2D( Texture2D tex )
	{
		mTexture2D = tex;
	}
	public UInt32 getMaximum() {
		return (UInt32)mHeader.MaxPixelValue;
	}
	public UInt32 getMinimum() {
		return (UInt32)mHeader.MinPixelValue;;
	}
}

