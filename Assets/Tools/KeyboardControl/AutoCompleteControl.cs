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
	public KeyboardControl keyboardControl;
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
		int lastSymbolIndex = this.input.text.Length;
		//Debug.Log ("lastSymbolIndex:" + lastSymbolIndex);
		//Debug.Log("lastSysmbol: "+this.input.text.Substring(lastSymbolIndex-1));
		bool isLastSymbolSeperator = false;
		for (int i = 0; i < this.seperator.Length; i++) {
			//Debug.Log (this.input.text.Substring(lastSymbolIndex-1).CompareTo(this.seperator[i])==0);
			if (lastSymbolIndex>0 & this.input.text.Substring(lastSymbolIndex-1).CompareTo(this.seperator[i])==0) {
				isLastSymbolSeperator = true;
				break;
			}
		}
		if (!isLastSymbolSeperator) {			
			string[] words = this.getWordsFromInput (this.input.text);
			if (words != null & words.Length > 0)
				tempLikelyWords = autoCompleteDic.getSortedLikelyWordsAfterRate (words [words.Length - 1]);
			string[] suggest = new string[3];
			DictEntrySingleWord[] suggestArray = tempLikelyWords.ToArray ();
			for (int i = 0; i < Mathf.Min (3, suggestArray.Length); i++) {
				suggest [i] = suggestArray [i].getWord (); 
			}
			suggestionTop.text = suggest [0];
			if (suggestionTop.text.Length == 0) {
				suggestionTop.gameObject.SetActive (false);
			} else {
				suggestionTop.gameObject.SetActive (true);
			}
			suggestionMiddle.text = suggest [1];
			if (suggestionMiddle.text.Length == 0) {
				suggestionMiddle.gameObject.SetActive (false);
			}else {
				suggestionMiddle.gameObject.SetActive (true);
			}
			suggestionBottom.text = suggest [2];
			if (suggestionBottom.text.Length == 0) {
				suggestionBottom.gameObject.SetActive (false);
			}else {
				suggestionBottom.gameObject.SetActive (true);
			}
		} else {
			suggestionTop.text = "";
			suggestionMiddle.text = "";
			suggestionBottom.text = "";
		}

	}

	//Interface method, called by the keyboard
	public void enteredText(string text){
		string[] words = this.getWordsFromInput (text);
		if (words != null) {
			for (int i = 0; i < words.Length; i++) {
				autoCompleteDic.insert (words [i]);
			}		
		}
		//TestDictionary t = new TestDictionary ();
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
			int lastIndex = 0;
			for (int i = 0; i < seperator.Length; i++) {
				lastIndex = Mathf.Max(lastIndex,input.text.LastIndexOf(seperator[i]));
			}
			if (lastIndex > 0)
				lastIndex++;
			//Debug.Log("LastIndexOF:"+lastIndex);
			this.keyboardControl.deleteText (lastIndex);
			this.keyboardControl.enterTextEvent (suggestionText);
		}
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
}