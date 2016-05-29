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
		PATIENT_StartLoading,
		PATIENT_Loaded,
		PATIENT_FinishedLoading,

		PATIENT_NewPatientDirectoryFound,

		MESH_LoadedSingle,		// Called whenever a new mesh has been loaded
		MESH_LoadedAll,			// Called after all of the patient's meshes have been loaded

		DICOM_NewList,
		DICOM_NewLoaded,
		DICOM_AllCleared,

		LOADING_AddLoadingJob,
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
		Debug.Log("Added event listener for event: " + eventType);
	}
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
		Debug.Log("Removed event listener for event: " + eventType);
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

