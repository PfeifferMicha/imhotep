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
    public GameObject annotationLabel;
    //public Button addAnnotationButton;
	public GameObject annotationListEntry;

	public GameObject newAnnotationSetupScreen;
	public GameObject instructionText;
	public InputField annotationTextInput;
	public Button saveButton;

	public GameObject listScreen;
	public GameObject meshNode;
	public GameObject meshPositionNode;

    private State currentState = State.idle;
    private GameObject currentAnnotatinPoint = null;
    private List<GameObject> annotationPoints = new List<GameObject>();

    private InputDeviceManager idm;

    private static int annotationPointCounter = 0; //Used to create unique id for annoation point

	private ClickNotifier clickNotifier;
    
	//The states of the annotation control. 
	// idle - The annotations are displayed. The user can press 'Add annoation' to create a new annotation.
	// addAnnotationPressed - The user pressed 'Add annoation' and we are waiting for a click on the mesh to select a point for the annoation
	// annotationPointSelected - The user selected a point on the mesh and we are waiting for the text of the annotation. If the user confirm the text we return to 'idle'.
	private enum State
    {
        idle,
        generatingAnnotationPoint,
        annotationPointSelected,
        //textEntered
    }

    void OnEnable()
    {
        // Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_FinishedLoading, loadAnnotationFromFile);
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, closePatient);
        loadAnnotationFromFile();

		newAnnotationSetupScreen.SetActive (false);
		listScreen.SetActive (false);

		annotationLabel.SetActive (false);
		annotationPointObj.SetActive (false);

		clickNotifier = meshPositionNode.AddComponent<ClickNotifier> ();
		clickNotifier.notificationEvent = OnMeshClicked;
    }

    void OnDisable()
    {
        // Unregister myself:
		PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_FinishedLoading, loadAnnotationFromFile);
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, closePatient);
		clearAll();
		Destroy (clickNotifier);
    }


    // Use this for initialization
    void Start () {
        changeCurrentStateToIdle();

        idm = GameObject.Find("GlobalScript").GetComponent<InputDeviceManager>();

        if(annotationPointObj == null)
        {
            Debug.LogError("No Annotation Point Object is set in Annotation.cs");
        }

        annotationListEntry.gameObject.SetActive(false);
    }

    private GameObject createAnnotationPoint(Quaternion rotation, Vector3 point)
	{
		point = meshPositionNode.transform.InverseTransformPoint (point);
        GameObject newAnnotationPoint = (GameObject)Instantiate(annotationPointObj, point, rotation);

        //newAnnotationPoint.transform.localScale *= meshNode.transform.localScale.x; //x,y,z are the same
		newAnnotationPoint.transform.localScale = new Vector3( 5, 5, 5 );
		newAnnotationPoint.transform.SetParent( meshPositionNode.transform, false );

        AnnotationPoint ap = newAnnotationPoint.AddComponent<AnnotationPoint>();
        ap.id = getUniqueAnnotationPointID();
        ap.enabled = false;

        annotationPoints.Add(newAnnotationPoint);
		newAnnotationPoint.SetActive (true);
        return newAnnotationPoint;
    }

    private int getUniqueAnnotationPointID()
    {
        int result = annotationPointCounter;
        annotationPointCounter++;
        return result;
    }

	//Called if the user pressed 'add annoation' button
    public void AddAnnotationPressed()
    {
        if (currentState == State.idle)
		{
			listScreen.SetActive (false);
            changeCurrentStateToAddAnnotationPressed();
        }
    }

	//! Switches to list of annotations:
	public void ShowAnnotationsList()
	{
		newAnnotationSetupScreen.SetActive (false);
		listScreen.SetActive (true);
	}

	void OnMeshClicked( PointerEventData eventData )
	{
		Debug.Log ("Clicked: " + eventData.pointerPressRaycast.worldPosition);
		if (currentState == State.generatingAnnotationPoint) {

			currentAnnotatinPoint = createAnnotationPoint(
				Quaternion.LookRotation( eventData.pointerPressRaycast.worldNormal ),
				eventData.pointerPressRaycast.worldPosition);
			//createAnnotationLabelAndLine(currentAnnotatinPoint, "");
			currentState = State.annotationPointSelected;
			annotationTextInput.gameObject.SetActive (true);
			saveButton.gameObject.SetActive (true);
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
			annotationTextInput.gameObject.SetActive (false);
			saveButton.gameObject.SetActive (false);

            saveAnnotationInFile();
        }
    }

    private void createAnnotationLabelAndLine(GameObject annotationPoint, string textLabel)
    {
        AnnotationPoint ap = annotationPoint.GetComponent<AnnotationPoint>();
        ap.text = textLabel;
        ap.enabled = true;

        //Create Label
		GameObject newAnnotationLabel = (GameObject)Instantiate(annotationLabel, Vector3.zero, annotationPoint.transform.localRotation);
		newAnnotationLabel.transform.localScale = new Vector3 (0.05f, 0.05f, 0.05f);	//*= meshNode.transform.localScale.x; //x,y,z are the same
		newAnnotationLabel.transform.SetParent(annotationPoint.transform, false);
		newAnnotationLabel.SetActive (true);

        // Since the currentAnnotationPoint faces along the normal of the attached object,
        // we can get an offset direction from its rotation:
		newAnnotationLabel.transform.localPosition = new Vector3(0f,0f,15f);
        ap.annotationLabel = newAnnotationLabel;
        // Change label text:
        GameObject textObject = newAnnotationLabel.transform.Find("Background/Text").gameObject;
        Text labelText = textObject.GetComponent<Text>();
		labelText.text = ap.text;

        //Create line form point to label
        annotationPoint.GetComponent<LineRenderer>().SetPosition(0, annotationPoint.transform.position);
        annotationPoint.GetComponent<LineRenderer>().SetPosition(1, newAnnotationLabel.transform.position);
    }

	// Deletes all annotations.
	public void clearAll()
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
        for(int i = 0; i < annotationListEntry.transform.parent.childCount; i++)
        {
            if(i != 0) //TODO !=0
            {
                Destroy(annotationListEntry.transform.parent.GetChild(i).gameObject);
            }
        }

        foreach (GameObject g in annotationPoints)
        {            
            // Create a new instance of the list button:
            GameObject newEntry = Instantiate(annotationListEntry).gameObject;
			newEntry.SetActive(true);

            // Attach the new button to the list:
			newEntry.transform.SetParent(annotationListEntry.transform.parent, false);


            // Change button text to name of tool:
			//GameObject textObject = newEntry.transform.Find("Text").gameObject;
			Text buttonText = newEntry.GetComponent<Text>();
            AnnotationPoint ap = g.GetComponent<AnnotationPoint>();
            if(ap != null)
            {
                buttonText.text = ap.text;

                // Attach AnnotationPointID to the new button and save the id of the annotation point 
				newEntry.AddComponent<AnnotationPointID>().annotationPointID = ap.id;
            }
            
        }
    }

    private void changeCurrentStateToIdle()
    {
        /*addAnnotationButton.enabled = true;
        Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if(t != null)
        {
            t.text = "Add Annotation"; //Change text on button
        }*/

		newAnnotationSetupScreen.SetActive (false);
		listScreen.SetActive (false);

        currentState = State.idle;

        updateAnnotationList();
    }

    private void changeCurrentStateToAddAnnotationPressed()
    {
        /*addAnnotationButton.enabled = false;
        Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if (t != null)
        {
            t.text = "Select point";
        }*/

		newAnnotationSetupScreen.SetActive (true);
		listScreen.SetActive (false);
        annotationTextInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
		instructionText.GetComponent<Text> ().text = "Click organ to create new annotation...";

        //TODO change color of button

        currentState = State.generatingAnnotationPoint;
    }

    private void changeCurrentStateToAnnotationPointSelected()
    {
        /*Text t = addAnnotationButton.GetComponentInChildren<Text>();
        if (t != null)
        {
            t.text = "Enter text";
        }*/

		instructionText.GetComponent<Text> ().text = "Enter text for new label:";

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

        clearAll();

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

			GameObject annotationPoint = createAnnotationPoint(Quaternion.identity, position);
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
		clearAll();
    }

}
