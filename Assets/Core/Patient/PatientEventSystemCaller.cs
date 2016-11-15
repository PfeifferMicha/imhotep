using UnityEngine;
using System.Collections;
/*
 * Calls the PatientEventSystem in main thread so the events can be executed in the main thread 
 */
public class PatientEventSystemCaller : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        PatientEventSystem.executeSavedEvents();
	}
}
