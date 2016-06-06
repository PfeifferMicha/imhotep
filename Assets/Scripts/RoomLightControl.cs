using UnityEngine;
using System.Collections;

public class RoomLightControl : MonoBehaviour {

	public GameObject[] roomLights;
	public GameObject sun;
	public float lowAmbientIntensity;

	private Animator animator;
	private float initialAmbientIntensity;
	private bool mLoweringShutters = false;
	private bool mRaisingShutters = false;

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, lowerShutters);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, raiseShutters);

		animator = GetComponent<Animator>();

		initialAmbientIntensity = sun.GetComponent<Light> ().intensity;
	}

	public void lowerShutters( object obj = null )
	{
		animator.SetTrigger ("Shut");
		mLoweringShutters = true;
		mRaisingShutters = false;
	}
	public void raiseShutters( object obj = null )
	{
		animator.SetTrigger ("Open");
		mLoweringShutters = false;
		mRaisingShutters = true;
	}

	public void Update()
	{
		/*if (mLoweringShutters) {
			sun.GetComponent<Light> ().intensity = sun.GetComponent<Light> ().intensity - Time.deltaTime;
			if (sun.GetComponent<Light> ().intensity < lowAmbientIntensity) {
				sun.GetComponent<Light> ().intensity= lowAmbientIntensity;
				mLoweringShutters = false;
			}
		}*/
	}
}
