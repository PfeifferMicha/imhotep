using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnnotationRotater : MonoBehaviour {

	private GameObject meshPositionNode;
	private bool rotate;
	private Vector3 localRotationAxis;


	public void startRotate (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		this.transform.GetChild (0).gameObject.SetActive (true);
		Vector3 localUp = this.transform.parent.InverseTransformDirection (this.transform.parent.forward);
		localRotationAxis = Vector3.Cross (localUp, this.transform.localPosition);


		rotate = true;
	}

	public void stopRotate (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (data.button != PointerEventData.InputButton.Left) {
			return;
		}
		this.transform.GetChild (0).gameObject.SetActive (false);
		rotate = false;
	}

	private void RotateObject () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		float dist = 0f;
		Plane intersectPlane = new Plane ();

		Vector3 normal = this.transform.parent.TransformDirection(localRotationAxis.normalized);
		intersectPlane.SetNormalAndPosition (normal, this.transform.parent.position);
		//Debug.DrawLine (this.transform.parent.position, this.transform.parent.position + normal);
		Ray intersectRay = inputDevice.createRay ();
		//Move Ray to CameraCenter
		intersectRay.origin = inputDevice.getEventCamera ().transform.position;
		if (intersectPlane.Raycast (intersectRay, out dist)) {
			Vector3 intersectPoint = intersectRay.GetPoint (dist);
			//intersectPoint local

			//Debug.DrawLine (this.transform.parent.position, this.transform.position);
			//Debug.DrawLine (this.transform.parent.position, intersectPoint);
			Vector3 posDir = this.transform.position - this.transform.parent.position;
			Vector3 intersectDir = intersectPoint - this.transform.parent.position;
			float angle = Vector3.Angle (posDir, intersectDir);

			//angle pos or neg ?
			if(Vector3.Dot(Vector3.Cross(posDir, intersectDir), normal) < 0) {
				angle =  -angle;
			}

			//Debug.Log (angle);
			this.GetComponentInParent<Annotation> ().rotateMesh(Quaternion.AngleAxis (angle, localRotationAxis));
		
		}
	}

	// Update is called once per frame
	void Update () {
		this.transform.parent.rotation = this.GetComponentInParent<Annotation>().myAnnotationMesh.transform.rotation;
		if(rotate) {
			RotateObject ();
		}
	}
}
