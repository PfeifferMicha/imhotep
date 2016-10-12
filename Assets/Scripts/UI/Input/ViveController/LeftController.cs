using UnityEngine;
using System.Collections;

public class LeftController : MonoBehaviour {

	//--------------- controller stuff---------------------
	private SteamVR_Controller.Device controller { get{ return SteamVR_Controller.Input ((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	//-----------------------------------------------------

	public SteamVR_TrackedObject.EIndex controllerIndex {
		get {
			if (trackedObj != null) {
				return trackedObj.index;
			} else {
				return 0;
			}
		}
	}

	// Use this for initialization
	void Start () {
		trackedObj = this.GetComponent<SteamVR_TrackedObject> ();
		InputDeviceManager.instance.registerLeftController (this);
	}

	public void shake( ushort milliseconds )
	{
		SteamVR_Controller.Input( (int)controllerIndex ).TriggerHapticPulse( milliseconds );
	}
}
