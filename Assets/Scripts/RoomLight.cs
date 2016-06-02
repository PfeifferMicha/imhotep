using UnityEngine;
using System.Collections;

public class RoomLight : MonoBehaviour {

	private Light mLight;

	private float mInitialRange;
	private float mInitialIntensity;
	// Use this for initialization
	void Start () {
		mLight = GetComponent<Light> ();
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, dimLights);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, raiseLights);

		mInitialRange = mLight.range;
		mInitialIntensity = mLight.intensity;
	}

	public void dimLights( object obj = null )
	{
		mLight.range = 4f;
		mLight.intensity = 1f;
		RenderSettings.ambientIntensity = 0.1f;
	}

	public void raiseLights( object obj = null )
	{
		mLight.range = mInitialRange;
		mLight.intensity = mInitialIntensity;
		RenderSettings.ambientIntensity = 0.3f;
	}
}
