using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolControl : MonoBehaviour {

	public GameObject ToolStandPrefab;
	public GameObject ControllerPrefab;

	List<GameObject> toolStands = new List<GameObject>();

	// Use this for initialization
	void Start () {
		ToolStandPrefab.SetActive (false);

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
			GameObject newToolStand = Object.Instantiate( ToolStandPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			newToolStand.name = "ToolStand (" + s + ")";
			newToolStand.transform.SetParent (go.transform, false);
			StartCoroutine (activateToolStand (newToolStand, Random.value*0.25f + 0.3f*Mathf.Abs(availableTools.Count*0.5f - i)));

			GameObject controllerChoise = Object.Instantiate (ControllerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			controllerChoise.transform.localRotation = Quaternion.Euler (new Vector3 (0f, 270f, 270f));
			controllerChoise.transform.localPosition = new Vector3 ( 0.2f, 0f, 0.13f );
			ToolChoise tc = controllerChoise.GetComponent<ToolChoise> ();
			tc.toolName = s;
			Transform tableBone = newToolStand.transform.Find ("ToolStandArmature/BoneArm/BoneRotate/BoneSlide");
			Debug.Log ("Found: " + tableBone);
			controllerChoise.transform.SetParent( tableBone, false );
			controllerChoise.SetActive (true);

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
