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
	public Quaternion rotation;
    [DefaultValue(-1)]
	public int id;



    // Use this for initialization
    void Start () {
        creationDate = DateTime.Now;
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

		annotationLabel = (GameObject)Instantiate(annotationLabelMaster, Vector3.zero, this.transform.localRotation);
		annotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		annotationLabel.transform.SetParent(this.transform, false);
		annotationLabel.SetActive (true);

		// Since the currentAnnotationPoint faces along the normal of the attached object,
		// we can get an offset direction from its rotation:
		annotationLabel.transform.localPosition = new Vector3(0f,0f,15f);

		//Create line form point to label
		this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
		this.GetComponent<LineRenderer>().SetPosition(1, this.transform.position);

		Text labelText = annotationLabel.transform.Find("Background/Text").gameObject.GetComponent<Text>();
		labelText.text = text;
	}

	//Updates the Label Text, if no Label exists  create one
	public void SetLabel(String newLabel) {

		if(annotationLabel == null) {
			//create AnnotationLabel
			Debug.LogError("Annotation has no AnnotationLabel to edit");
			return;
		}
		// Change label text:
		Text labelText = annotationLabel.transform.Find("Background/Text").gameObject.GetComponent<Text>();
		labelText.text = newLabel;
		text = newLabel;
	}
}
