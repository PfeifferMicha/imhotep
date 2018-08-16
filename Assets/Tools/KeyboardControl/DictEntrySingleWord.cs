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
		} else {
			List<string> temp = new List<string> ();
			temp.Add (word);
			temp.Add (this.word);
			char currentLetter = word [level-1];
			this.previous.getEntries ().Remove (currentLetter);
			this.previous.getEntries ().Add(currentLetter, new DictEntryMultyWord ());
			this.previous.getEntries () [currentLetter].insert (temp, level);
		}
	}

	public override void insert(List<string> word, int level = 0){
		
	}
	public int getRate(){
		return this.rate;
	}
}
