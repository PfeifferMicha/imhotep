using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Represents a single word with a rate. The rate will be increased if the user entered the word again.
 */
public class DictEntrySingleWord : DictEntry {

	private string word;
	private int rate;
	private DictEntryMultyWord previous;

	public DictEntrySingleWord(string word,int rate,DictEntryMultyWord previous){
		this.rate = rate;
		this.word = word;
		this.previous = previous;
	}
	public string getWord(){
		return this.word;
	}


	public override void print (int level = 0)
	{
		string indentation = new string (' ', level);
		Debug.Log (indentation + word);
	}

	public override List<DictEntrySingleWord> getLikelyWords( string prefix, int level = 0 )
	{
		if (this.word.ToLower().StartsWith (prefix.ToLower())) {
			//Wenn das eingegebene Wort identisch mit Dictionary-Wort, erhöhe Häufigkeit(rate) um eins
			if (this.word.Equals (prefix)) {
				this.rate++;
			}
			return getAllSubWords ();
		}
		return new List<DictEntrySingleWord> ();
	}
	public override List<DictEntrySingleWord> getAllSubWords ()
	{
		List<DictEntrySingleWord> result = new List<DictEntrySingleWord> ();
		result.Add (this);
		return result;
	}

	public override DictEntrySingleWord insert(string word,int newRate = 0, int level = 0){
		//Debug.Log ("Test");
		if (word.CompareTo (this.word) == 0) {
			this.rate++;
			return this;
			//Debug.Log ("Rate: "+rate +" of "+word);
		} else {
			char currentLetter = char.ToLower(word [level-1]);
			//Debug.Log ("CurrentLetter-SingleWord: " + currentLetter);
			this.previous.getEntries ().Remove (currentLetter);
			this.previous.getEntries ().Add(currentLetter, new DictEntryMultyWord ());
			return this.previous.getEntries () [currentLetter].insert (word,newRate,this.word,this.rate,level);
		}
	}

	public override DictEntrySingleWord insert(string newWord,int newRate,string oldWord,int oldRate, int level){
		return null;
	}

	public int getRate(){
		return this.rate;
	}
}
