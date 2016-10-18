using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class AnnotationLabel : MonoBehaviour {

	public GameObject textBackground;
	public GameObject myText;
	public InputField myInputField;
	private RectTransform myRect;
	private BoxCollider myBoxCollider;


	// Use this for initialization
	private void initLabel () {
		myRect = this.GetComponent<RectTransform>();
		myBoxCollider = this.gameObject.AddComponent<BoxCollider>();
		myBoxCollider.center = Vector3.zero;
	}

	//Used to set Label when load from file
	public void setLabelText(string newLabel) {
		if(myBoxCollider == null) {
			initLabel ();
		}
		myText.GetComponent<Text> ().text = newLabel;
		updateColliderSize ();
	}

	public String getLabelText() {
		return myText.GetComponent<Text> ().text;
	}

	//Called when you click on Text Label
	public void LabelClicked( PointerEventData eventData ) {
		myInputField.text = myText.GetComponent<Text> ().text;
		myBoxCollider.size = new Vector3(myInputField.gameObject.GetComponent<RectTransform>().rect.width, myInputField.gameObject.GetComponent<RectTransform>().rect.height, 0.1f);
		textBackground.SetActive (false);
		myInputField.gameObject.SetActive (true);
		myInputField.ActivateInputField ();

	}

	// called when vlue in input Field changed
	public void ValueChanged () {
		myText.GetComponent<Text> ().text = myInputField.text;
	}

	//used to dynamicly update collider size
	private void updateColliderSize() {
		//Text updates preferedHeight with delay ... force Update needed
		Canvas.ForceUpdateCanvases ();
		RectOffset padding = textBackground.GetComponent<HorizontalLayoutGroup>().padding;
		myBoxCollider.size = new Vector3 (this.GetComponent<RectTransform>().rect.width, myText.GetComponent<Text>().preferredHeight + padding.top + padding.bottom, 0.1f);
	}

	//Called when User finishs editing Label
	public void  EditingFinished () {
		Debug.LogWarning ("Finished");
		setLabelText (myInputField.text);
		if(this.GetComponentInParent<Annotation> () != null) {
			this.GetComponentInParent<Annotation> ().saveChanges ();
		}
			
		myInputField.gameObject.SetActive (false);
		textBackground.SetActive (true);
		updateColliderSize ();
	}
}
