using UnityEngine;
using System.Collections;
using System;
using itk.simple;


public class DICOMLoadReturnObjectVolume {

	public Image itkImage { get; private set; }
	public DICOMHeader header { get; private set; }

	public DICOMLoadReturnObjectVolume ( Image image, DICOMHeader header )
    {
		this.itkImage = image;
		this.header = header;
    }
}
