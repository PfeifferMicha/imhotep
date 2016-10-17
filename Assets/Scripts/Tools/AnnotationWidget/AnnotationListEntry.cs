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
		listEntryLabel.GetComponent<Text> ().text = myAnnotation.GetComponent<Annotation> ().getLabel();
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



}
