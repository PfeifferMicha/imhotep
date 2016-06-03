using UnityEngine;
using System.Collections;

public class RoomLightControl : MonoBehaviour {

	public GameObject[] roomLights;
	public GameObject sun;

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, lowerShutters);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, raiseShutters);
	
	}

	public void lowerShutters( object obj = null )
	{
		GetComponent<Animation> () ["LowerShutters"].speed = 1f;
		GetComponent<Animation> () ["LowerShutters"].time = 0f;
		GetComponent<Animation> ().Play ("LowerShutters");
	}
	public void raiseShutters( object obj = null )
	{
		GetComponent<Animation> () ["LowerShutters"].speed = -1f;
		GetComponent<Animation> ()["LowerShutters"].time = GetComponent<Animation> ()["LowerShutters"].length;
		GetComponent<Animation> ().Play ("LowerShutters");
		//RenderSettings.ambientIntensity = 1.5f;
	}
}
