using UnityEngine;
using System.Collections;

/*! Transform which the UI for the Tools will be anchored to.
 * Usually, this is attached to a controller. If no controller is present, it can also be attached to the camera. */
public class ToolUIAnchor : MonoBehaviour {

	public static ToolUIAnchor instance { private set; get; }

	public void OnEnable()
	{
		if (instance != null) {
			throw(new System.Exception ("Error: Cannot create more than one instance of ToolUIAnchor!"));
		}
		instance = this;
	}
	public void OnDisable()
	{
		if( this == instance )
		{
			instance = null;
		}
	}
}
