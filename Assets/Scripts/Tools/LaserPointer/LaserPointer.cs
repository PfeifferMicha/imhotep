using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LaserPointer : MonoBehaviour {

	private bool isOn = true;
	private Mouse3DMovement mMouse;

	public Button onOffButton;
	private LineRenderer lineRenderer;


	// Use this for initialization
	void Start () {
		mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> ();
		if (mMouse == null)
		{
			Debug.LogError("Can't find Mouse3DMovement script");
		}
		if (onOffButton == null) {
			Debug.LogError ("[LaserPointer.cs] Button not set"); 
		}
		lineRenderer = this.GetComponent<LineRenderer> ();
		if (lineRenderer == null) {
			Debug.LogError ("[LaserPointer.cs] Line Renderer not set"); 
		}

	}
	
	// Update is called once per frame
	void Update () {
		if(isOn){
			
			RaycastHit hit;
			Ray ray;

			//Use different ray for mouse and vive controller
			if (mMouse.owner.name == "mouse") //TODO !!! Use interface 
			{ 
				ray = new Ray(Camera.main.transform.position, mMouse.transform.position - Camera.main.transform.position); //TODO generate ray in Mouse3DMovement

				LayerMask onlyMeshViewLayer = 1000000000; // hits only the mesh view layer
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, onlyMeshViewLayer)) {
					Vector3 offset = new Vector3 (0.1f, 0.1f, 0);
					lineRenderer.SetPosition (0, Camera.main.transform.position + offset);
					lineRenderer.SetPosition (1, hit.point);
				} else {
					Vector3 zero = new Vector3 (0, 0, 0);
					lineRenderer.SetPosition(0, zero);
					lineRenderer.SetPosition(1, zero);
				}
			}
			/*else TODO controller
			{
				ray = new Ray(mMouse.owner.transform.position, mMouse.owner.transform.forward);
			}*/


		}
	}

	public void onOffButtonPressed(){
		isOn = !isOn;

		string bt = "";
		if (isOn) {
			bt = "On";
			onOffButton.image.color = Color.white;

			lineRenderer.enabled = true;
		} else {
			bt = "Off";
			onOffButton.image.color = Color.red;

			lineRenderer.enabled = false;
		}

		Text t = onOffButton.GetComponentInChildren<Text>();
		if (t != null)
		{
			t.text = bt;
		}

	}
}
