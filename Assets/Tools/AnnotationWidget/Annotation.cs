using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI;
using System.Collections.Generic;

public class Annotation : MonoBehaviour,  IPointerHoverHandler{

	[DefaultValue ("")]
	public string creator;
	public DateTime creationDate;
	//needs to be public so clone dont "loose" knowledge of his label ... but still child
	public GameObject myAnnotationLabel;
	public GameObject myAnnotationMesh;
	public GameObject rotationObjects;
	public GameObject translationObjects;
	public GameObject scalingObjects;
	public GameObject viveMoveObject;
	public GameObject myAnnotationListEntry;
	public AnnotationControl.AnnotationType myType;
	private Color myColor;
	private Color defaultColor;

	public Material defaultMaterial, previewMaterial;
	public Collider myCollider;
	public float maxDistanceToMeshNode = 12f;

	public float MoveCircleRadiant = 0.5f;
	private bool rotateWithVive;

	// Use this for initialization
	void Start () {
		creationDate = DateTime.Now;
	}

	void OnEnable () {
		defaultColor = new Color (defaultMaterial.color.r, defaultMaterial.color.g, defaultMaterial.color.b);
		defaultMaterial = Instantiate (defaultMaterial);
		previewMaterial = Instantiate (previewMaterial);
	}

	//Used to create new Label
	public void CreateLabel (GameObject annotationLabelMaster) {
		//Create Label
		myAnnotationLabel = (GameObject)Instantiate (annotationLabelMaster, new Vector3 (0f, 0f, 15f), this.transform.localRotation);
		myAnnotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		myAnnotationLabel.transform.SetParent (this.transform, false);
		myAnnotationLabel.SetActive (true);
		myAnnotationLabel.GetComponent<AnnotationLabel> ().setLabelText (annotationLabelMaster.GetComponent<AnnotationLabel> ().getLabelText ());

	}

	//Updates the Label Text
	public void setLabeText (String newLabel) {

		if (myAnnotationLabel == null) {
			Debug.LogError ("Annotation has no AnnotationLabel to edit");
			return;
		}
		// Change label text:
		myAnnotationLabel.GetComponent<AnnotationLabel> ().setLabelText (newLabel);
	}


	/// <summary>
	/// Updates the position and Rotation in local Coordinates.
	/// </summary>
	/// <param name="rotation">new Rotation.</param>
	/// <param name="position">new Position.</param>
	public void updatePosition (Quaternion rotation, Vector3 position) {
		this.transform.localPosition = position;
		this.transform.localRotation = rotation;
	}
		
	//to get AnnotationLabel
	public GameObject getLabel () {
		return myAnnotationLabel;
	}

	//to get Annotation Label text
	public String getLabelText () {
		return myAnnotationLabel.GetComponent<AnnotationLabel> ().getLabelText ();
	}

	//to get Color
	public Color getColor () {
		return new Color (myColor.r, myColor.g, myColor.b, myAnnotationMesh.GetComponent<MeshRenderer> ().material.color.a);
	}

	//Used to save Label Changes
	public void saveLabelChanges () {
		if (myAnnotationListEntry != null) {
			myAnnotationListEntry.GetComponent<AnnotationListEntry> ().updateLabel (getLabelText ());
		}
		AnnotationControl.instance.updatePatientAnnotationList ();
	}

	//used to change color of Annotation
	public void changeColor (Color newColor) {
		myColor = new Color (newColor.r, newColor.g, newColor.b, myAnnotationMesh.GetComponent<MeshRenderer> ().material.color.a);
		Material[] mats = myAnnotationMesh.GetComponent<MeshRenderer> ().materials;
		for (int i = 0; i < mats.GetLength (0); i++) {
			mats [i].color = myColor;
		}
		myAnnotationMesh.GetComponent<MeshRenderer> ().materials = mats;
	}

	//used to change color of Annotation
	public void setDefaultColor () {
		changeColor (new Color (defaultColor.r, defaultColor.g, defaultColor.b, myAnnotationMesh.GetComponent<Renderer> ().material.color.a));
	}

	/// <summary>
	/// Makes the Annotation transperent.
	/// </summary>
	public void makeTransperent (float alpha) {
		Material[] mats = myAnnotationMesh.GetComponent<MeshRenderer> ().materials;
		if(alpha == 1f) {
			//make Opaque
			for (int i = 0; i < mats.GetLength (0); i++) {
				Color pinColor = mats [i].color;
				pinColor.a = 1f;
				mats [i] = defaultMaterial;
				mats [i].color = pinColor;
			}
			myAnnotationMesh.GetComponent<MeshRenderer> ().materials = mats;
			return;
		}
		for (int i = 0; i < mats.GetLength (0); i++) {
			Color pinColor = mats [i].color;
			pinColor.a = alpha;
			mats [i] = previewMaterial;
			mats [i].color = pinColor;
		}
		myAnnotationMesh.GetComponent<MeshRenderer> ().materials = mats;
	}

	/// <summary>
	/// Reste Transparency of Annotation
	/// </summary>
	public void setDefaultTransparency () {
		if(myType == AnnotationControl.AnnotationType.sphere) {
			makeTransperent (AnnotationControl.instance.defaultTransperency);
			return;
		}
		if(myType == AnnotationControl.AnnotationType.plane) {
			makeTransperent (AnnotationControl.instance.defaultTransperency);
			return;
		}
		if(myType == AnnotationControl.AnnotationType.pin) {
			//make Opaque
			Material[] mats = myAnnotationMesh.GetComponent<MeshRenderer> ().materials;
			for (int i = 0; i < mats.GetLength (0); i++) {
				Color pinColor = mats [i].color;
				pinColor.a = 1f;
				mats [i] = defaultMaterial;
				mats [i].color = pinColor;
			}
			myAnnotationMesh.GetComponent<MeshRenderer> ().materials = mats;
			return;
		}

	}

	//Disables MeshCollider
	public void disableMeshCollider () {
		myCollider.enabled = false;
	}

	public void enableAllCollider (bool enable) {
		myCollider.enabled = enable;
		myAnnotationLabel.GetComponent<AnnotationLabel> ().myBoxCollider.enabled = enable;
	}

	//Transfers all Settings from old Annotation in this one (Label color etc.)
	public void transferAnnotationSettings (GameObject settingSource) {
		changeColor (settingSource.GetComponent<Annotation> ().getColor ());
		setLabeText (settingSource.GetComponent<Annotation> ().getLabelText ());
	}


	/// <summary>
	/// Sets the movement meshs active.
	/// </summary>
	/// <param name="active">If set to <c>true</c> active the moving Objects appear depending on active Controller and the Annotation gets Transparent.</param>
	public void setMovementMeshsActive (bool active) {
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;

		if(active) {
			makeTransperent (0.4f);
			changeAnnotationLabelLayer ("UIAnnotationEdit");
			GameObject.Find("GlobalScript").GetComponent<HierarchicalInputModule>().disableLayer("UIOrgans");
		} else {
			setDefaultTransparency ();
			GameObject.Find("GlobalScript").GetComponent<HierarchicalInputModule>().enableLayer("UIOrgans");
			changeAnnotationMeshLayer ("UIOrgans");
			changeAnnotationLabelLayer ("UIOrgans");
		}

		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.Mouse) {
			if (rotationObjects != null) {
				rotationObjects.SetActive (active);
			}
			if (translationObjects != null) {
				translationObjects.SetActive (active);
			}
			if (scalingObjects != null) {
				scalingObjects.SetActive (active);
			}
		} else if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			changeAnnotationMeshLayer ("UIAnnotationEdit");
			if(viveMoveObject != null) {
				viveMoveObject.SetActive (active);
			}
		}
	}

	public void OnPointerHover (UnityEngine.EventSystems.PointerEventData eventData) {
		//Debug.Log ("Hover");
		if(viveMoveObject == null) {
			return;
		}
		if(eventData.pointerEnter != myAnnotationMesh) {
			return;
		}
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.ViveController) {
			Vector3 localEventPos = transform.InverseTransformPoint (eventData.pointerCurrentRaycast.worldPosition);
			GameObject trans = viveMoveObject.transform.GetChild (0).gameObject;
			GameObject rot = viveMoveObject.transform.GetChild (1).gameObject;
			viveMoveObject.transform.position = eventData.pointerCurrentRaycast.worldPosition;
			if(myType == AnnotationControl.AnnotationType.sphere) {
				//move or rotate with sphere
				Vector3 localDevicePos = transform.InverseTransformPoint (inputDevice.getEventCamera().transform.position);
				float angle = Vector3.Angle (localEventPos, localDevicePos);
				if(angle > 30) {
					rotateWithVive = true;
					trans.SetActive (false);
					rot.SetActive (true);
				} else {
					rotateWithVive = false;
					rot.SetActive (false);
					trans.SetActive (true);
				}
			} else if(myType == AnnotationControl.AnnotationType.plane) {
				//move or rotate with plane
				float minScale = Mathf.Min (myAnnotationMesh.transform.localScale.x, myAnnotationMesh.transform.localScale.y) / 2f;
				float distance = (localEventPos - myAnnotationMesh.transform.localPosition).magnitude * 1f/minScale;

				if(distance > MoveCircleRadiant) {
					rotateWithVive = true;
					trans.SetActive (false);
					rot.SetActive (true);
				} else {
					rotateWithVive = false;
					rot.SetActive (false);
					trans.SetActive (true);
				}
			}
		}

	}

	public void endHover() {
		if(viveMoveObject == null) {
			return;
		}
		viveMoveObject.transform.GetChild (0).gameObject.SetActive(false);
		viveMoveObject.transform.GetChild (1).gameObject.SetActive(false);
	}

	//rescaling AnnotationMesh
	public void rescaleMesh (Vector3 newScale) {
		if(scalingObjects != null) {
			myAnnotationMesh.transform.localScale = newScale;
		}
	}

	public Vector3 getMeshScale() {
		return myAnnotationMesh.transform.localScale;
	}

	//rotate Annotation Mesh with given Rotation in world space
	public void rotateMesh(Quaternion rotation) {
		myAnnotationMesh.transform.rotation *= rotation;
	}

	public Quaternion getMeshRotation() {
		return myAnnotationMesh.transform.rotation;
	}

	//Moves Annotation to given Position in WorldSpace
	public void translateMesh(Vector3 newPos) {
		//world Space
		this.transform.position = newPos;

	}

	public void changeAnnotationMeshLayer(String layer) {
		myAnnotationMesh.layer = LayerMask.NameToLayer (layer);
	}

	private void changeAnnotationLabelLayer(String layer) {
		myAnnotationLabel.layer = LayerMask.NameToLayer (layer);
	}

	public bool LabelClicked(PointerEventData data) {
		return AnnotationControl.instance.annotationClicked (data);
	}

	public void AnnotationClicked (BaseEventData eventData) {
		PointerEventData data = eventData as PointerEventData;
		if (AnnotationControl.instance.annotationClicked (data)) {
			myAnnotationMesh.GetComponent<AnnotationMoveVive> ().AnnotationClicked (data, rotateWithVive);
		}
	}

	public void saveAnnotationChanges() {
		AnnotationControl.instance.updatePatientAnnotationList ();
	}

	public void destroyAnnotation() {
		//Destroy Label

		if (myAnnotationLabel != null)
		{
			Destroy(myAnnotationLabel);
		}
	}

	public void ViveMovement(bool active) {
		viveMoveObject.SetActive (!active);
		if(active) {
			makeTransperent (1f);
		} else {
			makeTransperent (0.4f);
		}
	}
}