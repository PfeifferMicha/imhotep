using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LeftController : Controller {

	// Use this for initialization
	new public void Start () {
		base.Start ();
		InputDeviceManager.instance.registerLeftController (this);
	}
}
