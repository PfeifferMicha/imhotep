using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolControl : MonoBehaviour {

	public GameObject ToolStandPrefab;
	public GameObject ControllerPrefab;
	public Platform platform;

	List<GameObject> toolStands = new List<GameObject>();

	private GameObject activeTool = null;
	private GameObject activeToolChoise = null;

	public static ToolControl instance { private set; get; }

	public ToolControl() {
		instance = this;
	}

	void Start () {
		ToolStandPrefab.SetActive (false);

		// Register event callbacks for all Patient events:
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Loaded, generateAvailableTools );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, patientClosed );

		clearAllToolStands ();
		//generateAvailableTools (null);
	}

	public void generateAvailableTools( object obj = null )
	{
		Patient p = obj as Patient;

		uint i = 0;
		foreach (Transform child in transform) {
			string toolName = child.name;


			Debug.Log ("Generating tool stand for: " + toolName);
			GameObject go = platform.toolStandPosition (i, (uint)transform.childCount);
			GameObject newToolStand = Object.Instantiate( ToolStandPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			newToolStand.name = "ToolStand (" + toolName + ")";
			newToolStand.transform.SetParent (go.transform, false);
			StartCoroutine (activateToolStand (newToolStand, Random.value*0.25f + 0.3f*Mathf.Abs(transform.childCount*0.5f - i)));

			toolStands.Add (newToolStand);

			GameObject controllerChoise = Object.Instantiate (ControllerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			controllerChoise.transform.localRotation = Quaternion.Euler (new Vector3 (0f, 270f, 270f));
			controllerChoise.transform.localPosition = new Vector3 ( 0.2f, 0f, 0.13f );
			ToolChoise tc = controllerChoise.GetComponent<ToolChoise> ();
			tc.toolName = toolName;
			tc.toolControl = this;
			Transform tableBone = newToolStand.transform.Find ("ToolStandArmature/BoneArm/BoneRotate/BoneSlide");
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
		closeActiveTool ();
		clearAllToolStands ();
	}

	public void clearAllToolStands()
	{
		foreach( GameObject toolStand in toolStands )
		{
			GameObject.Destroy (toolStand);
		}
	}

	public void closeActiveTool()
	{
		if (activeTool != null) {
			Debug.Log ("Closing tool: " + activeTool.name);
			activeTool.SetActive (false);
			activeTool = null;
			activeToolChoise.SetActive (true);		// make choosable again
			activeToolChoise = null;
		}
	}

	public void chooseTool( ToolChoise tool )
	{
		closeActiveTool ();

		Debug.Log ("Activating tool: " + tool.toolName);
		foreach (Transform child in transform) {
			if (child.name == tool.toolName) {
				activeTool = child.gameObject;
				// Move the active tool to the tool anchor:
				activeTool.SetActive (true);
				activeToolChoise = tool.gameObject;
				activeToolChoise.SetActive (false);		// Hide toolchoise
				InputDeviceManager.instance.shakeLeftController( 1000 );
				return;
			}
		}
		Debug.LogWarning ("\tTool '" + tool.toolName + "' not found!");
	}
}
