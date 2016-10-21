using System;
using itk.simple;
using UnityEngine;

public class DICOMVolume
{
	private DICOMHeader mHeader;
	private Image itkImage;

	public DICOMVolume ()
	{
	}
	public DICOMHeader getHeader()
	{
		return mHeader;
	}
	public void setHeader( DICOMHeader hdr )
	{
		mHeader = hdr;
	}
	public Image getImage()
	{
		return itkImage;
	}
	public void setImage( Image image )
	{
		itkImage = image;
	}
	public UInt32 getMaximum() {
		return (UInt32)mHeader.MaxPixelValue;
	}
	public UInt32 getMinimum() {
		return (UInt32)mHeader.MinPixelValue;
	}
}

