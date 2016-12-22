using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnnotationListEntry : MonoBehaviour {


	public Button editButton;
	public Button deleteButton;
	public Text listEntryLabel;

	private GameObject myAnnotation;

	public void setupListEntry (GameObject annotation) {
		myAnnotation = annotation;
		annotation.GetComponent<Annotation> ().myAnnotationListEntry = this.gameObject;
		listEntryLabel.text = annotation.GetComponent<Annotation>().getLabelText();
	}

	public void DestroyAnnotation() {
		//Destroy Label
		myAnnotation.GetComponent<Annotation>().destroyAnnotation();
		Destroy(myAnnotation.gameObject);
		myAnnotation = null;
	}

	public GameObject getAnnotation() {
		return myAnnotation;
	}

	public Color getAnnotationColor() {
		return myAnnotation.GetComponent<Annotation>().getColor();
	}

	public void updateLabel(string newLabel) {
		listEntryLabel.text = newLabel;
	}

	//Called if the user pressed Edit Annotation Button (List Screen)
	public void EditAnnotation() {	
		AnnotationControl.instance.EditAnnotation (this.gameObject);
	}

	//Called if the user pressed Delete Annotation Button (List Screen)
	public void DeleteAnnotation() {	
		AnnotationControl.instance.DeleteAnnotation (this.gameObject);
	}

	public void changeAnnotationColor(Color newColor) {
		myAnnotation.GetComponent<Annotation>().changeColor (newColor);
	}

	public void updateAnnotationposition(Quaternion rotation, Vector3 position) {
		myAnnotation.GetComponent<Annotation>().updatePosition (rotation, position);
	}

	public Vector2 getListPos() {
		return this.gameObject.GetComponent<RectTransform> ().anchoredPosition;
	}

	public void replaceMyAnnotationMesh (GameObject newAnnotationGroup) {
		newAnnotationGroup.GetComponent<Annotation> ().transferAnnotationSettings (myAnnotation);
		DestroyAnnotation ();
		setupListEntry (newAnnotationGroup);
		AnnotationControl.instance.updateListInLabelPositioner ();
	}

	public void setAnnotationMovementActive(bool active) {
		myAnnotation.GetComponent<Annotation> ().setMovementMeshsActive (active);
	}

	public AnnotationControl.AnnotationType getMyAnnotationType() {
		return myAnnotation.GetComponent<Annotation> ().myType;
	}

	public void makeAnnotationTransparent(float alpha) {
		myAnnotation.GetComponent<Annotation> ().makeTransperent (alpha);
	}

	public void resetAnnotationTransparency() {
		myAnnotation.GetComponent<Annotation> ().setDefaultTransparency ();
	}

	public void setAnnotationLayer(string layer) {
		myAnnotation.GetComponent<Annotation> ().changeAnnotationMeshLayer (layer);
	}

	public void setMyAnnotationActive(bool active) {
		myAnnotation.SetActive (active);
	}
}
