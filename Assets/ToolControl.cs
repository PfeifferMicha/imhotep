using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolControl : MonoBehaviour {

	public GameObject ToolStand;

	List<GameObject> toolStands = new List<GameObject>();

	// Use this for initialization
	void Start () {
		ToolStand.SetActive (false);

		// Register event callbacks for all Patient events:
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Loaded, patientLoaded );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, patientClosed );

		clearAllToolStands ();

	}

	public void patientLoaded( object obj )
	{
		Patient p = obj as Patient;
		List<string> availableTools = new List<string> ();
		availableTools.Add ("Opacity Control");
		availableTools.Add ("Annotations");
		availableTools.Add ("Annotations");
		availableTools.Add ("Annotations");
		availableTools.Add ("Annotations");
		availableTools.Add ("Annotations");

		Platform platform = GetComponent<Platform> ();

		uint i = 0;
		foreach (string s in availableTools) {
			Debug.Log ("Generating tool stand for: " + s);
			GameObject go = platform.toolStandPosition (i, (uint)availableTools.Count);
			GameObject newToolStand = Object.Instantiate( ToolStand, Vector3.zero, Quaternion.identity) as GameObject;
			newToolStand.name = "ToolStand (" + s + ")";
			newToolStand.transform.SetParent (go.transform, false);
			StartCoroutine (activateToolStand (newToolStand, Random.value*0.25f + 0.3f*Mathf.Abs(availableTools.Count*0.5f - i)));
			i ++;
		}
	}

	public IEnumerator activateToolStand( GameObject newToolStand, float delayTime )
	{
		yield return new WaitForSeconds(delayTime);
		newToolStand.SetActive (true);
	}

	public void patientClosed( object obj )
	{
		Patient p = obj as Patient;
		clearAllToolStands ();
	}

	public void clearAllToolStands()
	{
		foreach( GameObject toolStand in toolStands )
		{
			GameObject.Destroy (toolStand);
		}
	}
}
