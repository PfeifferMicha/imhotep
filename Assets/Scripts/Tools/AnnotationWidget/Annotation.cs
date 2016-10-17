using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.UI;

public class Annotation : MonoBehaviour {

    [DefaultValue("")]
	public string text;
    [DefaultValue("")]
	public string creator;
	public DateTime creationDate;
	public GameObject annotationLabel;
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

        if (annotationLabel != null)
        {
            this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
            this.GetComponent<LineRenderer>().SetPosition(1, this.annotationLabel.transform.position);
        }
        
    }


	//Used to create new Label
	public void CreateLabel(GameObject annotationLabelMaster) {
		//Create Label

		annotationLabel = (GameObject)Instantiate(annotationLabelMaster, new Vector3(0f,0f,15f), this.transform.localRotation);
		annotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		annotationLabel.transform.SetParent(this.transform, false);
		annotationLabel.SetActive (true);

		//Create line form point to label
		this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
		this.GetComponent<LineRenderer>().SetPosition(1, this.annotationLabel.transform.position);
		SetLabel (text);
	}


	public void destroyAnnotation() {
		Destroy (annotationLabel);
		annotationLabel = null;
		Destroy (this.gameObject);
	}

	//Updates the Label Text, if no Label exists  create one
	public void SetLabel(String newLabel) {

		if(annotationLabel == null) {
			//create AnnotationLabel
			Debug.LogError("Annotation has no AnnotationLabel to edit");
			return;
		}
		// Change label text:
		text = newLabel;
		annotationLabel.GetComponentInChildren<Text> ().text = text;
	}

	public void AbortChanges(GameObject oldAnnotation) {
		SetLabel (oldAnnotation.GetComponent<Annotation> ().text);
		changeColor (oldAnnotation.GetComponent<Annotation> ().myColor);
	}

	public void changeColor(Color newColor) {
		myColor = newColor;
		this.GetComponent<Renderer> ().material.color = newColor;
	}
}
