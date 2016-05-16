using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using LitJson;
using System;
using System.IO;

using BlenderMeshReader;

public class AnnotationPointJson
{
    public string Text { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public double NormalX { get; set; }
    public double NormalY { get; set; }
    public double NormalZ { get; set; }
    public string Creator { get; set; }
    public DateTime CreationDate { get; set; }

}

public class AnnotationControl : MonoBehaviour {

    public GameObject annotationPointObj;
    public InputField annotationTextInput;
    public GameObject annotationLabel;
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


    void OnEnable()
    {
        meshNode = GameObject.Find("MeshViewer").GetComponent<Transform>(); //TODO error if name of MeshViewer is changed

        // Register event callbacks:
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Loaded, loadAnnotation);
        loadAnnotation();
    }

    void OnDisable()
    {
        // Unregister myself:
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Loaded, loadAnnotation);
        clearAllPressed();
    }


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
        //If user pressed "Add Annotation" and clicked 
		if (Input.GetMouseButtonDown(0) && currentState == State.addAnnotationPressed)
        {
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, mMouse.transform.position - Camera.main.transform.position);
            LayerMask onlyMeshViewLayer = 1000000000; // hit only the mesh view layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                // Calculate quaternion which looks along the normal of the hit surface (hopefully "away" from the object).
                GameObject newAnnotationPoint = createAnnotationPoint(hit.normal, hit.point, true);
                currentAnnotatinPoint = newAnnotationPoint;
                changeCurrentStateToAnnotationPointSelected();
            }
        }

    }

    private GameObject createAnnotationPoint(Vector3 normal, Vector3 point, bool isPointInWorldspace)
    {
        Quaternion lookDirection = Quaternion.LookRotation(normal);
        Vector3 pointInWorldspace = point;
        if (!isPointInWorldspace)
        {
            pointInWorldspace = meshNode.transform.TransformPoint(point);
        }
        GameObject newAnnotationPoint = (GameObject)Instantiate(annotationPointObj, pointInWorldspace, lookDirection);
        newAnnotationPoint.transform.localScale *= meshNode.localScale.x; //x,y,z are the same
        newAnnotationPoint.transform.parent = meshNode;

        AnnotationPoint ap = newAnnotationPoint.AddComponent<AnnotationPoint>();
        ap.normal = normal;
        ap.enabled = false;

        annotationPoints.Add(newAnnotationPoint);
        return newAnnotationPoint;
    }

    public void AddAnnotationPressed()
    {
        if (currentState == State.idle)
        {
            changeCurrentStateToAddAnnotationPressed();
        }
    }

    public void SaveAnnotationPoint()
    {
        if (currentState == State.annotationPointSelected && currentAnnotatinPoint != null)
        {
            createAnnotationLabelAndLine(currentAnnotatinPoint, annotationTextInput.text);

            annotationTextInput.text = "";
            currentAnnotatinPoint = null;
            changeCurrentStateToIdle();
        }
    }

    private void createAnnotationLabelAndLine(GameObject annotationPoint, string textLabel)
    {
        AnnotationPoint ap = annotationPoint.GetComponent<AnnotationPoint>();
        ap.text = textLabel;
        ap.enabled = true;

        //Create Label
        GameObject newAnnotationLabel = (GameObject)Instantiate(annotationLabel, annotationPoint.transform.position, annotationPoint.transform.localRotation);
        newAnnotationLabel.transform.localScale *= meshNode.localScale.x; //x,y,z are the same
        newAnnotationLabel.transform.SetParent(meshNode, true);

        // Since the currentAnnotationPoint faces along the normal of the attached object,
        // we can get an offset direction from its rotation:
        Vector3 offsetDirection = annotationPoint.transform.localRotation * Vector3.forward;
        Vector3 offset = offsetDirection / 1.5f;

        //Vector3 offset = new Vector3(90,20,0);
        newAnnotationLabel.transform.localPosition += offset;
        ap.annotationLabel = newAnnotationLabel;
        // Change label text:
        GameObject textObject = newAnnotationLabel.transform.Find("Button/OverlayImage/Text").gameObject;
        Text buttonText = textObject.GetComponent<Text>();
        buttonText.text = ap.text;

        //Create line form point to label
        annotationPoint.GetComponent<LineRenderer>().SetPosition(0, annotationPoint.transform.position);
        annotationPoint.GetComponent<LineRenderer>().SetPosition(1, newAnnotationLabel.transform.position);
    }

    public void clearAllPressed()
    {
        foreach (GameObject g in annotationPoints)
        {
            //Destroy Label
            GameObject label = g.GetComponent<AnnotationPoint>().annotationLabel;
            if (label != null)
            {
                Destroy(label);
            }
            //Delete points
            Destroy(g);
        }
        //Delete list
        annotationPoints = new List<GameObject>();

        changeCurrentStateToIdle();
    }

    private void updateAnnotationList()
    {
        //Destroy all object up to one button
        for(int i = 0; i < annotationListButton.transform.parent.childCount; i++)
        {
            if(i != 0) //TODO !=0
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

    public void saveAnnotation()
    {
        if (Patient.getLoadedPatient() == null)
        {
            return;
        }

        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/annotation.json";

        using (StreamWriter outputFile = new StreamWriter(currentPatient.path + @"\annotation.json"))
        {
            foreach(GameObject ap in annotationPoints)
            {
                AnnotationPointJson apj = new AnnotationPointJson();
                apj.Text = ap.GetComponent<AnnotationPoint>().text;
                apj.PositionX = ap.transform.localPosition.x;
                apj.PositionY = ap.transform.localPosition.y;
                apj.PositionZ = ap.transform.localPosition.z;

                apj.NormalX = ap.GetComponent<AnnotationPoint>().normal.x;
                apj.NormalY = ap.GetComponent<AnnotationPoint>().normal.y;
                apj.NormalZ = ap.GetComponent<AnnotationPoint>().normal.z;

                apj.Creator = ap.GetComponent<AnnotationPoint>().creator;
                apj.CreationDate = ap.GetComponent<AnnotationPoint>().creationDate;
                outputFile.WriteLine(JsonMapper.ToJson(apj));
            }
            outputFile.Close();
        }


        return;
    }

    private void loadAnnotation(object obj = null)
    {
        if (Patient.getLoadedPatient() == null)
        {
            return;
        }

        clearAllPressed();

        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/annotation.json";

        List<AnnotationPointJson> apjList = new List<AnnotationPointJson>();

        string line;

        // Read the file
        System.IO.StreamReader file = new System.IO.StreamReader(path);
        while ((line = file.ReadLine()) != null)
        {
            AnnotationPointJson apj = JsonMapper.ToObject<AnnotationPointJson>(line);
            apjList.Add(apj);
        }
        file.Close();

        foreach(AnnotationPointJson apj in apjList)
        {
            Vector3 normal = new Vector3((float)apj.NormalX, (float)apj.NormalY, (float)apj.NormalZ);
            Vector3 position = new Vector3((float)apj.PositionX, (float)apj.PositionY, (float)apj.PositionZ);
            GameObject annotationPoint = createAnnotationPoint(normal, position, false);
            createAnnotationLabelAndLine(annotationPoint, apj.Text);
        }

        updateAnnotationList();

      

    }


}
