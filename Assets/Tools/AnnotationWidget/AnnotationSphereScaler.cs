using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UI;

public class AnnotationSphereScaler : MonoBehaviour{

	public GameObject bobble;

	public float maxScale = 15f;
	public float minScale = 0.5f;

	private bool reScale = false;
	private bool onMesh = false;
	private Vector3 rescaleDirection;

	public void rescaleAnnotation(BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button == PointerEventData.InputButton.Left) {
			rescaleDirection = bobble.transform.localPosition;
			reScale = true;
		}
	}

	public void stopRescale(BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button == PointerEventData.InputButton.Left) {
			reScale = false;
		}

	}

	private void rescaleMesh() {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		Plane intersectPlane = new Plane ();
		float dist = 0f;
		Ray intersectRay = inputDevice.createRay ();
		//Move Ray to CameraCenter
		intersectRay.origin = inputDevice.getEventCamera ().transform.position;

		//update plane

		Vector3 globalDir = this.transform.parent.TransformDirection (rescaleDirection);
		Vector3 camAnnoVec = (intersectRay.origin - this.transform.parent.position);

		Vector3 planeVec = Vector3.Cross (globalDir, camAnnoVec);
		Vector3 normal = Vector3.Cross (globalDir, planeVec);
		normal.Normalize ();

		intersectPlane.SetNormalAndPosition (normal, this.transform.parent.position);

		if (intersectPlane.Raycast (intersectRay, out dist)) {
			Vector3 intersectPoint = intersectRay.GetPoint (dist);
			Vector3 intersectDirection = intersectPoint - this.transform.parent.position;
			if (intersectDirection.magnitude < maxScale && intersectDirection.magnitude > minScale) {
				bobble.transform.position = intersectPoint;
				//Add scale only in one Direction
				float newScale = bobble.transform.localPosition.magnitude * 2;
				this.GetComponentInParent<Annotation> ().rescaleMesh (new Vector3 (newScale, newScale, newScale));
			}




		}

	}

	public void Start() {
		Vector3 scale = this.GetComponentInParent<Annotation> ().getMeshScale ();
		bobble.transform.localPosition = new Vector3 (scale.x / 2f, 0f, 0f);
	}

	public void Update() {
		if(reScale) {
			rescaleMesh ();
		}
	}
}
