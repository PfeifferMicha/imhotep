using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnnotationScaler : MonoBehaviour {

	public GameObject arrow1;
	public GameObject arrow2;

	public float maxScale = 15f;
	public float minScale = 0.5f;

	private bool reScale = false;
	private Vector3 rescaleDirection;

	public void rescaleAnnotation(BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		rescaleDirection = Vector3.Scale(arrow1.transform.localPosition,arrow1.transform.localPosition).normalized;
		reScale = true;
	}

	public void stopRescale(BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		reScale = false;
	}

	private void rescaleMesh() {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		Plane intersectPlane = new Plane ();
		float dist = 0f;
		Ray intersectRay = inputDevice.createRay ();
		//Move Ray to CameraCenter
		intersectRay.origin = inputDevice.getEventCamera ().transform.position;

		//update plane
		Vector3 camAnnoVec = (intersectRay.origin - this.transform.position);
		Vector3 globalDir = this.transform.TransformDirection (rescaleDirection);
		Vector3 planeVec = Vector3.Cross (globalDir, camAnnoVec);
		Vector3 normal = Vector3.Cross (globalDir, planeVec);
		normal.Normalize ();

		intersectPlane.SetNormalAndPosition (normal, this.transform.position);

		if (intersectPlane.Raycast (intersectRay, out dist)) {
			Vector3 intersectPoint = intersectRay.GetPoint (dist);
			//intersectPoint is Local
			intersectPoint = this.transform.InverseTransformPoint (intersectPoint);

			//reduce to point on scale direction
			intersectPoint = Vector3.Scale (intersectPoint, rescaleDirection);
			//Intersect Point always Positiv
			intersectPoint = Vector3.Scale (intersectPoint, intersectPoint.normalized);
			Vector3 newScale = (intersectPoint.normalized * intersectPoint.magnitude) * 2;

			if (newScale.magnitude > maxScale || newScale.magnitude < minScale) {
				return;
			}

			arrow2.transform.localPosition = 0.5f * newScale + newScale.normalized;
			arrow1.transform.localPosition =  -1f * arrow2.transform.localPosition;

			Vector3 actScale = this.GetComponentInParent<Annotation> ().getMeshScale ();
			//Add scale only in one Direction
			newScale = actScale - Vector3.Scale (actScale, rescaleDirection) + newScale;


			this.GetComponentInParent<Annotation> ().rescaleMesh (newScale);


		}

	}

	public void Start() {
		Vector3 scale = this.GetComponentInParent<Annotation> ().getMeshScale ();
		Vector3 rescaleDir = Vector3.Scale(arrow1.transform.localPosition,arrow1.transform.localPosition).normalized;
		scale = Vector3.Scale (rescaleDir, scale);
		arrow2.transform.localPosition = 0.5f * scale + scale.normalized;
		arrow1.transform.localPosition =  -1f * arrow2.transform.localPosition;
	}

	public void Update() {
		this.transform.rotation = this.GetComponentInParent<Annotation>().myAnnotationMesh.transform.rotation;
		if(reScale) {
			rescaleMesh ();
		}
	}
}
