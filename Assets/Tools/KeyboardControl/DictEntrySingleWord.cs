using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictEntrySingleWord : DictEntry {

	private string word;
	private int rate;
	private DictEntryMultyWord previous;
	public DictEntrySingleWord(string word,DictEntryMultyWord previous){
		int rate = 0;
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
		if (this.word.StartsWith (prefix)) {
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

	public override void insert(string word, int level = 0){
		//Debug.Log ("Test");
		if (word.CompareTo (this.word) == 0) {
			this.rate++;
			//Debug.Log ("Rate: "+rate +" of "+word);
		} else {
			char currentLetter = char.ToLower(word [level-1]);
			//Debug.Log ("CurrentLetter-SingleWord: " + currentLetter);
			this.previous.getEntries ().Remove (currentLetter);
			this.previous.getEntries ().Add(currentLetter, new DictEntryMultyWord ());
			this.previous.getEntries () [currentLetter].insert (word,this.word, level);
		}
	}

	public override void insert(string newWord,string oldWord, int level){}

	public int getRate(){
		return this.rate;
	}
}
