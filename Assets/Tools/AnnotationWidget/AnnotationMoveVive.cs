using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnnotationMoveVive : MonoBehaviour {


	//Vive moving attributs
	private bool move;
	private bool rotateWithVive;
	private GameObject controllerRight;
	private GameObject controllerLeft;
	private Vector3 vecToMeshCenterLocal;
	private Vector3 hitPointStartWorld;
	private Vector3 hitPointStartLocal;
	private Vector3 prevHitPointScale;
	private Vector3 prevleftPos;
	private Vector3 prevRightPos;
	private Vector3 startScale;
	private float disControllerToMesh;
	private Quaternion offsetRot;
	private Quaternion annotationRot;

	public float maxScale = 50f;
	public float minScale = 5f;
	public float twoControllerRescaleFactor = 4f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(move) {
			moveWithVive3 ();
		}
	}

	private void dropMovement () {
		move = false;
		controllerRight = null;
		//Movement active
		GetComponentInParent<Annotation>().ViveMovement(false);
		this.GetComponentInParent<Annotation> ().saveAnnotationChanges ();


	}


	public void AnnotationClicked(PointerEventData eventData, bool rotateVive) {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			rotateWithVive = rotateVive;
			hitPointStartWorld = eventData.pointerCurrentRaycast.worldPosition;
			hitPointStartLocal = this.transform.InverseTransformPoint (hitPointStartWorld);
			Vector3 vecToMeshCenter = this.transform.position - hitPointStartWorld;
			startScale = this.transform.localScale;
			vecToMeshCenterLocal = this.transform.InverseTransformDirection (vecToMeshCenter);

			//Movement active
			GetComponentInParent<Annotation>().ViveMovement(true);

			moveAnnotationWithVive3 ();
		}
	}





	//########### Vive movement method 1

	//Called by annotation click with vive
	private void moveAnnotationWithVive1 () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			GameObject.Find("GlobalScript").GetComponent<HierarchicalInputModule>().disableLayer("UIOrgans");
			controllerRight = (inputDevice as Controller).gameObject;
			disControllerToMesh =  (hitPointStartWorld - controllerRight.transform.position).magnitude;
			annotationRot = this.transform.rotation;
			move = true;
		}
	}

	//Called every Frame
	private void moveWithVive1 () {
		if (controllerRight.GetComponent<Controller> ().triggerPressed ()) {
			Quaternion newRotation = controllerRight.transform.rotation;
			this.transform.rotation = newRotation * Quaternion.Inverse(offsetRot) * annotationRot;
			transform.parent.position = controllerRight.transform.position 
				+ controllerRight.transform.forward.normalized * disControllerToMesh
				+ this.transform.TransformDirection(vecToMeshCenterLocal);
		} else {
			dropMovement ();
		}

	}

	//########### Vive movement method 2

	//Called by annotation click with vive
	private void moveAnnotationWithVive2 () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			GameObject.Find("GlobalScript").GetComponent<HierarchicalInputModule>().disableLayer("UIOrgans");
			controllerRight = (inputDevice as Controller).gameObject;
			disControllerToMesh =  (hitPointStartWorld - controllerRight.transform.position).magnitude;
			offsetRot = controllerRight.transform.rotation;
			annotationRot = this.transform.rotation;
			move = true;
		}
	}

	//Called every Frame
	private void moveWithVive2 () {
		if (controllerRight.GetComponent<Controller> ().triggerPressed ()) {
			if(rotateWithVive) {
				//rotate
				Vector3 hitPoint = this.transform.InverseTransformPoint(controllerRight.transform.position + controllerRight.transform.forward.normalized * disControllerToMesh);
				Vector3 oldDir = hitPointStartLocal - this.transform.localPosition;
				Vector3 newDir = hitPoint - this.transform.localPosition;
				Vector3 rotAxis = Vector3.Cross (oldDir, newDir);
				float angle = Vector3.Angle (oldDir, newDir);
				this.transform.localRotation *= Quaternion.AngleAxis(angle, rotAxis);

			} else {
				//move
				transform.parent.position = controllerRight.transform.position 
					+ controllerRight.transform.forward.normalized * disControllerToMesh
					+ this.transform.TransformDirection(vecToMeshCenterLocal);
			}
		}  else {
			dropMovement ();
		}

	}


	//########### Vive movement method 3 (kombo method 1 & 2)

	//Called by annotation click with vive
	private void moveAnnotationWithVive3 () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		GameObject.Find("GlobalScript").GetComponent<HierarchicalInputModule>().disableLayer("UIOrgans");
		controllerRight = (inputDevice as Controller).gameObject;
		disControllerToMesh = (hitPointStartWorld - controllerRight.transform.position).magnitude;
		offsetRot = controllerRight.transform.rotation;
		annotationRot = this.transform.rotation;
		move = true;
	}

	//Called every Frame
	private void moveWithVive3 () {
		if (controllerRight.GetComponent<Controller> ().triggerPressed ()) {
			if(rotateWithVive) {
				//rotate
				Vector3 hitPoint = this.transform.InverseTransformPoint(controllerRight.transform.position + controllerRight.transform.forward.normalized * disControllerToMesh);
				Vector3 oldDir = hitPointStartLocal - this.transform.localPosition;
				Vector3 newDir = hitPoint - this.transform.localPosition;

				Vector3 rotAxis = Vector3.Cross (oldDir, newDir);
				float angle = Vector3.Angle (oldDir, newDir);
				this.transform.localRotation *= Quaternion.AngleAxis(angle, rotAxis);

				//scale
				rescaleStick(hitPoint);

			} else {
				//move
				Quaternion newRotation = controllerRight.transform.rotation;
				this.transform.rotation = newRotation * Quaternion.Inverse(offsetRot) * annotationRot;
				transform.parent.position = controllerRight.transform.position 
					+ controllerRight.transform.forward.normalized * disControllerToMesh
					+ this.transform.TransformDirection(vecToMeshCenterLocal);

			}
		}  else {
			dropMovement ();
		}

	}

	//########### Vive movement method 4 (2 controller)

	//Called by annotation click with vive
	private void moveAnnotationWithVive4 () {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			GameObject.Find ("GlobalScript").GetComponent<HierarchicalInputModule> ().disableLayer ("UIOrgans");
			controllerRight = (inputDevice as Controller).gameObject;
			controllerLeft = InputDeviceManager.instance.leftController.gameObject;
			disControllerToMesh =  (hitPointStartWorld - controllerRight.transform.position).magnitude;
			offsetRot = controllerRight.transform.rotation;
			annotationRot = this.transform.rotation;
			prevleftPos = controllerLeft.transform.position;
			prevRightPos = controllerRight.transform.position;
			move = true;
		}
	}

	//Called every Frame
	private void moveWithVive4 () {

		if (controllerRight.GetComponent<Controller> ().triggerPressed ()) {
			Vector3 oldDir = prevleftPos - prevRightPos;
			Vector3 newDir = controllerLeft.transform.position - controllerRight.transform.position;
			oldDir = this.transform.InverseTransformDirection(oldDir);
			newDir = this.transform.InverseTransformDirection(newDir);

			if(rotateWithVive) {
				//scale
				rescaleTwoControlers(oldDir, newDir);

			} else {
				//move
				//position
				transform.parent.position = controllerRight.transform.position 
					+ controllerRight.transform.forward.normalized * disControllerToMesh
					+ this.transform.TransformDirection(vecToMeshCenterLocal);

				//rotation
				Vector3 rotAxis = Vector3.Cross (oldDir, newDir);
				float angle = Vector3.Angle (oldDir, newDir);
				this.transform.localRotation *= Quaternion.AngleAxis(angle, rotAxis);


			}
			prevleftPos = controllerLeft.transform.position;
			prevRightPos = controllerRight.transform.position;

		}  else {
			dropMovement ();
		}

	}


	private void rescaleStick(Vector3 hitPoint) {
		Vector3 newScale;
		if(GetComponentInParent<Annotation>().myType == AnnotationControl.AnnotationType.sphere) {

			float dist = (transform.TransformPoint(hitPoint) - transform.position).magnitude;
			float refValue = (hitPointStartWorld - transform.position).magnitude;
			float d = dist * (startScale.x / refValue);
			newScale = new Vector3(d,d,d);
			this.transform.localScale = newScale;
		} else if(GetComponentInParent<Annotation>().myType == AnnotationControl.AnnotationType.plane) {
			Vector3 delta = new Vector3(Mathf.Abs(hitPoint.x) - Mathf.Abs(hitPointStartLocal.x),
				Mathf.Abs(hitPoint.y) - Mathf.Abs(hitPointStartLocal.y),
				Mathf.Abs(hitPoint.z) - Mathf.Abs(hitPointStartLocal.z));
			newScale = delta * 2 + this.transform.localScale;
			if (Mathf.Max(newScale.x, newScale.y) > maxScale || Mathf.Min(newScale.x, newScale.y) < minScale) {
				return;
			}
			this.transform.localScale = newScale;
		}
	}

	private void rescaleTwoControlers(Vector3 oldDir, Vector3 newDir) {
		Vector3 newScale;
		if(GetComponentInParent<Annotation>().myType == AnnotationControl.AnnotationType.sphere) {
			float d = newDir.magnitude * (this.transform.localScale.x / oldDir.magnitude);
			newScale = new Vector3(d,d,d);
			this.transform.localScale = newScale;
		} else if(GetComponentInParent<Annotation>().myType == AnnotationControl.AnnotationType.plane) {
			Vector3 delta = new Vector3(Mathf.Abs(newDir.x) - Mathf.Abs(oldDir.x),
				Mathf.Abs(newDir.y) - Mathf.Abs(oldDir.y),
				Mathf.Abs(newDir.z) - Mathf.Abs(oldDir.z));
			newScale = delta * 2f * twoControllerRescaleFactor + this.transform.localScale;
			if (Mathf.Max(newScale.x, newScale.y) > maxScale || Mathf.Min(newScale.x, newScale.y) < minScale) {
				return;
			}
			this.transform.localScale = newScale;
		}
	}
}
