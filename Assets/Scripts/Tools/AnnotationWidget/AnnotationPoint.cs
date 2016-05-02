using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;

public class AnnotationPoint : MonoBehaviour {

    [DefaultValue("")]
    public string text { get; set; }
    [DefaultValue("")]
    public string creator { get; set; }
    public DateTime creationDate { get; set; }
    public GameObject annotationLabel { get; set; }

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
}
