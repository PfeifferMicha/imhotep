using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class AnnotationLabel : MonoBehaviour {

	public Image textBackground;
	public Text myText;
	public InputField myInputField;
	public BoxCollider myBoxCollider;
	//Padding to top + bot edges of background
	private float padding = 0.0f;

	//Used to set Label when load from file
	public void setLabelText(string newLabel) {
		myText.text = newLabel;
		myInputField.text = newLabel;
		resizeLabel ();
	}

	public String getLabelText() {
		return myText.text;
	}

	//Called when you click on Text Label
	public void LabelClicked( PointerEventData eventData ) {
		Debug.Log ("Clicked");
		myInputField.gameObject.SetActive (true);
		myInputField.ActivateInputField ();
		myInputField.Select();
		myInputField.MoveTextEnd (true);
		textBackground.color = new Color (textBackground.color.r, textBackground.color.b, textBackground.color.b, 0.0f);
		myText.color = new Color (myText.color.r, myText.color.b, myText.color.b, 0.0f);


	}

	// called when vlue in input Field changed
	public void ValueChanged () {
		Debug.Log ("ValueChanged");
		myText.text = myInputField.text;
		this.GetComponentInParent<Annotation> ().saveLabelChanges ();
		resizeLabel ();
	}

	//Called when User finishs editing Label
	public void  EditingFinished () {
		Debug.LogWarning ("Finished");
		myInputField.gameObject.SetActive (false);
		textBackground.color = new Color (textBackground.color.r, textBackground.color.b, textBackground.color.b, 1.0f);
		myText.color = new Color (myText.color.r, myText.color.b, myText.color.b, 1.0f);
	}

	private void resizeLabel() {
		if(padding == 0.0f) {
			padding = textBackground.gameObject.GetComponent<VerticalLayoutGroup> ().padding.top
				+ textBackground.gameObject.GetComponent<VerticalLayoutGroup> ().padding.bottom;
		}

		//calc height
		Canvas.ForceUpdateCanvases();
		float newHeight = myText.preferredHeight + padding;
		Vector2 resize = new Vector2 (this.gameObject.GetComponent<RectTransform> ().rect.width, newHeight);
		//resize objects
		this.gameObject.GetComponent<RectTransform>().sizeDelta = resize;
		myBoxCollider.size = new Vector3(myBoxCollider.size.x, newHeight, myBoxCollider.size.z);
		myInputField.gameObject.GetComponent<RectTransform> ().sizeDelta = resize;
	}
}
