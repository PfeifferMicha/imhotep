using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.UI;

public class Annotation : MonoBehaviour {

    [DefaultValue("")]
    public string text { get; set; }
    [DefaultValue("")]
    public string creator { get; set; }
	public DateTime creationDate { get; set; }
    private GameObject annotationLabel { get; }
    public Quaternion rotation { get; set; }
    [DefaultValue(-1)]
    public int id { get; set; }

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
	private void CreateLabel() {
		//Create Label
		annotationLabel = (GameObject)Instantiate(annotationLabel, Vector3.zero, this.transform.localRotation);
		annotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		annotationLabel.transform.SetParent(this.transform, false);
		annotationLabel.SetActive (true);

		// Since the currentAnnotationPoint faces along the normal of the attached object,
		// we can get an offset direction from its rotation:
		annotationLabel.transform.localPosition = new Vector3(0f,0f,15f);

		//Create line form point to label
		this.GetComponent<LineRenderer>().SetPosition(0, this.transform.position);
		this.GetComponent<LineRenderer>().SetPosition(1, this.transform.position);
	}

	//Updates the Label Text, if no Label exists  create one
	public void SetLabel(String newLabel) {

		if(annotationLabel == null) {
			//create AnnotationLabel
			CreateLabel();
		}
		// Change label text:
		GameObject textObject = annotationLabel.transform.Find("Background/Text").gameObject;
		Text labelText = textObject.GetComponent<Text>();
		labelText.text = newLabel;
	}
}
