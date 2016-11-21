using System;
using itk.simple;
using UnityEngine;

public class DICOMVolume
{
	private DICOMHeader mHeader;
	public Image itkImage { get; private set; }

	public DICOMVolume ( Image image )
	{
		itkImage = image;
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
	public UInt32 getMaximum() {
		return (UInt32)mHeader.MaxPixelValue;
	}
	public UInt32 getMinimum() {
		return (UInt32)mHeader.MinPixelValue;
	}
}

