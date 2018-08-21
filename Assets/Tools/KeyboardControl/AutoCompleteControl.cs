using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/*
 * Responsible for inserting new Words in the Dictionary, but also for applying autocomplete-Suggestion selected by the user.
 * It's uses an Interface (IEnteredText) to get the saved text by the user in order to insert new words.
 * The Keyboard is the Caller.
 */
public class AutoCompleteControl : MonoBehaviour, IEnteredText{
	//When insert a new word?
	/* Wort: Ende/Anfangsmarker: "Leerzeichen"; ","; "."; "!"; "?"; ";" 
	 * 
	 */
	//Seperation signs for marking words
	private string[] seperator = {" ",",",".","!","?",";"};
	//Inputfield of the keyboard
	public InputField input;

	//Contains the possible suggestions
	public Text suggestionTop;
	public Text suggestionMiddle;
	public Text suggestionBottom;

	//Dictionary for Autocomplete
	DictEntryMultyWord autoCompleteDic = new DictEntryMultyWord ();
	// Use this for initialization 
	void Start () {
		
	}
	
	// Update is called once per frame
	//Searching the text for words in order to find likely matches
	void Update () {
		List<DictEntrySingleWord> tempLikelyWords = new List<DictEntrySingleWord>();
		/*<DictEntrySingleWord> stringlist = autoCompleteDic.getSortedLikelyWordsAfterRate("");
		foreach (DictEntrySingleWord s in stringlist)
			Debug.Log( "Words: " + s.getWord() );
		*/
		string[] words = this.getWordsFromInput (this.input.text);
		if (words!=null & words.Length>0) tempLikelyWords = autoCompleteDic.getSortedLikelyWordsAfterRate (words [words.Length - 1]);
		string[] suggest = new string[3];
		DictEntrySingleWord[] suggestArray =  tempLikelyWords.ToArray ();
		for (int i = 0; i < Mathf.Min(3,suggestArray.Length); i++) {
			suggest [i] = suggestArray [i].getWord(); 
		}

		suggestionTop.text = suggest [0];
		suggestionMiddle.text = suggest [1];
		suggestionBottom.text = suggest [2];

	}

	//Interface method, called by the keyboard
	public void enteredText(string text){
		string[] words = this.getWordsFromInput (text);
		if (words != null) {
			for (int i = 0; i < words.Length; i++) {
				autoCompleteDic.insert (words [i]);
			}		
		}
		TestDictionary t = new TestDictionary ();
		/*List<DictEntrySingleWord> stringlist = autoCompleteDic.getSortedLikelyWordsAfterRate("");
		foreach (DictEntrySingleWord s in stringlist)
			Debug.Log( "Words: " + s.getWord() );
		*/
	}

	//Applie's the suggestion-word in the inputfield of the keyboard, selected by the user
	public void applySuggestion(){
		GameObject selected = EventSystem.current.currentSelectedGameObject;
		if (selected != null) {
			//selected suggested-word
			string suggestionText = selected.GetComponentInChildren<Text> ().text;
			int maxlength = suggestionText.Length;
			//whole length of Inputfield - text
			int maxLengthInput = input.text.Length;
			//Idea: go to the end of the text minus the length of the suggested-word
			int startIndex = (maxLengthInput - maxlength) > 0 ? (maxLengthInput - maxlength) : 0; 
			string textToSearch =  this.input.text.Substring (startIndex).ToLower();
			bool foundPartOfSuggestionText = false;
			int length = maxlength;
			/*Motivation: Have to findout how many letters of the suggested-word are already entered 
			* in order to insert the suggested-word at the right position in the inputfield text
			* Approach: Compare the end of the text with parts of the suggested-word; start with the whole word, reduce it every iteration
			*/
			while (!foundPartOfSuggestionText & length>=0){
				string tempCompare = suggestionText.Substring (0,length).ToLower();
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
		//Set the Focus back to the keyboard-Inputfield
		this.reFocusKeyboardInputfield ();
	}
	//Extract all words from a given text
	private string[] getWordsFromInput(string text){
		if (text != null) {			
			string[] words = text.Split (seperator, System.StringSplitOptions.RemoveEmptyEntries);
			//for (int i = 0; i < words.Length; i++) {
				//Debug.Log ("word: " + words [i]);
			//}
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