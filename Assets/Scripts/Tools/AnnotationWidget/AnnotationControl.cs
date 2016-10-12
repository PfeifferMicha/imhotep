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
using System.Linq;


public class AnnotationJson
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




	//Screens
	public GameObject listScreen;
	public GameObject AddEditScreen;

	//Add Annotation Screen Things
	public GameObject instructionText;
	public InputField annotationTextInput;
	public Button saveButton;

	public GameObject meshNode;
	public GameObject meshPositionNode;

	//States
	private ActiveScreen currentActiveScreen = ActiveScreen.none;

	private GameObject currentAnnotation = null;
	private List<GameObject> annotationList = new List<GameObject>();

    private InputDeviceManager idm;

	private static int annotationCounter = 0; //Used to create unique id for annoation point

	private ClickNotifier clickNotifier;

	//The State of Annotation Control, which scrren is active
	// none , no screen is active
	// add , add screen is active
	// list, list screen is active
	private enum ActiveScreen {
		none,
		add,
		list
	}

    void OnEnable()
    {
        // Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_FinishedLoading, loadAnnotationFromFile);
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, closePatient);
        loadAnnotationFromFile();

		AddEditScreen.SetActive (false);
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

        idm = GameObject.Find("GlobalScript").GetComponent<InputDeviceManager>();

        if(annotationPointObj == null)
        {
            Debug.LogError("No Annotation Point Object is set in Annotation.cs");
        }

        annotationListEntry.gameObject.SetActive(false);

    }

	//called by  "On Value Changed Event" , to update Label of current Annotation
	public void InputChanged(string arg0){
		if(currentAnnotation != null) {
			currentAnnotation.GetComponent<Annotation> ().SetLabel (arg0);
		}
	}

	//Used to Create Annotation
	private GameObject createAnnotation(Quaternion rotation, Vector3 point) {
		point = meshPositionNode.transform.InverseTransformPoint (point);
        GameObject newAnnotation = (GameObject)Instantiate(annotationPointObj, point, rotation);

        //newAnnotationPoint.transform.localScale *= meshNode.transform.localScale.x; //x,y,z are the same
		newAnnotation.transform.localScale = new Vector3( 5, 5, 5 );
		newAnnotation.transform.SetParent( meshPositionNode.transform, false );

        Annotation ap = newAnnotation.AddComponent<Annotation>();
        ap.id = getUniqueAnnotationPointID();
        ap.enabled = false;

        annotationList.Add(newAnnotation);
		newAnnotation.SetActive (true);

		//Create Label for annotation
		newAnnotation.GetComponent<Annotation> ().CreateLabel (annotationLabel);

		return newAnnotation;
    }

	//calculates an unique Annotation ID
	private int getUniqueAnnotationPointID()
    {
        int result = annotationCounter;
        annotationCounter++;
        return result;
    }

	//Called if the user pressed 'add annoation' button
    public void AddAnnotationPressed()
    {
		if(currentActiveScreen == ActiveScreen.add) {
			//TODO Abort AddAnnotation and close Screen
			AddEditScreen.SetActive(false);
			currentActiveScreen = ActiveScreen.none;
		} else {
			//Open AnnotationScreen
			listScreen.SetActive (false);
			AddEditScreen.SetActive(true);
			currentActiveScreen = ActiveScreen.add;
		}

		/*if (currentState == State.idle)
		{
			listScreen.SetActive (false);
            changeCurrentStateToAddAnnotationPressed();
        }*/
    }

	//Called if the user pressed 'show Annotation' button
	public void ShowAnnotationsList()
	{
		if(currentActiveScreen == ActiveScreen.list) {
			// Close AnnotationListScreen
			listScreen.SetActive (false);
			currentActiveScreen = ActiveScreen.none;
		} else {
			//Open AnnotationListScreen
			AddEditScreen.SetActive(false);
			listScreen.SetActive (true);
			currentActiveScreen = ActiveScreen.list;
		}

		/*newAnnotationSetupScreen.SetActive (false);
		listScreen.SetActive (true);
		*/
	}



	// Called when user clicks on Organ
	void OnMeshClicked( PointerEventData eventData )
	{
		Debug.Log ("Clicked: " + eventData.pointerPressRaycast.worldPosition);

		if(currentActiveScreen == ActiveScreen.add) {
			//TODO Create/Move Annotation on Clicked Position
		} else if(currentActiveScreen == ActiveScreen.list) {
			//TODO Is an Annotation on Clicked Position?
		}
		/*
		if (currentState == State.generatingAnnotationPoint) {

			currentAnnotatinPoint = createAnnotation(
				Quaternion.LookRotation( eventData.pointerPressRaycast.worldNormal ),
				eventData.pointerPressRaycast.worldPosition);
			//createAnnotationLabelAndLine(currentAnnotatinPoint, "");
			currentState = State.annotationPointSelected;
			annotationTextInput.gameObject.SetActive (true);
			saveButton.gameObject.SetActive (true);
		}
		*/
	}

	//Called if the user confirmed changes on an Annotation in List and File
    public void SaveAnnotation()
    {	
		
    	currentAnnotation = null;
        
		//Reset Edit Tools
		annotationTextInput.text = "";
        
		//Deactivate all Edit Tools
		annotationTextInput.gameObject.SetActive (false);
		saveButton.gameObject.SetActive (false);

		//Save changes in File
		saveAnnotationInFile();
    }

	// Deletes all annotations.
	public void clearAll()
    {
        foreach (GameObject g in annotationList)
        {
            if(g != null)
            {
                //Destroy Label
                GameObject label = g.GetComponent<Annotation>().annotationLabel;
                if (label != null)
                {
                    Destroy(label);
                }
                //Delete points
                Destroy(g);
            }
        }
        //Delete list
        annotationList = new List<GameObject>();
    }

	//Creates view of annotationList (List elements on Screen)
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

        foreach (GameObject g in annotationList)
        {            
            // Create a new instance of the list button:
            GameObject newEntry = Instantiate(annotationListEntry).gameObject;
			newEntry.SetActive(true);

            // Attach the new button to the list:
			newEntry.transform.SetParent(annotationListEntry.transform.parent, false);


            // Change button text to name of tool:
			//Doesnt work TODO
			//GameObject textObject = newEntry.transform.Find("Text").gameObject;
			Text buttonText = newEntry.GetComponent<Text>();
            Annotation ap = g.GetComponent<Annotation>();
            if(ap != null)
            {
                buttonText.text = ap.text;

                // Attach AnnotationPointID to the new button and save the id of the annotation point 
				newEntry.AddComponent<AnnotationPointID>().annotationPointID = ap.id;
            }
            
        }
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
            foreach(GameObject ap in annotationList)
            {
                AnnotationJson apj = new AnnotationJson();
                apj.Text = ap.GetComponent<Annotation>().text;
                apj.PositionX = ap.transform.localPosition.x;
                apj.PositionY = ap.transform.localPosition.y;
                apj.PositionZ = ap.transform.localPosition.z;

                apj.RotationW = ap.transform.localRotation.w;
                apj.RotationX = ap.transform.localRotation.x;
                apj.RotationY = ap.transform.localRotation.y;
                apj.RotationZ = ap.transform.localRotation.z;

                apj.Creator = ap.GetComponent<Annotation>().creator;
				apj.CreationDate = ap.GetComponent<Annotation> ().creationDate;
                outputFile.WriteLine(JsonMapper.ToJson(apj));
            }
            outputFile.Close();
        }
        return;
    }

	//Load all annotations out of File in List
    private void loadAnnotationFromFile(object obj = null)
    {	
        if (Patient.getLoadedPatient() == null)
        {
            return;
        }
		
		//Clear Screen
        clearAll();
		
		//get Annotation.json
        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/annotation.json"; //TODO read from meta.json??

        if (!File.Exists(path))
        {
            return;
        }
		
        List<AnnotationJson> apjList = new List<AnnotationJson>();


        // Read the file
        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(path);
        while ((line = file.ReadLine()) != null)
        {
            AnnotationJson apj = JsonMapper.ToObject<AnnotationJson>(line);
            apjList.Add(apj);
        }
        file.Close();

		//List of Json Objects -> AnnotationList
        foreach(AnnotationJson apj in apjList)
        {
            Quaternion rotation = new Quaternion((float)apj.RotationX, (float)apj.RotationY, (float)apj.RotationZ, (float)apj.RotationW);
            Vector3 position = new Vector3((float)apj.PositionX, (float)apj.PositionY, (float)apj.PositionZ);

			GameObject annotation = createAnnotation(Quaternion.identity, position);
            annotation.transform.localRotation = rotation;

			annotation.GetComponent<Annotation> ().SetLabel (apj.Text);
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
        foreach (GameObject g in annotationList)
        {
            if (g.GetComponent<Annotation>().id == apID.annotationPointID)
            {
                //Destroy Label
                GameObject label = g.GetComponent<Annotation>().annotationLabel;
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
        foreach (GameObject g in annotationList)
        {
            if (g.GetComponent<Annotation>().id == apID.annotationPointID)
            {
                objToRemove = g;
                break;
            }
        }
        if (objToRemove != null)
        {
            annotationList.Remove(objToRemove);
        }

        updateAnnotationList();

        saveAnnotationInFile();
		
    }

	//Called if the patient is closed
    public void closePatient(object obj = null)
    {
		clearAll();
    }

}
