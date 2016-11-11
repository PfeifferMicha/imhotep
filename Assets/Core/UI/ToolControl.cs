using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*! This class handles the tool options and how tools are selected/deselected.
 * For every Tool, this class automatically generates an entry in the ToolRing, which is placed around the
 * left controller to be chosen by the user. The class also handles the ToolRing movement and enabling/disabling.
 * 
 * Legacy: Before the last update, this class handled the creation of the tool stands instead - some code
 * is still left from this stage for backwards-compatability reasons. */
public class ToolControl : MonoBehaviour {

	public GameObject ToolStandPrefab;
	public GameObject ControllerPrefab;
	public Platform platform;
	public Sprite ToolSelectSprite;
	public Sprite ToolAcceptSprite;
	public Sprite ArrowL;
	public Sprite ArrowR;
	public Sprite Cancel;

	List<GameObject> toolStands = new List<GameObject>();
	List<GameObject> controllerChoises = new List<GameObject>();

	private GameObject activeTool = null;
	private GameObject activeToolChoise = null;

	public static ToolControl instance { private set; get; }

	public float controllerPickupDist = 0.3f;

	private GameObject toolRing;

	private float rotationStartTime = 0f;
	private float toolRingTargetAngle = 0.0f;
	private float rotationTime = 0.3f;

	private bool toolRingActive = false;
	private float movingStartTime = 0f;
	private Vector3 targetPosition;
	private float movingTime = 0.3f;
	private float toolRingY = 0.1f;
	private TextMesh ActiveToolName;

	public Color iconColor = new Color( 0.7f, 0.85f, 1.0f );

	//! The tool ring entry which is currently rotated towards the user
	private ToolRingEntry selectedToolEntry = null;

	public ToolControl() {
		instance = this;
	}

	void Start () {
		ToolStandPrefab.SetActive (false);

		// Register event callbacks for all Patient events:
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Loaded, generateAvailableTools );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, patientClosed );

		clearAllToolStands ();
		InputDeviceManager.instance.setLeftControllerTouchpadIconCentral (ToolSelectSprite);

		generateToolRing ();
	}

	void Update() {

		// If the input device is a controller, handle picking up and highlighting:
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType() == InputDeviceManager.InputDeviceType.ViveController) {
			Controller rc = inputDevice as Controller;

			// Check which tool choise is closest:
			float minDist = controllerPickupDist;
			GameObject closestController = null;
			foreach (GameObject controllerChoise in controllerChoises) {
					float dist = Vector3.Distance (
						            controllerChoise.transform.position,
							rc.transform.position);
					if (dist < minDist) {
						minDist = dist;
						closestController = controllerChoise;
					}
					controllerChoise.GetComponent<ToolChoise> ().UnHighlight ();
			}

			// Also check left controller:
			Controller lc = InputDeviceManager.instance.leftController;
			if (lc != null) {
				foreach (GameObject controllerChoise in controllerChoises) {
					float dist = Vector3.Distance (
						             controllerChoise.transform.position,
						             lc.transform.position);
					if (dist < minDist) {
						minDist = dist;
						closestController = controllerChoise;
					}
					//controllerChoise.GetComponent<ToolChoise> ().UnHighlight ();
				}
			}


			// If a tool choise controller was close enough, highlight it:
			if (closestController != null) {
				if (closestController.activeSelf) {
					ToolChoise tc = closestController.GetComponent<ToolChoise> ();
					tc.Highlight ();

					// If the user pressed the trigger, choose the tool:
					if (rc.triggerPressed() || lc.triggerPressed ()) {
						chooseTool (tc);
					}
				}
			}


			if( lc != null )
			{
				if( lc.touchpadButtonState == UnityEngine.EventSystems.PointerEventData.FramePressState.Released ) {
					if (lc.hoverTouchpadCenter()) {
						toggleToolRing ();
					} else if (toolRingActive) {
						if (lc.hoverTouchpadLeft()) {
							toolRingPrev ();
						} else if (lc.hoverTouchpadRight()) {
							toolRingNext ();
						} else if (lc.hoverTouchpadDown()) {
							toolRingCancel ();
						}
					}
				}
			}

		}

		if (toolRing != null) {
			// Rotate the tool ring if left/right was pressed:
			Quaternion targetRotation = Quaternion.AngleAxis (toolRingTargetAngle, Vector3.up);
			toolRing.transform.localRotation = Quaternion.Slerp (toolRing.transform.localRotation,
				targetRotation, (Time.time - rotationStartTime) / rotationTime);

			// Move the tool ring up or down when it's being activated or deactivated:
			toolRing.transform.localPosition = Vector3.Slerp( toolRing.transform.localPosition,
				targetPosition, (Time.time - movingStartTime) / movingTime);

			ActiveToolName.transform.localPosition = toolRing.transform.localPosition + new Vector3 ( 0f, -0.06f, -0.07f );

			updateToolRingIcons ();
		}
	}

	public void generateAvailableTools( object obj = null )
	{
		//////////////////////////////////////////////////
		/// Old, Tool Stands:
		uint i = 0;
		foreach (Transform child in transform) {
			string toolName = child.name;

			GameObject go = platform.toolStandPosition (i, (uint)transform.childCount);
			GameObject newToolStand = Object.Instantiate (ToolStandPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			newToolStand.name = "ToolStand (" + toolName + ")";
			newToolStand.transform.SetParent (go.transform, false);
			StartCoroutine (activateToolStand (newToolStand, Random.value * 0.25f + 0.3f * Mathf.Abs (transform.childCount * 0.5f - i)));

			toolStands.Add (newToolStand);

			GameObject controllerChoise = Object.Instantiate (ControllerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			controllerChoise.transform.localRotation = Quaternion.Euler (new Vector3 (0f, 270f, 270f));
			controllerChoise.transform.localPosition = new Vector3 (0.2f, 0f, 0.13f);
			ToolChoise tc = controllerChoise.GetComponent<ToolChoise> ();
			tc.toolName = toolName;
			tc.toolControl = this;
			Transform tableBone = newToolStand.transform.Find ("ToolStandArmature/BoneArm/BoneRotate/BoneSlide");
			controllerChoise.transform.SetParent (tableBone, false);
			controllerChoise.SetActive (true);
			controllerChoises.Add (controllerChoise);
			i++;
		}

	}

	/*! Create a new Tool Ring and add an entry for each available Tool.*/
	public void generateToolRing()
	{
		Controller lc = InputDeviceManager.instance.leftController;
		if (lc != null) {
			// If there's already a tool ring element, delete it:
			Transform oldToolRing = lc.transform.Find ("ToolRingAnchor");
			if (oldToolRing != null)
				Destroy (oldToolRing.gameObject);

			// Create new Tool Ring:
			GameObject anchor = new GameObject ("ToolRingAnchor");	// Anchor object for rotation/positon only.
			anchor.transform.SetParent (lc.transform, false);
			anchor.transform.localPosition = new Vector3 (0f, -0.025f, 0f);
			anchor.transform.localRotation = Quaternion.AngleAxis (85f, Vector3.right);
			toolRing = new GameObject ("ToolRing");		// Actual tool ring. Will only be rotated around its local Y axis.
			toolRing.transform.SetParent (anchor.transform, false);

			// Add a choice for each tool to the ring:
			int i = 0;
			int numTools = transform.childCount;
			float radius = 0.07f;
			foreach( Transform child in transform ) {

				ToolWidget tool = child.GetComponent<ToolWidget> ();
				if( tool != null ) {
					float currentAngle = (float)i * (2f*Mathf.PI) / (float)numTools;

					GameObject go = new GameObject (i.ToString ());
					go.transform.SetParent (toolRing.transform);
					go.transform.localPosition = radius*(new Vector3 ( -Mathf.Sin( currentAngle ), 0f, -Mathf.Cos( currentAngle ) ));
					go.transform.localRotation = Quaternion.AngleAxis (currentAngle*180f/Mathf.PI, Vector3.up);
					go.transform.localScale = new Vector3 (0.045f, 0.045f, 0.045f);
					SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
					sr.sprite = tool.ToolIcon;

					ToolRingEntry entry = go.AddComponent<ToolRingEntry> ();
					entry.Tool = tool;
					entry.name = tool.name;
				}
				i ++;
			}

			GameObject text = new GameObject ("ToolNameText");
			text.transform.SetParent (anchor.transform);
			text.transform.localPosition = new Vector3 ( 0f, -0.06f, -radius );
			text.transform.localRotation = Quaternion.AngleAxis (0f*180f/Mathf.PI, Vector3.up);
			text.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
			ActiveToolName = text.AddComponent<TextMesh> ();
			ActiveToolName.text = "";
			ActiveToolName.fontSize = 40;
			ActiveToolName.alignment = TextAlignment.Center;
			ActiveToolName.anchor = TextAnchor.MiddleCenter;
		}
	}

	public void removeToolRing()
	{
		targetPosition = new Vector3 (0f, 0f, 0.0f);
		movingTime = 0.5f;
		movingStartTime = Time.time;
	}

	public void activateToolRing()
	{
		if (toolRing == null)
			generateToolRing ();

		InputDeviceManager.instance.setLeftControllerTouchpadIconCentral (ToolAcceptSprite);
		InputDeviceManager.instance.setLeftControllerTouchpadIcons (ArrowL, ArrowR, null, Cancel);
		toolRingActive = true;

		// Start animation: (ease the ring in)
		targetPosition = new Vector3 (0f, toolRingY, 0f);
		movingTime = 1f;
		movingStartTime = Time.time;
	}

	public void toggleToolRing()
	{
		if( toolRingActive )
		{
			closeActiveTool ();
			removeToolRing ();
			InputDeviceManager.instance.setLeftControllerTouchpadIconCentral (ToolSelectSprite);
			InputDeviceManager.instance.setLeftControllerTouchpadIcons (null, null, null, null);
			toolRingActive = false;

			// Select the current tool:
			if( selectedToolEntry != null )
			{
				activeTool = selectedToolEntry.Tool.gameObject;
				// Move the active tool to the tool anchor:
				activeTool.SetActive (true);
				InputDeviceManager.instance.shakeLeftController( 3000 );
			}
		} else {
			closeActiveTool ();
			activateToolRing ();
		}
	}

	public void toolRingCancel()
	{
		removeToolRing ();
		InputDeviceManager.instance.setLeftControllerTouchpadIconCentral (ToolSelectSprite);
		InputDeviceManager.instance.setLeftControllerTouchpadIcons (null, null, null, null);
		toolRingActive = false;
	}

	/*! Rotate the ring of tools to the next tool */
	public void toolRingNext()
	{
		if (toolRing == null)
			return;
		
		int numTools = transform.childCount;
		float angle = (360f) / (float)numTools;
		setToolRingTargetRotation( toolRingTargetAngle + angle );
		//toolRing.transform.localRotation *= Quaternion.AngleAxis (angle, Vector3.up);
	}

	/*! Rotate the ring of tools to the previous tool */
	public void toolRingPrev()
	{
		if (toolRing == null)
			return;
		
		int numTools = transform.childCount;
		float angle = (360f) / (float)numTools;
		setToolRingTargetRotation( toolRingTargetAngle - angle );
		//toolRing.transform.localRotation *= Quaternion.AngleAxis (-angle, Vector3.up);
	}

	public void setToolRingTargetRotation( float angle )
	{
		toolRingTargetAngle = angle;
		rotationStartTime = Time.time;
		rotationTime = 0.3f;
	}

	public void updateToolRingIcons()
	{
		if (toolRing != null) {

			// Make the tool ring become less transparent as it eases in:
			float alpha = Mathf.Pow (toolRing.transform.localPosition.y / toolRingY, 2f);

			float scale = 0.75f + 0.25f * alpha;
			Vector3 scaleVec = 0.045f*(new Vector3( scale, scale, scale ));
			//toolRing.transform.localScale = new Vector3 (scale, scale, scale);

			float smallestAngleDiff = float.MaxValue;
			selectedToolEntry = null;

			foreach (Transform tf in toolRing.transform) {
				if (alpha > 0) {
					tf.gameObject.SetActive (true);

					float angleDiff = Mathf.Abs(-toolRing.transform.localEulerAngles.y - tf.localEulerAngles.y);
					angleDiff = angleDiff % 360f;		// Just to make sure...
					if (angleDiff > 180f)
						angleDiff = 360f - angleDiff;
					Color col = (1f - angleDiff / 360f)*(iconColor);
					// Make the tool ring become less transparent as 
					col.a *= alpha;
					tf.localScale = scaleVec;

					if(tf.GetComponent<SpriteRenderer> () != null)
						tf.GetComponent<SpriteRenderer> ().color = col;

					if (angleDiff < smallestAngleDiff) {
						smallestAngleDiff = angleDiff;
						selectedToolEntry = tf.GetComponent<ToolRingEntry> ();
					}
				} else {
					tf.gameObject.SetActive (false);
				}
			}
			if (selectedToolEntry != null) {
				Color col = Color.white;
				col.a *= alpha;
				selectedToolEntry.GetComponent<SpriteRenderer>().color = col;
				ActiveToolName.text = selectedToolEntry.name;
			}
			Color textCol = ActiveToolName.color;
			textCol.a = alpha*alpha;
			ActiveToolName.color = textCol;
			ActiveToolName.transform.localScale = scaleVec * 0.1f;
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
		foreach( GameObject controllerChoise in controllerChoises )
		{
			GameObject.Destroy (controllerChoise);
		}
		controllerChoises.Clear ();
		foreach( GameObject toolStand in toolStands )
		{
			GameObject.Destroy (toolStand);
		}
		toolStands.Clear ();
	}

	public void closeActiveTool()
	{
		if (activeTool != null) {
			Debug.Log ("Closing tool: " + activeTool.name);
			activeTool.SetActive (false);
			activeTool = null;
			if( activeToolChoise != null )
			{
				activeToolChoise.SetActive (true);		// make choosable again
			}
			activeToolChoise = null;
			InputDeviceManager.instance.resetToolIcons ();
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
				InputDeviceManager.instance.shakeLeftController( 3000 );
				return;
			}
		}
		Debug.LogWarning ("\tTool '" + tool.toolName + "' not found!");
	}
}
