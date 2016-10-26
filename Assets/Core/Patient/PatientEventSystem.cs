using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ObjectEvent : UnityEvent<object> {} //empty class; just needs to exist

/*! Event system for all patient-related events.
 * Listeners can register for specific events and will be notified from now on.
 * This class is a (static) signleton. */
public class PatientEventSystem
{
	private Dictionary< Event, ObjectEvent> mEventDictionary;
	private static PatientEventSystem mInstance = null;

	/*! All possible events: */
	public enum Event {
		/*! Called when we start loading a new Patient. */
		PATIENT_StartLoading,
		/*! Called when we have loaded a new Patient.
		 * \note Only the meta-data of the Patient will be available at this point (views, 
		 *	annotations etc.). The Mesh and DICOM might still be loading!
		 * Patient is passed to the callbacks.*/
		PATIENT_Loaded,
		/*! Called when we have finished loading a new Patient, including Mesh and DICOM.
		 * \note This will be called when the DICOM directory has been parsed and the Mesh
		 *	has been loaded.
		 * Patient is passed to the callbacks.*/
		PATIENT_FinishedLoading,
		/*! Called when we've found a new directory while looking for patients.*/
		PATIENT_NewPatientDirectoryFound,

		/*! Called as soon as someone closes the Patient using the close button.
		 * \note At this point, you should stop referencing any Patients and clear all
		 *	Patient-related displays/data. */
		PATIENT_Closed,
		/*! Called when a new (single!) mesh has been loaded.
		 * Other meshes might still be loading.
		 * The new GameObject holding the new mesh is passed to the callbacks. */
		MESH_LoadedSingle,		// Called whenever a new mesh has been loaded
		/*! Called when all meshes has been loaded.*/
		MESH_LoadedAll,			// Called after all of the patient's meshes have been loaded
		/*! Called when mesh opacity is changed. Internal use only.
		 * \note Not called if a slider changed the opacity.*/
		MESH_Opacity_Changed,

		/*! Called when a new list of DICOMs has been created from the DICOM folder.
		 * If needed, the list can be retrieved using
		 * PatientDirectoryLoader.getAvailableSeries().*/
		DICOM_NewList,
		/*! Called when the user wants to start loading a new DICOM slice or series.*/
		DICOM_StartLoading,
		/*! Called when we've loaded a new slice.*/
		DICOM_NewLoaded,
		/*! Called when we've loaded a new volume.*/
		DICOM_NewLoadedVolume,
		/*! Internal use only. */
		DICOM_AllCleared,

		/*! Internal use only. */
		LOADING_AddLoadingJob,
		/*! Internal use only. */
		LOADING_RemoveLoadingJob
    }

	/*! Returns the singleton instance: */
	public static PatientEventSystem instance
	{
		get {
			if (mInstance == null) {
				mInstance = new PatientEventSystem ();
			}
			return mInstance;
		}
	}

	/*! Start calling the function listener whenever eventType happens. */
	public static void startListening(Event eventType, UnityAction<object> listener)
	{
		ObjectEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else {
			thisEvent = new ObjectEvent();
			thisEvent.AddListener(listener);
			instance.mEventDictionary.Add(eventType, thisEvent);
		}
		//Debug.Log("Added event listener for event: " + eventType);
	}
	/*! Stop calling the function listener.
	 * This should be called with the exact same parameters as were passed to startListening before.*/
	public static void stopListening(Event eventType, UnityAction<object> listener)
	{
		if (mInstance == null)
			return;

		ObjectEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
		//Debug.Log("Removed event listener for event: " + eventType);
	}
	public static void triggerEvent(Event eventType, object obj = null )
	{
		ObjectEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			//Debug.Log("Triggering Event: " + eventType);
			thisEvent.Invoke( obj );
		}
	}

	private PatientEventSystem()
	{
		if( mEventDictionary == null )
		{
			mEventDictionary = new Dictionary< Event, ObjectEvent >();
		}
	}

}

