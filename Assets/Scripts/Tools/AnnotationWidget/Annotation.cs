using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.UI;

public class Annotation : MonoBehaviour {

    [DefaultValue("")]
	public string creator;
	public DateTime creationDate;
	public GameObject myAnnotationLabel;
	public GameObject myAnnotationListEntry;
	public Color myColor;


    // Use this for initialization
    void Start () {
        creationDate = DateTime.Now;
		if(myColor == null) {
			changeColor (Color.black);	
		}

    }

    // Update is called once per frame
	void Update () {
        //Update lines from annotation point to label
        //because lines are no game objects

        if (myAnnotationLabel != null)
        {
            this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
            this.GetComponent<LineRenderer>().SetPosition(1, this.myAnnotationLabel.transform.position);
        }
        
    }


	//Used to create new Label
	public void CreateLabel(GameObject annotationLabelMaster) {
		//Create Label

		myAnnotationLabel = (GameObject)Instantiate(annotationLabelMaster, new Vector3(0f,0f,15f), this.transform.localRotation);
		myAnnotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		myAnnotationLabel.transform.SetParent(this.transform, false);
		myAnnotationLabel.SetActive (true);

		//Create line form point to label
		this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
		this.GetComponent<LineRenderer>().SetPosition(1, this.myAnnotationLabel.transform.position);
	}

	//Destroys Annotation and Label
	public void destroyAnnotation() {
		Destroy (myAnnotationLabel);
		myAnnotationLabel = null;
		Destroy (this.gameObject);
	}

	//Updates the Label Text, if no Label exists  create one
	public void SetLabel(String newLabel) {

		if(myAnnotationLabel == null) {
			//create AnnotationLabel
			Debug.LogError("Annotation has no AnnotationLabel to edit");
			return;
		}
		// Change label text:
		myAnnotationLabel.GetComponent<AnnotationLabel> ().setLabel (newLabel);
	}

	//used to Abort last Changes on Annotation
	public void AbortChanges(GameObject oldAnnotation) {
		SetLabel (oldAnnotation.GetComponent<Annotation> ().myAnnotationLabel.GetComponent<AnnotationLabel>().text);
		changeColor (oldAnnotation.GetComponent<Annotation> ().myColor);
	}

	//used to change color of Annotation
	public void changeColor(Color newColor) {
		myColor = newColor;
		this.GetComponent<Renderer> ().material.color = newColor;
	}
		
	//to get Annotation Label text
	public String getLabel() {
		return myAnnotationLabel.GetComponent<AnnotationLabel> ().text;
	}

	//Used to save Label Changes
	public void saveChanges() {
		myAnnotationListEntry.GetComponent<AnnotationListEntry> ().updateLabel (getLabel ());
	}
}
