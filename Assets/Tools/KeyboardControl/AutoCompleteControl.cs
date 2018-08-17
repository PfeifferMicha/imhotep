using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class AutoCompleteControl : MonoBehaviour, IEnteredText{
	//When insert a new word?
	/* Wort: Ende/Anfangsmarker: "Leerzeichen"; ","; "."; "!"; "?"; ";" 
	 * 
	 */
	private string[] seperator = {" ",",",".","!","?",";"};
	public InputField input;
	public Text suggestionTop;
	public Text suggestionMiddle;
	public Text suggestionBottom;

	//Dictionary for Autocomplete
	DictEntryMultyWord autoCompleteDic = new DictEntryMultyWord ();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		List<DictEntrySingleWord> tempLikelyWords = new List<DictEntrySingleWord>();
		/*<DictEntrySingleWord> stringlist = autoCompleteDic.getSortedLikelyWordsAfterRate("");
		foreach (DictEntrySingleWord s in stringlist)
			Debug.Log( "Words: " + s.getWord() );
		*/
		string[] words = this.getWordsFromInput (this.input.text);
		if (words!=null & words.Length>0) tempLikelyWords = autoCompleteDic.getLikelyWords (words [words.Length - 1]);
		string[] suggest = new string[3];
		DictEntrySingleWord[] suggestArray =  tempLikelyWords.ToArray ();
		for (int i = 0; i < Mathf.Min(3,suggestArray.Length); i++) {
			suggest [i] = suggestArray [i].getWord(); 
		}
		suggestionTop.text = suggest [0];
		suggestionMiddle.text = suggest [1];
		suggestionBottom.text = suggest [2];
	}



	public void enteredText(string text){
		string[] words = this.getWordsFromInput (text);
		if (words != null) {
			for (int i = 0; i < words.Length; i++) {
				autoCompleteDic.insert (words [i]);
			}		
		}
		/*List<DictEntrySingleWord> stringlist = autoCompleteDic.getSortedLikelyWordsAfterRate("");
		foreach (DictEntrySingleWord s in stringlist)
			Debug.Log( "Words: " + s.getWord() );
		*/
	}

	public void applySuggestion(){
		GameObject selected = EventSystem.current.currentSelectedGameObject;
		if (selected != null) {
			string suggestionText = selected.GetComponentInChildren<Text> ().text;
			int maxlength = suggestionText.Length;
			int maxLengthInput = input.text.Length;
			int startIndex = (maxLengthInput - maxlength) > 0 ? (maxLengthInput - maxlength) : 0; 
			string textToSearch = this.input.text.Substring (startIndex);
			bool foundPartOfSuggestionText = false;
			int length = maxlength;
			while (!foundPartOfSuggestionText & length>=0){
				string tempCompare = suggestionText.Substring (0,length);
				int index = textToSearch.IndexOf (tempCompare);
				if (index != -1 & maxLengthInput>(maxLengthInput - maxlength + index)) {
					foundPartOfSuggestionText = true;
					this.input.text = this.input.text.Remove (startIndex + index);
					this.input.text += suggestionText;
				} else {
					length--;
				}
			}
		}
		this.reFocusKeyboardInputfield ();
	}
	private string[] getWordsFromInput(string text){
		if (text != null) {			
			string[] words = text.Split (seperator, System.StringSplitOptions.RemoveEmptyEntries);
			return words;
		}
		return null;
	}

	//Set the focus back to the keyboard-Inputfield and deselect's the text
	private void reFocusKeyboardInputfield(){
		EventSystem.current.SetSelectedGameObject (this.input.gameObject);
		StartCoroutine (this.waitForFrame ());
	}
	//used to deselect the text of keyboard-Inputfield
	private IEnumerator waitForFrame(){		
		yield return 0;
		this.input.MoveTextEnd (true);
	}
}