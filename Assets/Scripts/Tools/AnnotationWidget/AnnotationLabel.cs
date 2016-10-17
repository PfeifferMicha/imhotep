using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AnnotationLabel : MonoBehaviour {

	public GameObject textBackground;
	public GameObject myText;
	public string text = " ";
	public InputField myInputField;
	private RectTransform myRect;
	private BoxCollider myBoxCollider;

	// Use this for initialization
	void initLabel () {
		myText.GetComponent<Text> ().text = text;
		myRect = this.GetComponent<RectTransform>();
		if (myRect != null)
		{
			myBoxCollider = this.gameObject.AddComponent<BoxCollider>();
			myBoxCollider.center = Vector3.zero;
		}
		else
		{
			Debug.LogError("No rect trnasform found");
		}

	}

	//Used to set Label when load from file
	public void setLabel(string newLabel) {
		if(myBoxCollider == null) {
			initLabel ();
		}
		myText.GetComponent<Text> ().text = newLabel;
		text = newLabel;
		updateColliderSize ();
	}

	//Called when you click on Text Label
	public void LabelClicked( PointerEventData eventData ) {
		myInputField.text = myText.GetComponent<Text> ().text;
		myBoxCollider.size = new Vector3(0.0f, 0.0f, 0.0f);
		textBackground.SetActive (false);
		myInputField.gameObject.SetActive (true);
		myInputField.ActivateInputField ();

	}

	// called when vlue in input Field changed
	public void ValueChanged () {
		text = myInputField.text;
		myText.GetComponent<Text> ().text = text;
	}

	//used to dynamicly update collider size
	private void updateColliderSize() {
		//Text updates preferedHeight with delay ... force Update needed
		Canvas.ForceUpdateCanvases ();
		myBoxCollider.size = new Vector3 (this.GetComponent<RectTransform>().rect.width, myText.GetComponent<Text>().preferredHeight, 0.1f);
	}

	//Called when User finishs editing Label
	public void EditingFinished () {
		Debug.LogWarning ("Finished");
		setLabel (myInputField.text);
		if(this.GetComponentInParent<Annotation> () != null) {
			this.GetComponentInParent<Annotation> ().saveChanges ();
		}

		myInputField.gameObject.SetActive (false);
		textBackground.SetActive (true);
		updateColliderSize ();
	}
}
