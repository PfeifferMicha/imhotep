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
	public GameObject annotationToolBar;




	//Screens
	public GameObject listScreen;
	public GameObject AddEditScreen;

	//Add Annotation Screen Things
	public GameObject instructionText;
	public GameObject annotationSettings;
	public InputField annotationTextInput;
	/*public Button saveButton;
	public Button abortButton;
	public Button redButton;
	public Button blueButton;
	public Button greenButton;
	public Button yellowButton;
	public Button purpleButton;
	public Button cyanButton;*/



	public GameObject meshNode;
	public GameObject meshPositionNode;

	//States
	private ActiveScreen currentActiveScreen = ActiveScreen.none;

	//current is the edited of actual Object to reset when abort is pressed
	private GameObject currentAnnotationListEntry = null;
	private GameObject oldAnnotationListEntry = null;

	private List<GameObject> annotationListEntryList = new List<GameObject>();
	//private List<GameObject> annotationList = new List<GameObject>();

    private InputDeviceManager idm;

	//private static int annotationCounter = 0; //Used to create unique id for annoation point

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
	public void InputChanged(){
		if(currentAnnotationListEntry != null) {
			currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().updateLabel (annotationTextInput.text);

		}
	}

	//Used to Create Annotation Mesh
	// Local Position
	private GameObject createAnnotationMesh(Quaternion rotation, Vector3 position) {

		GameObject newAnnotation = (GameObject)Instantiate(annotationPointObj, position, rotation);
        

		newAnnotation.transform.localScale = new Vector3( 5, 5, 5 );
		newAnnotation.transform.SetParent( meshPositionNode.transform, false );

        Annotation ap = newAnnotation.AddComponent<Annotation>();
        ap.enabled = false;

		newAnnotation.SetActive (true);

		//Create Label for annotation
		newAnnotation.GetComponent<Annotation> ().CreateLabel (annotationLabel);

		return newAnnotation;
    }

	//Called if the user pressed 'add annoation' button
    public void AddAnnotationPressed()
    {
		if(currentActiveScreen == ActiveScreen.add) {
			//TODO Abort AddAnnotation and close Screen
			AbortAnnotationChanges ();
			closeAnnotationScreen ();
		} else {
			//Open AnnotationScreen
			listScreen.SetActive (false);
			openAnnotationScreen ();
			if(currentAnnotationListEntry == null) {
				//deactivate input till current Annotation exists
				annotationSettings.gameObject.SetActive(false);
				instructionText.gameObject.SetActive (true);
			} else {
				UnlockEditSettings ();
				GameObject curAnno = currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().getAnnotation ();
				annotationTextInput.text = curAnno.GetComponent<Annotation> ().text;
			}

			currentActiveScreen = ActiveScreen.add;
		}
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

	//Swap image of Annotation button
	private void closeAnnotationScreen() {
		GameObject addButton = annotationToolBar.transform.GetChild (0).GetChild (0).gameObject;
		//show Add
		addButton.transform.GetChild (0).gameObject.SetActive (true);
		addButton.transform.GetChild (1).gameObject.SetActive (false);
	
		// Reset Screen
		currentAnnotationListEntry = null;
		oldAnnotationListEntry = null;
		//Reset Edit Tools
		annotationSettings.GetComponentInChildren<InputField>().text = "";
		annotationSettings.gameObject.SetActive(false);
		instructionText.gameObject.SetActive (true);
		// Close Screen
		AddEditScreen.SetActive(false);
		currentActiveScreen = ActiveScreen.none;
	}

	private void openAnnotationScreen() {
		GameObject addButton = annotationToolBar.transform.GetChild (0).GetChild (0).gameObject;
		//show Abort
		addButton.transform.GetChild (1).gameObject.SetActive (true);
		addButton.transform.GetChild (0).gameObject.SetActive (false);
		// open Screen
		AddEditScreen.SetActive(true);
		currentActiveScreen = ActiveScreen.add;
	}



	// Called when user clicks on Organ
	public void OnMeshClicked( PointerEventData eventData )
	{
		Debug.Log ("Clicked: " + eventData.pointerPressRaycast.worldPosition);

		if(currentActiveScreen == ActiveScreen.add) {
			if(currentAnnotationListEntry == null) {
				
				Vector3 localpos = meshPositionNode.transform.InverseTransformPoint(eventData.pointerPressRaycast.worldPosition);
				Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerPressRaycast.worldNormal);
				GameObject newAnnotation = currentAnnotationListEntry = createAnnotationMesh(Quaternion.LookRotation( localNormal),
					localpos);
				//add to List
				currentAnnotationListEntry = createNewAnnotationListEntry (newAnnotation);
				UnlockEditSettings ();
			}
		} else if(currentActiveScreen == ActiveScreen.list) {
			//Edit Annotation
			if(eventData.pointerPressRaycast.gameObject.CompareTag("AnnotationPoint")) {
				currentAnnotationListEntry = eventData.pointerPressRaycast.gameObject.GetComponent<Annotation> ().myAnnotationListEntry;
				//TODO Highlight in List
			}
		} else if(currentActiveScreen == ActiveScreen.none) {
			//Edit Annotation
			if(eventData.pointerPressRaycast.gameObject.CompareTag("AnnotationPoint")) {
				currentAnnotationListEntry = eventData.pointerPressRaycast.gameObject.GetComponent<Annotation> ().myAnnotationListEntry;
				AddAnnotationPressed ();
			}
		}
	}

	//Called if the user pressed Save Button
    public void SaveAnnotation()
    {	

		if (oldAnnotationListEntry == null) {
			annotationListEntryList.Add (currentAnnotationListEntry);			
		}

		closeAnnotationScreen ();

		//Save changes in File
		saveAnnotationInFile();
    }

	//Called to abort current add /edit process
	public void AbortAnnotationChanges()
	{	
		if (oldAnnotationListEntry == null) {
			if(currentAnnotationListEntry != null) {
				removeOneAnnotation (currentAnnotationListEntry);
			}
		} else {
			GameObject curAnnotation = currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().getAnnotation ();
			curAnnotation.GetComponent<Annotation>().AbortChanges(oldAnnotationListEntry.GetComponent<AnnotationListEntry>().getAnnotation());
		}
			
		closeAnnotationScreen ();
	}

	//Called to Create a New Annotation when Open Add/Edit Screen
	private void UnlockEditSettings () {
		//Mesh should be existing
		if(currentAnnotationListEntry == null) {
			Debug.LogAssertion("currentAnnotation is null");
		} else {
			// you can now Change Label text and save
			instructionText.gameObject.SetActive(false);
			annotationSettings.gameObject.SetActive (true);
		}


	}

	//Creates a new AnnotationListEntry, gets the Annotation to this entry, does not add to list
	private GameObject createNewAnnotationListEntry (GameObject annotation) {
		if(annotation != null) {

			// Create a new instance of the list button:
			GameObject newEntry = Instantiate(annotationListEntry).gameObject;
			newEntry.SetActive(true);

			// Attach the new Entry to the list:
			newEntry.transform.SetParent(annotationListEntry.transform.parent, false);

			newEntry.GetComponent<AnnotationListEntry> ().setupListEntry (annotation);


			return newEntry;
		} else {
			Debug.LogAssertion ("Annotation is Null");
		}
		return null;
	}


	//Called by AnnotationListEntryControl with the annotation to edit
	public void EditAnnotation(GameObject aListEntry) {	
		currentAnnotationListEntry = aListEntry;
		oldAnnotationListEntry = aListEntry;
		AddAnnotationPressed ();

	}

	//Called by AnnotationListEntryControl with the annotation to delete form view and File
	public void DeleteAnnotation(GameObject aListEntry) {


		//delete List Entry
		annotationListEntryList.Remove (aListEntry);

		removeOneAnnotation (aListEntry);

		//delete in File (save new File)
		saveAnnotationInFile();
	}

	// Deletes all annotationsMesh from Screen(clears Screen) and out off annotationList 
	private void clearAll()
    {
		foreach (GameObject g in annotationListEntryList)
        {
            if(g != null)
            {
				removeOneAnnotation (g);
            }
        }
        //Delete list
		annotationListEntryList = new List<GameObject>();
    }

	//Saves all annotations in a file
    private void saveAnnotationInFile()
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
            foreach(GameObject apListEntry in annotationListEntryList)
            {
				GameObject ap = apListEntry.GetComponent<AnnotationListEntry> ().getAnnotation();
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

			//setup new Annotation as maesh and in List
			GameObject newAnnotation = createAnnotationMesh(rotation, position);
			newAnnotation.GetComponent<Annotation>().SetLabel(apj.Text);

			annotationListEntryList.Add (createNewAnnotationListEntry (newAnnotation));
        }
    }

	//removes a annotation given in self from view
	private void removeOneAnnotation(GameObject aListEntry) {

		//delete Annotation Mesh
		aListEntry.GetComponent<AnnotationListEntry> ().getAnnotation().GetComponent<Annotation>().destroyAnnotation();

		Destroy (aListEntry);		
    }

	//Called if the patient is closed
    public void closePatient(object obj = null)
    {
		clearAll();
    }

}
