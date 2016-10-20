Patient Event System
=================================

The [PatientEventSystem] can be used to react to events which happen when loading or closing a patient. Generally, every tool should react to these events, for example to load tool-specific data after a patient is loaded or to hide buttons when there's no patient loaded.

To register a method which should be called when a specific event is fired, call the function PatientEventSystem.startListening. For example:

        PatientEventSystem.startListening(PatientEventSystem.Event.MESH_LoadedAll, createContent);
 
This line will make sure that every time the MESH_LoadedAll event happens, the createContent function will be called.
Note that create content should be a method of the class you're programming and it should have the following signature:

	public void createContent( object obj = null )
	{
		//...
	}

Some of the events will fill the variable, some will not. For example, the PATIENT_Loaded event will pass the newly loaded Patient object to the callback.
In the callback you can use this patient by casting the object:

	public void onPatientLoaded( object obj = null )
	{
		Patient p = obj as Patient;
		if( p != null )
		{
			// ...
		}
	}

A list of Events can be found in PatientEventSystem.Event.

You can un-register callbacks by calling the function PatientEventSystem.stopListening with the same parameters as the startListening call.

Note: We recommend calling the startListening function in your MonoBehavior's OnEnable function and the stopListening in OnDisable only.
This is because your MonoBehaviour might be enabled and disabled multiple times (for example if the user switches tools back and forth). This way,
you will make sure that you will get notified only while your tool/widget is actually active.

For examples, check out the source code for OpacityControl.cs and DicomDisplay.cs.
