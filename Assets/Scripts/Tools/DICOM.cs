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
	public Texture3D getTexture()
	{
		return mTexture;
	}
	public void setHeader( DICOMHeader hdr )
	{
		mHeader = hdr;
	}
	public void setTexture( Texture3D tex )
	{
		mTexture = tex;
	}
	public UInt32 getMaximum() {
		return mMaximum;
	}
	public UInt32 getMinimum() {
		return mMinimum;
	}
	public void setMaximum( UInt32 max ) {
		mMaximum = max;
	}
	public void setMinimum( UInt32 min) {
		mMinimum = min;
	}

	private DICOMHeader mHeader;
	private Texture3D mTexture;
	private UInt32 mMaximum;
	private UInt32 mMinimum;
}

