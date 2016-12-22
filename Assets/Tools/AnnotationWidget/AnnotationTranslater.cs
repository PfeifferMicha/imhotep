using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnnotationTranslater : MonoBehaviour {

	//Mouse Moving attributs
	private bool translate;
	private Vector3 moveDirection;
	private Vector3 cursorAnnoVector;
	public float maxDistanceToMeshNode = 12f;


	public void dragXDirection (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		moveDirection = new Vector3 (1f, 0f, 0f);
		setupDirection ();
	}

	public void dragYDirection (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		moveDirection = new Vector3 (0f, 1f, 0f);
		setupDirection ();
	}

	public void dragZDirection (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		moveDirection = new Vector3 (0f, 0f, 1f);
		setupDirection ();
	}

	public void setupDirection() {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		Plane intersectPlane = new Plane ();
		//get distance cursor to center
		Vector3 camAnnoVec = (inputDevice.getEventCamera ().transform.position - this.transform.position);
		Vector3 globalDir = this.transform.TransformDirection (moveDirection);
		Vector3 planeVec = Vector3.Cross (globalDir, camAnnoVec);
		Vector3 normal = Vector3.Cross (planeVec, globalDir);
		//Debug.DrawLine (transform.position, transform.position + normal);
		normal.Normalize ();
		intersectPlane.SetNormalAndPosition (normal, transform.position);

		float dist = 0f;
		Ray intersectRay = inputDevice.createRay ();
		//Move Ray to CameraCenter
		intersectRay.origin = inputDevice.getEventCamera ().transform.position;
		if (intersectPlane.Raycast (intersectRay, out dist)) {
			Vector3 intersectPoint = intersectRay.GetPoint (dist);
			intersectPoint = this.transform.InverseTransformPoint (intersectPoint);
			intersectPoint = Vector3.Scale (intersectPoint, moveDirection);
			cursorAnnoVector = intersectPoint - this.transform.localPosition;
		}
		translate = true;
	}

	public void stopTranslation(BaseEventData eventData){
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		translate = false;
	}

	private void translateAnnotation () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		Plane intersectPlane = new Plane ();

		//get distance cursor to center
		Vector3 camAnnoVec = (inputDevice.getEventCamera ().transform.position - this.transform.position);
		Vector3 globalDir = this.transform.TransformDirection (moveDirection);
		Vector3 planeVec = Vector3.Cross (globalDir, camAnnoVec);
		Vector3 normal = Vector3.Cross (planeVec, globalDir);
		//Debug.DrawLine (transform.position, transform.position + normal);
		normal.Normalize ();
		intersectPlane.SetNormalAndPosition (normal, transform.position);

		float dist = 0f;
		Ray intersectRay = inputDevice.createRay ();
		//Move Ray to CameraCenter
		intersectRay.origin = inputDevice.getEventCamera ().transform.position;
		if (intersectPlane.Raycast (intersectRay, out dist)) {
			Vector3 intersectPoint = intersectRay.GetPoint (dist);
			//intersectPoint is Local
			intersectPoint = this.transform.InverseTransformPoint (intersectPoint);
			intersectPoint = Vector3.Scale (intersectPoint, moveDirection);
			intersectPoint -= cursorAnnoVector;
			intersectPoint = this.transform.TransformPoint (intersectPoint);

			Vector3 vectorToMesh = AnnotationControl.instance.meshPositionNode.transform.position - intersectPoint;

			//Dont go outside of the dome !!!
			if (vectorToMesh.magnitude < maxDistanceToMeshNode) {

				//Debug.DrawLine (transform.position, intersectPoint);
				this.GetComponentInParent<Annotation>().translateMesh(intersectPoint);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if(AnnotationControl.instance.meshPositionNode != null) {
			this.transform.rotation = AnnotationControl.instance.meshPositionNode.transform.rotation;
		}

		if(translate) {
			translateAnnotation ();
		}
	}
}
