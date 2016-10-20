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
		annotation.GetComponent<Annotation>().myAnnotationListEntry = this.gameObject;
		listEntryLabel.text = annotation.GetComponent<Annotation>().getLabelText();
	}

	public void destroyAnnotation() {
		//Destroy Label
		GameObject label = myAnnotation.GetComponent<Annotation>().getLabel();
		if (label != null)
		{
			Destroy(label);
		}
		//Delete points
		Destroy(myAnnotation.gameObject);
		myAnnotation = null;
	}

	public GameObject getAnnotation() {
		return myAnnotation;
	}

	public void updateLabel(string newLabel) {
		listEntryLabel.text = newLabel;
	}

	//Called if the user pressed Edit Annotation Button (List Screen)
	public void EditAnnotation() {	
		this.GetComponentInParent<AnnotationControl> ().EditAnnotation (this.gameObject);
	}

	//Called if the user pressed Delete Annotation Button (List Screen)
	public void DeleteAnnotation() {	
		this.GetComponentInParent<AnnotationControl> ().DeleteAnnotation (this.gameObject);
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
}
