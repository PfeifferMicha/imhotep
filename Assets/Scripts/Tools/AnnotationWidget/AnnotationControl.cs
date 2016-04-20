using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnnotationControl : MonoBehaviour {

    public GameObject annotationPointObj;
    public InputField annotationTextInput;
    public Button addAnnotationButton;
    public Button saveButton;
    public Button annotationListButton;

    private enum State
    {
        idle,
        addAnnotationPressed,
        annotationPointSelected,
        //textEntered
    }

    private State currentState = State.idle;
    private Transform meshNode;
    private Mouse3DMovement mMouse;
    private GameObject currentAnnotatinPoint = null;
    private List<GameObject> annotationPoints = new List<GameObject>();


    // Use this for initialization
    void Start () {
        changeCurrentStateToIdle();

        mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> (); //TODO error if name of Mouse3D is changed
        meshNode = GameObject.Find("MeshViewer").GetComponent<Transform>(); //TODO error if name of MeshViewer is changed

        if(annotationPointObj == null)
        {
            Debug.LogError("No Annotation Point Object is set in Annotation.cs");
        }
        if (mMouse == null)
        {
            Debug.LogError("No Mouse3D found in Annotation.cs");
        }
        if (meshNode == null)
        {
            Debug.LogError("No Mesh Node found in Annotation.cs");
        }

        annotationListButton.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) && currentState == State.addAnnotationPressed)
        {
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, mMouse.transform.position - Camera.main.transform.position);
            LayerMask onlyMeshViewLayer = 1000000000; // hit only the mesh view layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                GameObject newAnnotationPoint = (GameObject)Instantiate(annotationPointObj, hit.point, Quaternion.identity);
                newAnnotationPoint.transform.localScale *= meshNode.localScale.x; //x,y,z are the same
                newAnnotationPoint.transform.parent = meshNode;
                annotationPoints.Add(newAnnotationPoint);
                currentAnnotatinPoint = newAnnotationPoint;

                changeCurrentStateToAnnotationPointSelected();
            }
        }

    }

    public void AddAnnotationPressed()
    {
        if(currentState == State.idle)
        {
            changeCurrentStateToAddAnnotationPressed();
        }
    }

    public void SaveAnnotationPoint()
    {
        if (currentState == State.annotationPointSelected && currentAnnotatinPoint != null)
        {
            AnnotationPoint ap = currentAnnotatinPoint.AddComponent<AnnotationPoint>();
            ap.text = annotationTextInput.text;
            annotationTextInput.text = "";
            currentAnnotatinPoint = null;
            changeCurrentStateToIdle();
        }
    }

    public void clearAllPressed()
    {
        foreach(GameObject g in annotationPoints)
        {
            Destroy(g);
        }
        annotationPoints = new List<GameObject>();

        changeCurrentStateToIdle();
    }

    private void updateAnnotationList()
    {
        //Destroy all object up to one button
        for(int i = 0; i < annotationListButton.transform.parent.childCount; i++)
        {
            if(i != 0) //TODO
            {
                Destroy(annotationListButton.transform.parent.GetChild(i).gameObject);
            }
        }

        foreach (GameObject g in annotationPoints)
        {            
            // Create a new instance of the list button:
            GameObject newButton = Instantiate(annotationListButton).gameObject;
            newButton.SetActive(true);

            // Attach the new button to the list:
            newButton.transform.SetParent(annotationListButton.transform.parent, false);

            // Change button text to name of tool:
            GameObject textObject = newButton.transform.Find("OverlayImage/Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();

            AnnotationPoint ap = g.GetComponent<AnnotationPoint>();
            if(ap != null)
            {
                buttonText.text = ap.text;
            }
            
        }
    }

    private void changeCurrentStateToIdle()
    {
        addAnnotationButton.enabled = true;
        Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if(t != null)
        {
            t.text = "Add Annotation"; //Change text on button
        }

        annotationTextInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);

        currentState = State.idle;

        updateAnnotationList();
    }

    private void changeCurrentStateToAddAnnotationPressed()
    {
        addAnnotationButton.enabled = false;
        Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if (t != null)
        {
            t.text = "Select point";
        }

        annotationTextInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);

        //TODO change color of button

        currentState = State.addAnnotationPressed;
    }

    private void changeCurrentStateToAnnotationPointSelected()
    {
        Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if (t != null)
        {
            t.text = "Enter text";
        }

        annotationTextInput.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);

        currentState = State.annotationPointSelected;
    }


}
