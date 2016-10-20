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
	public string Text;
	public float ColorR;
	public float ColorG;
	public float ColorB;
	public double PositionX;
	public double PositionY;
	public double PositionZ;
	public double RotationW;
	public double RotationX;
	public double RotationY;
	public double RotationZ;
	public string Creator;
	public DateTime CreationDate;

}

public class AnnotationControl : MonoBehaviour
{

	public GameObject annotationPointObj;
	public GameObject annotationLabel;
	public GameObject annotationListEntry;
	public GameObject annotationToolBar;

	//Screens
	public GameObject listScreen;
	public GameObject AddEditScreen;

	//Add Annotation Screen Things
	public GameObject instructionText;
	public GameObject annotationSettings;

	public GameObject meshNode;
	public GameObject meshPositionNode;

	//States
	private ActiveScreen currentActiveScreen = ActiveScreen.none;

	//current is the edited of actual Object to reset when abort is pressed
	private GameObject currentAnnotationListEntry = null;

	private List<GameObject> annotationListEntryList = new List<GameObject> ();


	private ClickNotifier clickNotifier;

	//The State of Annotation Control, which scrren is active
	// none , no screen is active
	// add , add screen is active
	// list, list screen is active
	private enum ActiveScreen
	{
		none,
		add,
		list
	}

	void OnEnable ()
	{
		// Register event callbacks:
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_FinishedLoading, loadAnnotationFromFile);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, closePatient);
		loadAnnotationFromFile ();

		AddEditScreen.SetActive (false);
		listScreen.SetActive (false);

		annotationLabel.SetActive (false);
		annotationPointObj.SetActive (false);

		clickNotifier = meshPositionNode.AddComponent<ClickNotifier> ();
		clickNotifier.clickNotificationEvent = OnMeshClicked;
		clickNotifier.hoverNotificationEvent = hoveredOverMesh;
	}

	void OnDisable ()
	{
		// Unregister myself:
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_FinishedLoading, loadAnnotationFromFile);
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Closed, closePatient);
		clearAll ();
		Destroy (clickNotifier);
	}


	// Use this for initialization
	void Start ()
	{
		if (annotationPointObj == null) {
			Debug.LogError ("No Annotation Point Object is set in Annotation.cs");
		}

		annotationListEntry.gameObject.SetActive (false);
	}

	//################ Called By Buttons ###############

	//Called if the user pressed 'add annoation' button
	public void AddAnnotationPressed ()
	{
		if (currentActiveScreen == ActiveScreen.add) {
			closeAnnotationScreen ();
		} else {
			//Open AnnotationScreen
			if (currentAnnotationListEntry == null) {
				instructionText.SetActive (true);
			} else {
				instructionText.SetActive (false);
			}
			openAnnotationScreen ();
		}
	}

	//Called if the user pressed 'show Annotation' button
	public void ShowAnnotationsList ()
	{
		if (currentActiveScreen == ActiveScreen.list) {
			// Close AnnotationListScreen
			listScreen.SetActive (false);
			currentActiveScreen = ActiveScreen.none;
		} else {
			//Open AnnotationListScreen
			closeAnnotationScreen();
			listScreen.SetActive (true);
			currentActiveScreen = ActiveScreen.list;
		}
	}

	//Called by Color Button with it self as Attribut
	public void ChangeColorPressed (GameObject newColorButton)
	{
		currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().changeAnnotationColor (
			newColorButton.GetComponent<Button> ().colors.normalColor);
		saveAnnotationInFile ();
	}

	//################ Called By Events ################

	// Called when user clicks on Organ
	public void OnMeshClicked (PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) {
			return;
		}
		if (eventData.pointerEnter.CompareTag ("AnnotationLabel")) {
			if (currentActiveScreen == ActiveScreen.add) {
				eventData.pointerEnter.GetComponentInParent<AnnotationLabel> ().LabelClicked (eventData);
			} else if (currentActiveScreen == ActiveScreen.list) {
				//Jump to Annotation in List
				jumpToListEntry (eventData.pointerEnter.GetComponentInParent<Annotation> ().gameObject);
			} else {
				EditAnnotation (eventData.pointerEnter.GetComponentInParent<Annotation> ().myAnnotationListEntry);
			}
		} else if (eventData.pointerEnter.CompareTag ("Annotation")) {
			if (currentActiveScreen == ActiveScreen.add) {
				
			} else if (currentActiveScreen == ActiveScreen.list) {
				jumpToListEntry (eventData.pointerEnter);
			} else {
				EditAnnotation (eventData.pointerEnter.GetComponent<Annotation> ().myAnnotationListEntry);
			}
		} else {
			if (currentActiveScreen == ActiveScreen.add) {
				Vector3 localpos = meshPositionNode.transform.InverseTransformPoint (eventData.pointerPressRaycast.worldPosition);
				Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerPressRaycast.worldNormal);
				if (currentAnnotationListEntry == null) {
					if (!eventData.pointerEnter.CompareTag ("Annotation") && !eventData.pointerEnter.CompareTag ("AnnotationLabel")) {
						GameObject newAnnotation = createAnnotationMesh (Quaternion.LookRotation (localNormal), localpos);
						//add to List
						EditAnnotation (createNewAnnotationListEntry (newAnnotation));

					} 
				} else {
					if (!eventData.pointerEnter.CompareTag ("Annotation") && !eventData.pointerEnter.CompareTag ("AnnotationLabel")) {
						changeAnnotationPosition (Quaternion.LookRotation (localNormal), localpos);
					} 
				}
			} 
		}


	}

	//Called by Hover Organ Event
	public void hoveredOverMesh (PointerEventData eventData)
	{
		/*if (hoverAnnotation != null) {
			Debug.Log ("Hover");
			Vector3 localpos = meshPositionNode.transform.InverseTransformPoint (eventData.pointerCurrentRaycast.worldPosition);
			Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerCurrentRaycast.worldNormal);
			hoverAnnotation.GetComponent<Annotation> ().updatePosition (Quaternion.LookRotation (localNormal), localpos);
		}*/
	}

	//################ Private Methods #################

	private void jumpToListEntry(GameObject annotation) {
		
		if(currentActiveScreen == ActiveScreen.list) {
			GameObject listEntry = annotation.GetComponent<Annotation> ().myAnnotationListEntry;
			Vector2 pos = listEntry.gameObject.GetComponent<AnnotationListEntry>().getListPos();
			listEntry.transform.parent.GetComponent<RectTransform>().anchoredPosition = 
				new Vector2(0.0f, (-(pos.y) - (listScreen.GetComponent<RectTransform>().rect.height / 3 ) ));
		}
	}


	//Used to Create Annotation Mesh
	// Local Position
	private GameObject createAnnotationMesh (Quaternion rotation, Vector3 position)
	{

		GameObject newAnnotation = (GameObject)Instantiate (annotationPointObj, position, rotation);

		newAnnotation.transform.localScale = new Vector3 (5, 5, 5);
		newAnnotation.transform.SetParent (meshPositionNode.transform, false);

		newAnnotation.SetActive (true);
		//Create Label for annotation
		newAnnotation.GetComponent<Annotation> ().CreateLabel (annotationLabel);

		return newAnnotation;
	}

	//Swap image of Annotation button
	private void closeAnnotationScreen ()
	{
		// Reset Screen
		currentAnnotationListEntry = null;
		/*if (hoverAnnotation != null) {
			hoverAnnotation.GetComponent<Annotation> ().destroyAnnotation ();
			hoverAnnotation = null;
		}*/
		//Reset Edit Tools
		instructionText.gameObject.SetActive (true);
		// Close Screen
		AddEditScreen.SetActive (false);
		currentActiveScreen = ActiveScreen.none;

		//save changes when close screen
		saveAnnotationInFile ();

	}

	//Opens AnnotationScreen
	private void openAnnotationScreen ()
	{
		// open Screen
		listScreen.SetActive(false);
		AddEditScreen.SetActive (true);
		currentActiveScreen = ActiveScreen.add;
	}

	//Change Annotation Position
	private void changeAnnotationPosition (Quaternion rotation, Vector3 position) {
		currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().updateAnnotationposition (rotation, position);
	}

	//Creates a new AnnotationListEntry, gets the Annotation to this entry, does not add to list
	private GameObject createNewAnnotationListEntry (GameObject annotation)
	{
		if (annotation != null) {

			// Create a new instance of the list button:
			GameObject newEntry = Instantiate (annotationListEntry).gameObject;
			newEntry.SetActive (true);

			// Attach the new Entry to the list:
			newEntry.transform.SetParent (annotationListEntry.transform.parent, false);

			newEntry.GetComponent<AnnotationListEntry> ().setupListEntry (annotation);

			annotationListEntryList.Add (newEntry);
			return newEntry;
		} else {
			Debug.LogAssertion ("Annotation is Null");
		}
		return null;
	}

	// Deletes all annotationsMesh from Screen(clears Screen) and out off annotationList
	private void clearAll ()
	{
		foreach (GameObject g in annotationListEntryList) {
			if (g != null) {
				removeOneAnnotation (g);
			}
		}
		//Delete list
		annotationListEntryList = new List<GameObject> ();
	}

	//Load all annotations out of File in List
	private void loadAnnotationFromFile (object obj = null)
	{	
		if (Patient.getLoadedPatient () == null) {
			return;
		}

		//Clear Screen
		clearAll ();

		//get Annotation.json
		Patient currentPatient = Patient.getLoadedPatient ();
		string path = currentPatient.path + "/annotation.json"; //TODO read from meta.json??

		if (!File.Exists (path)) {
			return;
		}

		List<AnnotationJson> apjList = new List<AnnotationJson> ();
		// Read the file
		string line;
		System.IO.StreamReader file = new System.IO.StreamReader (path);
		while ((line = file.ReadLine ()) != null) {
			AnnotationJson apj = JsonUtility.FromJson<AnnotationJson> (line);
			apjList.Add (apj);
		}
		file.Close ();

		//List of Json Objects -> AnnotationList
		foreach (AnnotationJson apj in apjList) {
			Quaternion rotation = new Quaternion ((float)apj.RotationX, (float)apj.RotationY, (float)apj.RotationZ, (float)apj.RotationW);
			Vector3 position = new Vector3 ((float)apj.PositionX, (float)apj.PositionY, (float)apj.PositionZ);

			//setup new Annotation as maesh and in List
			GameObject newAnnotation = createAnnotationMesh (rotation, position);
			newAnnotation.GetComponent<Annotation> ().setLabeText (apj.Text);
			if(apj.ColorR == 0.0f && apj.ColorG == 0.0f && apj.ColorB == 0.0f) {
				newAnnotation.GetComponent<Annotation> ().setDefaultColor ();
			} else {
				newAnnotation.GetComponent<Annotation> ().changeColor (new Color (apj.ColorR, apj.ColorG, apj.ColorB));
			}

			createNewAnnotationListEntry (newAnnotation);
		}
	}

	//removes a annotation given in self from view
	private void removeOneAnnotation (GameObject aListEntry)
	{

		//delete Annotation Mesh
		aListEntry.GetComponent<AnnotationListEntry> ().DestroyAnnotation();

		Destroy (aListEntry);		
	}

	//################ Other Methods ###################

	//Called to edit Annotation
	public void EditAnnotation (GameObject aListEntry)
	{
		currentAnnotationListEntry = aListEntry;
		/*hoverAnnotation = currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().duplicateAnnotation ();
		hoverAnnotation.GetComponent<Annotation> ().disableCollider ();*/
		instructionText.SetActive (false);
		openAnnotationScreen ();
	}

	//Called by AnnotationListEntryControl with the annotation to delete form view and File
	public void DeleteAnnotation (GameObject aListEntry)
	{
		//delete List Entry
		annotationListEntryList.Remove (aListEntry);

		removeOneAnnotation (aListEntry);

		//delete in File (save new File)
		saveAnnotationInFile ();
	}

	//Saves all annotations in a file
	public void saveAnnotationInFile ()
	{

		if (Patient.getLoadedPatient () == null) {
			return;
		}

		Patient currentPatient = Patient.getLoadedPatient ();
		string path = currentPatient.path + "/annotation.json";

		//Create file if it not exists
		if (!File.Exists (path)) {
			using (StreamWriter outputFile = new StreamWriter (path, true)) {
				outputFile.Close ();
			}
		}

		//Write annotations in file
		using (StreamWriter outputFile = new StreamWriter (path)) {
			foreach (GameObject apListEntry in annotationListEntryList) {
				GameObject ap = apListEntry.GetComponent<AnnotationListEntry> ().getAnnotation ();
				AnnotationJson apj = new AnnotationJson ();
				apj.Text = ap.GetComponent<Annotation> ().getLabelText ();
				apj.ColorR = ap.GetComponent<Annotation> ().getColor().r;
				apj.ColorG = ap.GetComponent<Annotation> ().getColor().g;
				apj.ColorB = ap.GetComponent<Annotation> ().getColor().b;
				apj.PositionX = ap.transform.localPosition.x;
				apj.PositionY = ap.transform.localPosition.y;
				apj.PositionZ = ap.transform.localPosition.z;

				apj.RotationW = ap.transform.localRotation.w;
				apj.RotationX = ap.transform.localRotation.x;
				apj.RotationY = ap.transform.localRotation.y;
				apj.RotationZ = ap.transform.localRotation.z;

				apj.Creator = ap.GetComponent<Annotation> ().creator;
				apj.CreationDate = ap.GetComponent<Annotation> ().creationDate;
				outputFile.WriteLine (JsonUtility.ToJson (apj));
			}
			outputFile.Close ();
		}
		return;
	}

	//Called if the patient is closed
	public void closePatient (object obj = null)
	{
		clearAll ();
	}
}
