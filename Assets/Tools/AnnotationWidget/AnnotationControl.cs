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

	//instance
	public static AnnotationControl instance;


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
	private GameObject hoverAnnotation;


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

	public AnnotationControl()
	{
		if( instance != null )
			throw(new System.Exception ("Error: Cannot create more than one instance of ANNOTATIONCONTROL!"));
		instance = this;

	}

	void OnEnable ()
	{
		// Register event callbacks:
		AddEditScreen.SetActive (false);
		listScreen.SetActive (false);
		currentActiveScreen = ActiveScreen.none;


		annotationLabel.SetActive (false);
		annotationPointObj.SetActive (false);

		clickNotifier = meshPositionNode.AddComponent<ClickNotifier> ();
		clickNotifier.clickNotificationEvent = OnMeshClicked;
		clickNotifier.hoverNotificationEvent = hoveredOverMesh;
		clickNotifier.exitNotificationEvent = pointerExitMesh;
	}

	void OnDisable ()
	{
		// Unregister myself:
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
			closeAnnotationScreen ();
			listScreen.SetActive (true);
			currentActiveScreen = ActiveScreen.list;
		}
	}
	//Called by Color Button with it self as Attribut
	public void ChangeColorPressed (GameObject newColorButton)
	{
		if (currentAnnotationListEntry != null) {
			currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().changeAnnotationColor (
				newColorButton.GetComponent<Button> ().colors.normalColor);
		}
		if (hoverAnnotation != null) {
			hoverAnnotation.GetComponent<Annotation> ().changeColor (
				newColorButton.GetComponent<Button> ().colors.normalColor);
		}

		updatePatientAnnotationList ();
	}

	//################ Called By Events ################

	// Called when user clicks on Organ
	public void OnMeshClicked (PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) {
			return;
		}
		if (eventData.pointerEnter.CompareTag ("AnnotationLabel")) {
			if (currentActiveScreen == ActiveScreen.list) {
				//Jump to Annotation in List
				jumpToListEntry (eventData.pointerEnter.GetComponentInParent<Annotation> ().gameObject);
			} else {
				EditAnnotation (eventData.pointerEnter.GetComponentInParent<Annotation> ().myAnnotationListEntry);
				eventData.pointerEnter.GetComponentInParent<AnnotationLabel> ().LabelClicked (eventData);
			}
		} else if (eventData.pointerEnter.CompareTag ("Annotation")) {
			if (currentActiveScreen == ActiveScreen.list) {
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
		if (hoverAnnotation != null) {
			if(!eventData.pointerEnter.gameObject.CompareTag("AnnotationLabel") && !eventData.pointerEnter.gameObject.CompareTag("Annotation")) {
				hoverAnnotation.SetActive (true);
				Vector3 localpos = meshPositionNode.transform.InverseTransformPoint (eventData.pointerCurrentRaycast.worldPosition);
				Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerCurrentRaycast.worldNormal);
				hoverAnnotation.GetComponent<Annotation> ().updatePosition (Quaternion.LookRotation (localNormal), localpos);
			} else {
				hoverAnnotation.SetActive (false);
			}
		}
	}

	//Called if pointer is noty anymore on Mesh
	public void pointerExitMesh (PointerEventData eventData)
	{
		if (hoverAnnotation != null) {
			hoverAnnotation.SetActive (false);
		}
	}

	//################ Private Methods #################



	private void jumpToListEntry (GameObject annotation)
	{
		if (currentActiveScreen == ActiveScreen.list) {
			GameObject listEntry = annotation.GetComponent<Annotation> ().myAnnotationListEntry;
			Vector2 pos = listEntry.gameObject.GetComponent<AnnotationListEntry> ().getListPos ();
			listEntry.transform.parent.GetComponent<RectTransform> ().anchoredPosition = 
				new Vector2 (0.0f, (-(pos.y) - (listScreen.GetComponent<RectTransform> ().rect.height / 3)));
		}
	}

	private void createHoverAnnotation ()
	{
		//Clone Annotation
		hoverAnnotation = (GameObject)Instantiate (annotationPointObj);

		hoverAnnotation.transform.localScale = new Vector3 (5, 5, 5);
		hoverAnnotation.transform.SetParent (meshPositionNode.transform, false);
		//SetSettings
		hoverAnnotation.SetActive (true);
		hoverAnnotation.GetComponent<Annotation> ().makeTransperent ();
		hoverAnnotation.GetComponent<Annotation> ().disableCollider ();
		hoverAnnotation.GetComponent<Annotation> ().setDefaultColor ();

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

		//set Color
		if(hoverAnnotation != null) {
			newAnnotation.GetComponent<Annotation> ().changeColor(hoverAnnotation.GetComponent<Annotation>().getColor());
		} else {
			newAnnotation.GetComponent<Annotation> ().setDefaultColor ();
		}
			
		return newAnnotation;
	}

	//Swap image of Annotation button
	private void closeAnnotationScreen ()
	{
		// Reset Screen
		currentAnnotationListEntry = null;
		Destroy (hoverAnnotation);
		hoverAnnotation = null;

		//Reset Edit Tools
		instructionText.gameObject.SetActive (true);
		// Close Screen
		AddEditScreen.SetActive (false);
		currentActiveScreen = ActiveScreen.none;

		//save changes when close screen
		updatePatientAnnotationList ();

	}

	//Opens AnnotationScreen
	private void openAnnotationScreen ()
	{
		//setup Hover
		if (hoverAnnotation == null) {
			createHoverAnnotation ();
		}
		//set color to actual edit annotation
		if (currentAnnotationListEntry != null) {
			hoverAnnotation.GetComponent<Annotation> ().changeColor (
				currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().getAnnotationColor ());
		}
		// open Screen
		listScreen.SetActive (false);
		AddEditScreen.SetActive (true);
		currentActiveScreen = ActiveScreen.add;
	}

	//Change Annotation Position
	private void changeAnnotationPosition (Quaternion rotation, Vector3 position)
	{
		currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().updateAnnotationposition (rotation, position);
		updatePatientAnnotationList ();
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





	private void setAllAnnotationActive(bool active) {
		Debug.LogWarning (annotationListEntryList.Count);
		foreach (GameObject g in annotationListEntryList) {
			if (g != null) {
				setAnnotationActive (g, active);
			}
		}
	}

	private void activateAnnotations(object obj = null) {
		setAllAnnotationActive (true);
	}

	//Deactivates Mesh on Screen
	private void setAnnotationActive(GameObject aListEntry, bool active) {
		aListEntry.GetComponent<AnnotationListEntry> ().setMyAnnotationActive (active);
	}

	//removes a annotation given in self from view
	private void removeOneAnnotation (GameObject aListEntry)
	{

		if(currentAnnotationListEntry = aListEntry)
		{
			currentAnnotationListEntry = null;
		}
		//delete Annotation Mesh
		aListEntry.GetComponent<AnnotationListEntry> ().DestroyAnnotation ();

		Destroy (aListEntry);		
	}

	//################ Other Methods ##################


	// deleteall Annotations
	public void deleteAllAnnotations ()
	{
		foreach (GameObject g in annotationListEntryList) {
			if (g != null) {
				removeOneAnnotation (g);
			}
		}

		currentAnnotationListEntry = null;
		//Delete hoverAnnotation
		Destroy (hoverAnnotation);
		hoverAnnotation = null;
		annotationListEntryList = new List<GameObject> ();
		updatePatientAnnotationList ();
	}

	// deactivates all Annotations
	public void clearAll ()
	{
		setAllAnnotationActive (false);
		currentAnnotationListEntry = null;
		//Delete hoverAnnotation
		Destroy (hoverAnnotation);
		hoverAnnotation = null;

	}

	//Called by load Method in Patient to Create all Annotations in File
	public void createAnnotation(AnnotationJson annotation) {
		Quaternion rotation = new Quaternion ((float)annotation.RotationX, (float)annotation.RotationY, (float)annotation.RotationZ, (float)annotation.RotationW);
		Vector3 position = new Vector3 ((float)annotation.PositionX, (float)annotation.PositionY, (float)annotation.PositionZ);

		//setup new Annotation as maesh and in List
		GameObject newAnnotation = createAnnotationMesh (rotation, position);
		newAnnotation.GetComponent<Annotation> ().setLabeText (annotation.Text);
		//Color not empty (Black)
		if (annotation.ColorR != 0.0f || annotation.ColorG != 0.0f || annotation.ColorB != 0.0f) {
			newAnnotation.GetComponent<Annotation> ().changeColor (new Color (annotation.ColorR, annotation.ColorG, annotation.ColorB));
		}

		createNewAnnotationListEntry (newAnnotation);
	}


	//updates list in Patient ... will be safed in file by Patient class
	public void updatePatientAnnotationList()
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null)
			p.updateAnnotationList (annotationListEntryList);
	}

	//Called to edit Annotation
	public void EditAnnotation (GameObject aListEntry)
	{
		currentAnnotationListEntry = aListEntry;
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
		updatePatientAnnotationList ();
	}
}
