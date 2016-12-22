using UnityEngine;
using System.Collections;

public class CanvasMovement : MonoBehaviour {

    // Use this for initialization

    public Vector3 newPos { get; set; }
    private Vector3 velocity = Vector3.one;
    
    private GameObject meshNode;
    private GameObject meshPositionNode;
    private GameObject meshViewerBase;
    private Vector3[] pointsOnLabel = new Vector3[4];
    private float[] distances = new float[4];
    public GameObject annotationPoint { get; set; }

    public float labelScale = 0.04f; // größe der labels..


    void Start ()
    {
        meshViewerBase = GameObject.Find("MeshViewerBase");
        meshNode = GameObject.Find("MeshViewerBase/MeshViewerScale");
        meshPositionNode = GameObject.Find("MeshViewerBase/MeshViewerScale/MeshRotationNode/MeshPositionNode");
        labelScale = 0.04f;
    }
	
	// Update is called once per frame
	void Update () {

        adaptLabelToZooming();

        this.transform.position = Vector3.SmoothDamp(this.transform.position, newPos, ref velocity,  2f);

        //this.transform.position = newPos;
        if(annotationPoint != null) {
            this.GetComponent<LineRenderer>().SetPosition(0, annotationPoint.transform.position);
        }



        float width = this.GetComponent<RectTransform>().rect.width * this.transform.parent.localScale.x * this.GetComponent<RectTransform>().localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
        float height = this.GetComponent<RectTransform>().rect.height * this.transform.parent.localScale.x * this.GetComponent<RectTransform>().localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;

        pointsOnLabel[0] = this.transform.position + this.transform.up * (height / 2) + this.transform.right * (width / 2);
        pointsOnLabel[1] = this.transform.position + this.transform.up * (height / 2) - this.transform.right * (width / 2);
        pointsOnLabel[2] = this.transform.position - this.transform.up * (height / 2) + this.transform.right * (width / 2);
        pointsOnLabel[3] = this.transform.position - this.transform.up * (height / 2) - this.transform.right * (width / 2);

        

        for (int i = 0; i < distances.Length; i++)
        {

            if(annotationPoint != null)
            {
                distances[i] = (pointsOnLabel[i] - annotationPoint.transform.position).magnitude;

            }
        }


        for (int i = 0; i < distances.Length; i++)
        {
            bool smallest = false;

            for (int j = i; j < distances.Length; j++)
            {

                if (distances[i] <= distances[j])
                {
                    smallest = true;
                }
                else
                {
                    smallest = false;
                }

            }

            if (smallest == true)
            {

                this.GetComponent<LineRenderer>().SetPosition(1, pointsOnLabel[i]);
                break;
            }

        }        
        
        this.GetComponent<LineRenderer>().enabled = true;

    }

    public void adaptLabelToZooming() // die größe der Labels sollten unabhängig vom zoomen sein..
    {                                // war davor am label angeheftet
        
        this.GetComponent<RectTransform>().localScale = new Vector3(labelScale / meshNode.transform.localScale.x, labelScale / meshNode.transform.localScale.x, labelScale / meshNode.transform.localScale.x);
        

    }
}
