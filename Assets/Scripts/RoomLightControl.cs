using UnityEngine;
using System.Collections;

public class RoomLightControl : MonoBehaviour {

	public GameObject[] roomLights;
	public GameObject sun;

	private Animator animator;

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, lowerShutters);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, raiseShutters);

		animator = GetComponent<Animator>();
	}

	public void lowerShutters( object obj = null )
	{
		animator.SetTrigger ("Shut");
	}
	public void raiseShutters( object obj = null )
	{
		animator.SetTrigger ("Open");
	}
}
