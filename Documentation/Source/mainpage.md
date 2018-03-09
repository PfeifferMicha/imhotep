IMHOTEP Framework								{#mainpage}
===============================================

Welcome to the [IMHOTEP](imhotep-medical.org) Documentation.

The IMHOTEP Framework can be used to view medical data in a Virtual Reality (VR) setting.
Supported VR-Devices are:
	- Oculus Rift (Consumer Version)
	- HTC Vive

Supported Input options:
	- Mouse/Keyboard
	- HTC Vive controllers

To compile the Framework, you need:
	- Unity3D (tested with Version 5.x)
	- Steam and %SteamVR
	- Blender (tested with Version 2.70)


VR Setup:
-----------------------------------------------
To run the project using the *Oculur Rift* or normal computer screen (for developping purposes):
	1. Search for the "Camera" GameObject in the Hierarchy
	2. The "Camera" has a child called "Camera(Rift)". Set it to active.
	3. The "Camera" also has a child called "[CameraRig(Vive)]". Set it to inactive.
	4. Connect the Oculus Rift.
	5. Run the project from within Unity.

To run the project using the *HTC Vive*:
	1. Search for the "Camera" GameObject in the Hierarchy
	2. The "Camera" has a child called "[CameraRig(Vive)]". Set it to active.
	3. The "Camera" also has a child called "Camera(Rift)". Set it to inactive.
	4. Connect the HTC Vive and start %SteamVR.
	5. Run the project from within Unity.

Getting Started:
----------------------------------------------

[Input](input.md): Describes how the IMHOTEP Input system differs from Unitys default system.

[User Interface](userInterface.md): Describes how %UI elements are set up in IMHOTEP.

[Building your own tools](customTools.md): Hints on how to build your own tools and %UI widgets.

[Patient Event System](patientEventSystem.md): Describes how to make your tools react to %Patient load/close events.

