using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnnotationListEntry : MonoBehaviour {


	public Button editButton;
	public Button deleteButton;
	public GameObject listEntryLabel;

	private GameObject myAnnotation;

	public void setupListEntry (GameObject annotation) {
		myAnnotation = annotation;
		annotation.GetComponent<Annotation> ().myAnnotationListEntry = this.gameObject;
		listEntryLabel.GetComponent<Text> ().text = myAnnotation.GetComponent<Annotation> ().getLabelText();
	}

	public void destroyAnnotation() {
		//Destroy Label
		GameObject label = myAnnotation.GetComponent<Annotation>().myAnnotationLabel;
		if (label != null)
		{
			Destroy(label);
		}
		//Delete points
		Destroy(myAnnotation);
		myAnnotation = null;
	}

	public GameObject getAnnotation() {
		return myAnnotation;
	}

	//returns a clone of current Annotation
	public GameObject duplicateAnnotation() {
		Quaternion rotation = myAnnotation.transform.localRotation;
		Vector3 position = myAnnotation.transform.localPosition;
		GameObject clone = (GameObject)Instantiate(myAnnotation, position, rotation);
		clone.transform.SetParent(myAnnotation.transform.parent, false );
		clone.SetActive (true);
		clone.GetComponent<Annotation> ().destroyLabel ();
		myAnnotation.GetComponent<Annotation> ().makeOpaque ();
		clone.GetComponent<Annotation> ().makeTransperent ();
		return clone;
	}

	//Replaces currentAnnotation with the one given and destroys old one
	public void replaceAnnotation(GameObject newAnnotation) {
		newAnnotation.GetComponent<Annotation> ().CreateLabel (myAnnotation.GetComponent<Annotation> ().getLabel());
		myAnnotation.GetComponent<Annotation> ().destroyAnnotation ();
		newAnnotation.GetComponent<Annotation> ().makeOpaque ();
		setupListEntry (newAnnotation);
	}

	public void updateLabel(string newLabel) {
		listEntryLabel.GetComponent<Text> ().text = newLabel;
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
		myAnnotation.GetComponent<Annotation> ().changeColor (newColor);
	}



}
