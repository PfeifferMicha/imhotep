using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/*! Event system for all patient-related events.
 * Listeners can register for specific events and will be notified from now on.
 * This class is a (static) signleton. */
public class PatientEventSystem
{
	private Dictionary< Event, UnityEvent> mEventDictionary;
	private static PatientEventSystem mInstance = null;

	/*! All possible events: */
	public enum Event {
		PATIENT_NewPatientDirectoryFound,
		MESH_Loaded,
		//MESH_Closed
		DICOM_NewList,
		DICOM_NewLoaded,
		DICOM_AllCleared
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
	public static void startListening(Event eventType, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else {
			thisEvent = new UnityEvent();
			thisEvent.AddListener(listener);
			instance.mEventDictionary.Add(eventType, thisEvent);
		}
		Debug.Log("Added event listener for event: " + eventType);
	}
	public static void stopListening(Event eventType, UnityAction listener)
	{
		if (mInstance == null)
			return;

		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
		Debug.Log("Removed event listener for event: " + eventType);
	}
	public static void triggerEvent(Event eventType)
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue(eventType, out thisEvent))
		{
			Debug.Log("Triggering Event: " + eventType);
			thisEvent.Invoke();
		}
	}

	private PatientEventSystem()
	{
		if( mEventDictionary == null )
		{
			mEventDictionary = new Dictionary< Event, UnityEvent >();
		}
	}

}

