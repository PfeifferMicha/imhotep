using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using LitJson;
using System;
using System.IO;

// TODO: Move saving and loading to Patient? I think the Patient class should have all info...
// That way any other widget/tool can access the annotations as well, if needed in the future.

//This class represents the annotiation points in a JSON file. It can be stored and loaded with the JSON mapper.
public class AnnotationPointJson
{
    public string Text { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public double RotationW { get; set; }
    public double RotationX { get; set; }
    public double RotationY { get; set; }
    public double RotationZ { get; set; }
    public string Creator { get; set; }
    public DateTime CreationDate { get; set; } //TODO

}

public class AnnotationControl : MonoBehaviour {

    public GameObject annotationPointObj;
    public InputField annotationTextInput;
    public GameObject annotationLabel;
    public Button addAnnotationButton;
    public Button saveButton;
    public Button annotationListButton;


    private State currentState = State.idle;
    private Transform meshNode;
	private Transform meshPositionNode;
    private GameObject currentAnnotatinPoint = null;
    private List<GameObject> annotationPoints = new List<GameObject>();

    private InputDeviceManager idm;

    private static int annotationPointCounter = 0; //Used to create unique id for annoation point

    
	//The states of the annotation control. 
	// idle - The annotations are displayed. The user can press 'Add annoation' to create a new annotation.
	// addAnnotationPressed - The user pressed 'Add annoation' and we are waiting for a click on the mesh to select a point for the annoation
	// annotationPointSelected - The user selected a point on the mesh and we are waiting for the text of the annotation. If the user confirm the text we return to 'idle'.
	private enum State
    {
        idle,
        addAnnotationPressed,
        annotationPointSelected,
        //textEntered
    }

    void OnEnable()
    {
        meshNode = GameObject.Find("MeshViewer").transform; //TODO error if name of MeshViewer is changed
		meshPositionNode = GameObject.Find("MeshViewer/MeshRotationNode/MeshPositionNode").transform;

        // Register event callbacks:
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Loaded, loadAnnotationFromFile);
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, closePatient);
        loadAnnotationFromFile();
    }

    void OnDisable()
    {
        // Unregister myself:
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Loaded, loadAnnotationFromFile);
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, closePatient);
        clearAllPressed();
    }


    // Use this for initialization
    void Start () {
        changeCurrentStateToIdle();

        idm = GameObject.Find("GlobalScript").GetComponent<InputDeviceManager>();
		meshNode = GameObject.Find("MeshViewer").transform; //TODO error if name of MeshViewer is changed
		meshPositionNode = GameObject.Find("MeshViewer/MeshRotationNode/MeshPositionNode").transform;

        if(annotationPointObj == null)
        {
            Debug.LogError("No Annotation Point Object is set in Annotation.cs");
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
        //if (Input.GetMouseButtonDown(0) && currentState == State.addAnnotationPressed)
		InputDevice inputDevice=  idm.currentInputDevice;
        if (inputDevice.getLeftButtonState() == PointerEventData.FramePressState.Pressed && currentState == State.addAnnotationPressed)
        {			
            RaycastHit hit;
            Ray ray = inputDevice.createRay();
            LayerMask onlyMeshViewLayer = 1000000000; // hit only the mesh view layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                // Calculate quaternion which looks along the normal of the hit surface (hopefully "away" from the object).
                GameObject newAnnotationPoint = createAnnotationPoint(Quaternion.LookRotation(hit.normal), hit.point);
                currentAnnotatinPoint = newAnnotationPoint;
                changeCurrentStateToAnnotationPointSelected();
            }
        }

    }

    private GameObject createAnnotationPoint(Quaternion rotation, Vector3 point)
    {
        GameObject newAnnotationPoint = (GameObject)Instantiate(annotationPointObj, point, rotation);
        newAnnotationPoint.transform.localScale *= meshNode.localScale.x; //x,y,z are the same
		newAnnotationPoint.transform.parent = meshPositionNode;

        AnnotationPoint ap = newAnnotationPoint.AddComponent<AnnotationPoint>();
        ap.id = getUniqueAnnotationPointID();
        ap.enabled = false;

        annotationPoints.Add(newAnnotationPoint);
        return newAnnotationPoint;
    }

    private int getUniqueAnnotationPointID()
    {
        int result = annotationPointCounter;
        annotationPointCounter++;
        return result;
    }

	//Called if the user pressed 'Add annoation' button
    public void AddAnnotationPressed()
    {
        if (currentState == State.idle)
        {
            changeCurrentStateToAddAnnotationPressed();
        }
    }

	//Called if the user confirmed the text
    public void SaveAnnotationPoint()
    {
        if (currentState == State.annotationPointSelected && currentAnnotatinPoint != null)
        {
            createAnnotationLabelAndLine(currentAnnotatinPoint, annotationTextInput.text);

            annotationTextInput.text = "";
            currentAnnotatinPoint = null;
            changeCurrentStateToIdle();

            saveAnnotationInFile();
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
		newAnnotationLabel.transform.SetParent(meshPositionNode, true);

        // Since the currentAnnotationPoint faces along the normal of the attached object,
        // we can get an offset direction from its rotation:
        Vector3 offsetDirection = annotationPoint.transform.localRotation * Vector3.forward;
        Vector3 offset = offsetDirection * 100f;

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

	//Called if the user pressed 'Clear all'. Deletes all annotations.
    public void clearAllPressed()
    {
        foreach (GameObject g in annotationPoints)
        {
            if(g != null)
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
            GameObject textObject = newButton.transform.Find("Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();
            AnnotationPoint ap = g.GetComponent<AnnotationPoint>();
            if(ap != null)
            {
                buttonText.text = ap.text;

                // Attach AnnotationPointID to the new button and save the id of the annotation point 
                newButton.AddComponent<AnnotationPointID>().annotationPointID = ap.id;
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

	//Saves all annotations in a file
    public void saveAnnotationInFile()
    {
        if (Patient.getLoadedPatient() == null)
        {
            return;
        }

        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/annotation.json";

        //Create file if it not exists
        if (!File.Exists(path))
        {
            using (StreamWriter outputFile = new StreamWriter(path,true))
            {
                outputFile.Close();
            }
        }

        //Write annotations in file
        using (StreamWriter outputFile = new StreamWriter(path))
        {
            foreach(GameObject ap in annotationPoints)
            {
                AnnotationPointJson apj = new AnnotationPointJson();
                apj.Text = ap.GetComponent<AnnotationPoint>().text;
                apj.PositionX = ap.transform.localPosition.x;
                apj.PositionY = ap.transform.localPosition.y;
                apj.PositionZ = ap.transform.localPosition.z;

                apj.RotationW = ap.transform.localRotation.w;
                apj.RotationX = ap.transform.localRotation.x;
                apj.RotationY = ap.transform.localRotation.y;
                apj.RotationZ = ap.transform.localRotation.z;

                apj.Creator = ap.GetComponent<AnnotationPoint>().creator;
                apj.CreationDate = ap.GetComponent<AnnotationPoint>().creationDate;
                outputFile.WriteLine(JsonMapper.ToJson(apj));
            }
            outputFile.Close();
        }
        return;
    }

    private void loadAnnotationFromFile(object obj = null)
    {
        if (Patient.getLoadedPatient() == null)
        {
            return;
        }

        clearAllPressed();

        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/annotation.json"; //TODO read from meta.json??

        if (!File.Exists(path))
        {
            return;
        }

        List<AnnotationPointJson> apjList = new List<AnnotationPointJson>();


        // Read the file
        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(path);
        while ((line = file.ReadLine()) != null)
        {
            AnnotationPointJson apj = JsonMapper.ToObject<AnnotationPointJson>(line);
            apjList.Add(apj);
        }
        file.Close();

        foreach(AnnotationPointJson apj in apjList)
        {
            Quaternion rotation = new Quaternion((float)apj.RotationX, (float)apj.RotationY, (float)apj.RotationZ, (float)apj.RotationW);
            Vector3 position = new Vector3((float)apj.PositionX, (float)apj.PositionY, (float)apj.PositionZ);

			GameObject annotationPoint = createAnnotationPoint(Quaternion.identity, meshPositionNode.transform.TransformPoint(position));
            annotationPoint.transform.localRotation = rotation;
            createAnnotationLabelAndLine(annotationPoint, apj.Text);
        }

        updateAnnotationList();      

    }

	//Deletes a annotation given in self
    public void deleteOneAnnotation(GameObject self)
    {
        Transform buttonWithText = self.transform.parent;
        AnnotationPointID apID = buttonWithText.GetComponent<AnnotationPointID>();
        if (buttonWithText == null || apID == null)
        {
            return;
        }

        //Delete point and label
        foreach (GameObject g in annotationPoints)
        {
            if (g.GetComponent<AnnotationPoint>().id == apID.annotationPointID)
            {
                //Destroy Label
                GameObject label = g.GetComponent<AnnotationPoint>().annotationLabel;
                if (label != null)
                {
                    Destroy(label);
                }
                //Delete points
                Destroy(g);

                break;
            }
        }

        //Delete object in list
        GameObject objToRemove = null;
        foreach (GameObject g in annotationPoints)
        {
            if (g.GetComponent<AnnotationPoint>().id == apID.annotationPointID)
            {
                objToRemove = g;
                break;
            }
        }
        if (objToRemove != null)
        {
            annotationPoints.Remove(objToRemove);
        }

        updateAnnotationList();

        saveAnnotationInFile();

        changeCurrentStateToIdle();
    }

	//Called if the patient is closed
    public void closePatient(object obj = null)
    {
        clearAllPressed();
    }

}
