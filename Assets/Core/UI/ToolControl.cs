using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*! This class handles the tool options and how tools are selected/deselected.
 * For every Tool, this class automatically generates an entry in the ToolRing, which is placed around the
 * left controller to be chosen by the user. The class also handles the ToolRing movement and enabling/disabling.
 * \note If the Rift Camera is enabled, the RiftToolRing will be used to display an array of available tools. */
public class ToolControl : MonoBehaviour {

	public Platform platform;
	public Sprite ToolSelectSprite;
	public Sprite ToolAcceptSprite;
	public Sprite ArrowL;
	public Sprite ArrowR;
	public Sprite Cancel;

	private GameObject activeTool = null;

	public static ToolControl instance { private set; get; }

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

	private List<ToolWidget> availableTools = new List<ToolWidget> ();

	public ToolControl() {
		instance = this;
	}

	void Start () {
		// Register event callbacks for all Patient events:
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_FinishedLoading, patientLoaded );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, patientClosed );

		InputDeviceManager.instance.setLeftControllerTouchpadIconCentral (ToolSelectSprite);
	}

	void Update() {

		// If the input device is a controller, handle the touch-pad-selection:
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType() == InputDeviceManager.InputDeviceType.ViveController) {
			Controller lc = InputDeviceManager.instance.leftController;
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

	public void updateAvailableTools( object obj = null )
	{
		availableTools = new List<ToolWidget> ();

		foreach (Transform child in transform) {
			ToolWidget tool = child.GetComponent<ToolWidget> ();
			if (tool != null) {
				// Only show the tool if it's currently available:
				if (tool.displayTime == ToolWidget.ToolDisplayTime.Always ||
				   (tool.displayTime == ToolWidget.ToolDisplayTime.WhenPatientIsLoaded && Patient.getLoadedPatient () != null) ||
				   (tool.displayTime == ToolWidget.ToolDisplayTime.WhenNoPatientIsLoaded && Patient.getLoadedPatient () == null)) {
					availableTools.Add (tool);
				}
			}
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
			int numTools = availableTools.Count;
			float radius = 0.07f;
			foreach (ToolWidget tool in availableTools) {
				Debug.Log ("Tool: " + tool);
				if (tool != null) {
					float currentAngle = (float)i * (2f * Mathf.PI) / (float)numTools;

					GameObject go = new GameObject (i.ToString ());
					go.transform.SetParent (toolRing.transform);
					go.transform.localPosition = radius * (new Vector3 (-Mathf.Sin (currentAngle), 0f, -Mathf.Cos (currentAngle)));
					go.transform.localRotation = Quaternion.AngleAxis (currentAngle * 180f / Mathf.PI, Vector3.up);
					go.transform.localScale = new Vector3 (0.045f, 0.045f, 0.045f);
					SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
					sr.sprite = tool.ToolIcon;

					ToolRingEntry entry = go.AddComponent<ToolRingEntry> ();
					entry.Tool = tool;
					entry.name = tool.name;
				}
				i++;
			}

			GameObject text = new GameObject ("ToolNameText");
			text.transform.SetParent (anchor.transform);
			text.transform.localPosition = new Vector3 (0f, -0.06f, -radius);
			text.transform.localRotation = Quaternion.AngleAxis (0f * 180f / Mathf.PI, Vector3.up);
			text.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
			ActiveToolName = text.AddComponent<TextMesh> ();
			ActiveToolName.text = "";
			ActiveToolName.fontSize = 40;
			ActiveToolName.alignment = TextAlignment.Center;
			ActiveToolName.anchor = TextAnchor.MiddleCenter;
		} else {
			// If no controller is active, let the RiftToolRing handle the display of available tools:
			RiftToolRing.instance.setAvailableTools (availableTools);
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
		updateAvailableTools ();

		Debug.Log ("TOols: " + availableTools.Count);

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

	public void patientLoaded( object obj = null )
	{
		updateAvailableTools ();
		generateToolRing ();
	}

	public void patientClosed( object obj = null )
	{
		if (activeTool != null) {
			if (activeTool.GetComponent<ToolWidget> ().displayTime == ToolWidget.ToolDisplayTime.WhenPatientIsLoaded) {
				closeActiveTool ();
			}
		}
		updateAvailableTools ();
		generateToolRing ();
	}

	public void closeActiveTool()
	{
		if (activeTool != null) {
			Debug.Log ("Closing tool: " + activeTool.name);
			activeTool.SetActive (false);
			activeTool = null;
			InputDeviceManager.instance.resetToolIcons ();
		}
	}

	public void chooseTool( ToolWidget tool )
	{
		closeActiveTool ();
		activeTool = tool.gameObject;
		// Move the active tool to the tool anchor:
		activeTool.SetActive (true);
		InputDeviceManager.instance.shakeLeftController( 3000 );
	}
}
