using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;

public class PatientCache : MonoBehaviour {

    private static PatientCache mInstance;
    private Dictionary<Event, UnityEvent> mEventDictionary;

    private Patient currentPatient;

    public enum Event
    {
        OpenPatient,
        ClosePatient
    }

    // Use this for initialization
    public PatientCache () {
        /*currentPatient = null;
        mPatientLoader = new PatientDirectoryLoader();
        mPatientLoader.setPath("../Patients/");*/
    }
	
	// Update is called once per frame
	/*void Update () {
	
	}*/


    public void openPatient(int index)
    {
        //currentPatient = mPatientLoader.loadPatient(index);

		Debug.Log("Path: " + currentPatient.path);

        //DicomCache dicomCache = DicomCache.instance;
		//dicomCache.loadDirectory(currentPatient.path + "/DICOM");


        Loader mModelLoader = GameObject.Find("GlobalScript").GetComponent<Loader>();
        // Load model in the directory:
		string modelPath = currentPatient.path + "/all_r.blend";
        if (File.Exists(modelPath)) //TODO use path from json file
        {
            mModelLoader.LoadFile(modelPath);
        }

    }

    public void closePatient()
    {
        currentPatient = null;
        Loader mModelLoader = GameObject.Find("GlobalScript").GetComponent<Loader>();
        mModelLoader.RemoveMesh();
    }

    public static Patient getCurrentPatient()
    {
        return instance.currentPatient;
    }


    //TODO Same code in DICOM Cache, merge it or use inheritance

    public static PatientCache instance
    {
        get
        {
            if (!mInstance)
            {
                mInstance = FindObjectOfType(typeof(PatientCache)) as PatientCache;
                if (!mInstance)
                {
                    Debug.LogError("There needs to be at least one PatientCache active in the project!");
                }
                mInstance.init();
            }
            return mInstance;
        }
    }

    void init()
    {
        if (mEventDictionary == null)
        {
            mEventDictionary = new Dictionary<Event, UnityEvent>();
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
            thisEvent.Invoke();
            Debug.Log("Triggered Event:" + eventType);
        }
    }


}
