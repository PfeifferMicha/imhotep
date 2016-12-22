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
	public AnnotationControl.AnnotationType type;

	public float ColorR;
	public float ColorG;
	public float ColorB;

	public float ScaleX;
	public float ScaleY;
	public float ScaleZ;

	public double PositionX;
	public double PositionY;
	public double PositionZ;

	public double RotationW;
	public double RotationX;
	public double RotationY;
	public double RotationZ;

	public double MeshRotationW;
	public double MeshRotationX;
	public double MeshRotationY;
	public double MeshRotationZ;


	public string Creator;
	public DateTime CreationDate;

}

public class AnnotationControl : MonoBehaviour
{
	//Prefabs
	public GameObject annotationPinGroupObj;
	public GameObject annotationPlaneGroupObj;
	public GameObject annotationSphereGroupObj;

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
	private AnnotationType currentAnnotationType;
	private GameObject currentAnnotationListEntry = null;

	private List<GameObject> annotationListEntryList = new List<GameObject> ();
	private GameObject previewAnnotation;

	private ClickNotifier clickNotifier;

	//Transparencys
	public float previewTransparency = 0.1f;
	public float editAnnotationTransparency = 0.1f;
	public float organTransparency = 0.4f;


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

	public enum AnnotationType {
		pin,
		plane,
		sphere
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

		setAllAnnotationsActive (true);

		annotationLabel.SetActive (false);
		annotationPinGroupObj.SetActive (false);
		annotationPlaneGroupObj.SetActive (false);
		annotationSphereGroupObj.SetActive (false);
		annotationListEntry.gameObject.SetActive (false);

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
		
		if (annotationPinGroupObj == null) {
			Debug.LogError ("No Annotation Pin Object is set in AnnotationControl.cs");
		}
		if (annotationPlaneGroupObj == null) {
			Debug.LogError ("No Annotation Plane Object is set in AnnotationControl.cs");
		}
		if (annotationSphereGroupObj == null) {
			Debug.LogError ("No Annotation Sphere Object is set in AnnotationControl.cs");
		}


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
				enableOrgans ();
				currentAnnotationType = AnnotationType.pin;
				createPreviewAnnotation ();
				openAnnotationScreen ();
			} 

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
		if (previewAnnotation != null) {
			previewAnnotation.GetComponent<Annotation> ().changeColor (
				newColorButton.GetComponent<Button> ().colors.normalColor);
		}
		updatePatientAnnotationList ();
	}

	public void changeAnnoTypeToPin () {
		changeCurrentAnnotationType(AnnotationType.pin);
	}

	public void changeAnnoTypeToPlane () {
		changeCurrentAnnotationType(AnnotationType.plane);
	}

	public void changeAnnoTypeToSphere () {
		changeCurrentAnnotationType(AnnotationType.sphere);
	}

	//################ Called By Events ################

	// Called when user clicks on Organ
	public void OnMeshClicked (PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) {
			return;
		}
		//Click on Organ
		if (currentActiveScreen == ActiveScreen.add) {
			Vector3 localpos = meshPositionNode.transform.InverseTransformPoint (eventData.pointerPressRaycast.worldPosition);
			Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerPressRaycast.worldNormal);
			if (currentAnnotationListEntry == null) {
				GameObject newAnnotation = createAnnotationGroup (currentAnnotationType, Quaternion.LookRotation (localNormal), localpos);
				//add to List
				EditAnnotation (createNewAnnotationListEntry (newAnnotation));
			} else if (currentAnnotationType == AnnotationType.pin){
				changeAnnotationPosition (Quaternion.LookRotation (localNormal), localpos); 
			}
		} 


	}

	//Called by Hover Organ Event
	public void hoveredOverMesh (PointerEventData eventData)
	{
		if (previewAnnotation != null) {
			if(!eventData.pointerEnter.gameObject.CompareTag("AnnotationLabel") && !eventData.pointerEnter.gameObject.CompareTag("Annotation")) {
				previewAnnotation.SetActive (true);
				Vector3 localpos = meshPositionNode.transform.InverseTransformPoint (eventData.pointerCurrentRaycast.worldPosition);
				Vector3 localNormal = meshPositionNode.transform.InverseTransformDirection (eventData.pointerCurrentRaycast.worldNormal);
				previewAnnotation.GetComponent<Annotation> ().updatePosition (Quaternion.LookRotation (localNormal), localpos);
			} else {
				previewAnnotation.SetActive (false);
			}
		}
	}

	//Called if pointer is noty anymore on Mesh
	public void pointerExitMesh (PointerEventData eventData)
	{
		if (previewAnnotation != null) {
			previewAnnotation.SetActive (false);
		}
	}

	//################ Private Methods #################


	private void setAllAnnotationsActive(bool active) {
		foreach(GameObject g in annotationListEntryList) {
			if(g!= null) {
				setAnnotationActive (g, active);
			}
		}
	}

	private void setAnnotationActive(GameObject aListentry, bool active) {
		aListentry.GetComponent<AnnotationListEntry> ().setMyAnnotationActive (active);
	}

	private void setCurrentAnnotation(GameObject anno) {
		currentAnnotationListEntry = anno;
		currentAnnotationType = anno.GetComponent<AnnotationListEntry> ().getMyAnnotationType ();
	}

	private void resetCurrentAnnotation() {
		currentAnnotationListEntry = null;
	}



	private void disableOrgans() {
		GameObject.Find ("GlobalScript").GetComponent<HierarchicalInputModule> ().disableLayer ("MeshViewer");
		meshPositionNode.GetComponent<OpacityOfAllChanger> ().changeOpacityofAll (organTransparency);
	}

	private void enableOrgans() {
		GameObject.Find ("GlobalScript").GetComponent<HierarchicalInputModule> ().enableLayer ("MeshViewer");
		meshPositionNode.GetComponent<OpacityOfAllChanger> ().changeOpacityofAll (1f);
	}

	private void makeAnnotationsTransparent() {
		foreach(GameObject g in annotationListEntryList) {
			if(g != currentAnnotationListEntry) {
				g.GetComponent<AnnotationListEntry> ().resetAnnotationTransparency ();
			}
		}
	}

	private void resetAnnotationTransparency() {
		foreach(GameObject g in annotationListEntryList) {
			if(g != currentAnnotationListEntry) {
				g.GetComponent<AnnotationListEntry> ().makeAnnotationTransparent (editAnnotationTransparency);
			}
		}
	}

	private void changeCurrentAnnotationType(AnnotationType newType) {
		if(currentAnnotationListEntry == null) {
			currentAnnotationType = newType;
			createPreviewAnnotation ();
		} else {
			changeAnnoTypeTo (currentAnnotationListEntry, newType);
			EditAnnotation (currentAnnotationListEntry);
		}
	}

	private void changeAnnoTypeTo(GameObject annotationGroup, AnnotationType newType) {
		if(annotationGroup != null) {
			currentAnnotationType = newType;
			GameObject curAnnoGroup = annotationGroup.GetComponent<AnnotationListEntry> ().getAnnotation ();
			GameObject newAnnoGroup = createAnnotationGroup (newType, curAnnoGroup.GetComponent<Transform> ().localRotation, curAnnoGroup.GetComponent<Transform> ().localPosition);
			annotationGroup.GetComponent<AnnotationListEntry> ().replaceMyAnnotationMesh (newAnnoGroup);
			updatePatientAnnotationList ();
		}
	}


	private void jumpToListEntry (GameObject annotation)
	{
		if (currentActiveScreen == ActiveScreen.list) {
			GameObject listEntry = annotation.GetComponent<Annotation> ().myAnnotationListEntry;
			Vector2 pos = listEntry.gameObject.GetComponent<AnnotationListEntry> ().getListPos ();
			listEntry.transform.parent.GetComponent<RectTransform> ().anchoredPosition = 
				new Vector2 (0.0f, (-(pos.y) - (listScreen.GetComponent<RectTransform> ().rect.height / 3)));
		}
	}

	private void createPreviewAnnotation ()
	{
		//Clone Annotation
		GameObject annotationType = getAnnotationTypeObject (currentAnnotationType);
		Color c = new Color ();
		bool newColor = false;

		if(previewAnnotation != null) {
			c = previewAnnotation.GetComponent<Annotation> ().getColor ();
			Destroy (previewAnnotation);
			newColor = true;
		}
		previewAnnotation = (GameObject)Instantiate (annotationType);

		previewAnnotation.transform.localScale = new Vector3 (5, 5, 5);
		previewAnnotation.transform.SetParent (meshPositionNode.transform, false);
		//SetSettings
		previewAnnotation.SetActive (true);
		previewAnnotation.GetComponent<Annotation> ().makeTransperent (previewTransparency);
		previewAnnotation.GetComponent<Annotation> ().disableCollider ();
		if(newColor) {
			previewAnnotation.GetComponent<Annotation> ().changeColor (c);
		} else {
			previewAnnotation.GetComponent<Annotation> ().setDefaultColor ();
		}
	}


	private GameObject getAnnotationTypeObject (AnnotationType type) {
		GameObject annotationType;
		switch(type) {
		case AnnotationType.pin:
			annotationType = annotationPinGroupObj;
			break;
		case AnnotationType.plane:
			annotationType = annotationPlaneGroupObj;
			break;
		case AnnotationType.sphere:
			annotationType = annotationSphereGroupObj;
			break;
		default:
			annotationType = annotationPinGroupObj;
			break;
		}
		return annotationType;
	}


	//Used to Create Annotation Mesh
	// Local Position
	private GameObject createAnnotationGroup (AnnotationType annoType, Quaternion rotation, Vector3 position)
	{
		GameObject annotationType = getAnnotationTypeObject (annoType);
		GameObject newAnnotation = (GameObject)Instantiate (annotationType, position, rotation);

		newAnnotation.transform.localScale = new Vector3 (5, 5, 5);
		newAnnotation.transform.SetParent (meshPositionNode.transform, false);

		newAnnotation.SetActive (true);

		//set Type
		newAnnotation.GetComponent<Annotation>().myType = annoType;

		//Create Label for annotation
		newAnnotation.GetComponent<Annotation> ().CreateLabel (annotationLabel);

		//set Color
		if(previewAnnotation != null) {
			newAnnotation.GetComponent<Annotation> ().changeColor(previewAnnotation.GetComponent<Annotation>().getColor());
		} else {
			newAnnotation.GetComponent<Annotation> ().setDefaultColor ();
		}

		newAnnotation.GetComponent<Annotation> ().setDefaultTransparency ();
			
		return newAnnotation;
	}

	//Swap image of Annotation button
	private void closeAnnotationScreen ()
	{	
		enableOrgans ();
		// Reset Screen
		if(currentAnnotationListEntry != null) {
			currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().setAnnotationMovementActive (false);
		}
		resetCurrentAnnotation ();
		Destroy (previewAnnotation);
		previewAnnotation = null;

		//Reset Edit Tools
		resetAnnotationTransparency();
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
		//open Screen
		makeAnnotationsTransparent();
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

	//removes a annotation given in self from view
	private void removeOneAnnotation (GameObject aListEntry)
	{

		if(currentAnnotationListEntry = aListEntry)
		{
			resetCurrentAnnotation ();
		}
		//delete Annotation Mesh
		aListEntry.GetComponent<AnnotationListEntry> ().DestroyAnnotation ();

		Destroy (aListEntry);		
	}

	//################ Other Methods ##################

	public void resetLayers() {
		GameObject inMod = GameObject.Find ("GlobalScript");
		if(inMod != null) {
			inMod.GetComponent<HierarchicalInputModule> ().resetLayerMask ();
		}
	}

	/// <summary>
	/// Called by Annotation when Clicked on Annotation
	/// </summary>
	/// <returns><c>true</c>, if clicked Annotation is current Annotation at the moment, <c>false</c> otherwise.</returns>
	/// <param name="data"> eventdata of click event Data.</param>
	public bool annotationClicked(PointerEventData data) {
		if(currentAnnotationListEntry == data.pointerEnter.GetComponentInParent<Annotation> ().myAnnotationListEntry) {
			return true;
		}
		if (currentActiveScreen == ActiveScreen.list) {
			jumpToListEntry (data.pointerEnter);
		} else {
			if (currentAnnotationListEntry != null || currentActiveScreen == ActiveScreen.add) {
				closeAnnotationScreen ();
			}
			EditAnnotation (data.pointerEnter.GetComponentInParent<Annotation> ().myAnnotationListEntry);	
		}
		return false;
	}

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
		Destroy (previewAnnotation);
		previewAnnotation = null;
		annotationListEntryList = new List<GameObject> ();
		updatePatientAnnotationList ();
	}

	// deactivates all Annotations
	public void clearAll ()
	{
		setAllAnnotationsActive (false);
		currentAnnotationListEntry = null;
		//Delete hoverAnnotation
		Destroy (previewAnnotation);
		previewAnnotation = null;

	}

	//Called by load Method in Patient to Create all Annotations in File
	public void createAnnotation(AnnotationJson annotation) {
		Quaternion rotation = new Quaternion ((float)annotation.RotationX, (float)annotation.RotationY, (float)annotation.RotationZ, (float)annotation.RotationW);
		Quaternion meshRotation = new Quaternion ((float)annotation.MeshRotationX, (float)annotation.MeshRotationY, (float)annotation.MeshRotationZ, (float)annotation.MeshRotationW);
		Vector3 position = new Vector3 ((float)annotation.PositionX, (float)annotation.PositionY, (float)annotation.PositionZ);

		//setup new Annotation as maesh and in List
		GameObject newAnnotation = createAnnotationGroup (annotation.type, rotation, position);
		newAnnotation.GetComponent<Annotation> ().myAnnotationMesh.transform.localRotation = meshRotation;
		newAnnotation.GetComponent<Annotation> ().setLabeText (annotation.Text);

		//Color not empty (Black)
		if (annotation.ColorR != 0.0f || annotation.ColorG != 0.0f || annotation.ColorB != 0.0f) {
			newAnnotation.GetComponent<Annotation> ().changeColor (new Color (annotation.ColorR, annotation.ColorG, annotation.ColorB));
		}
		if(annotation.ScaleX == 0f) {
			newAnnotation.GetComponent<Annotation> ().rescaleMesh (new Vector3 (1f, 1f, 1f));
		} else {
			newAnnotation.GetComponent<Annotation> ().rescaleMesh (new Vector3(annotation.ScaleX, annotation.ScaleY, annotation.ScaleZ));
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
		if(currentAnnotationListEntry == null) {
			setCurrentAnnotation (aListEntry);
		}
		if(currentAnnotationType == AnnotationType.plane) {
			//delete preview
			Destroy(previewAnnotation);
			previewAnnotation = null;
			disableOrgans ();
		} else if(currentAnnotationType == AnnotationType.sphere) {
			//delete preview
			Destroy(previewAnnotation);
			previewAnnotation = null;
			disableOrgans ();
		} else {
			createPreviewAnnotation ();
			previewAnnotation.GetComponent<Annotation> ().changeColor (
				currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().getAnnotationColor ());
			enableOrgans ();
		}
 
		currentAnnotationListEntry.GetComponent<AnnotationListEntry> ().setAnnotationMovementActive (true);
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
