
To Do
===========================================================================

- Text input method in VR. Currently, text input is done via keyboard. We are working on this.
- Loading indicator while loading volume data
- Re-add automatic label positioning.
- Queue for loading multiple DICOMs consequently? When a DICOM volume is being loaded, the loading of DICOM 2D slices is currently blocked.
- Screenshot tool
- Volumetrics:
	- Volumetric rendering of Saggital and Coronal volumes (i.e. volumes where the saved slices are oriented in non-transverse direction). These are currently disabled because they would be rendered using a wrong orientation.
	- Option to precompute the volume's gradient (and uploading it as a texture) instead of computing it on the fly. Will require much more graphic card memory, so it should be optional.
	- Possibly a better UI for loading the volume?
