using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictEntryMultyWord : DictEntry {


	private Dictionary<char,DictEntry> entries;
	public DictEntryMultyWord(){
		entries = new Dictionary<char,DictEntry> ();
	}
	public Dictionary<char,DictEntry> getEntries(){
		return this.entries;
	}

	public override void print (int level = 0)
	{
		string indentation = new string (' ', level);
		foreach (KeyValuePair<char, DictEntry> entry in entries) {
			Debug.Log(indentation + entry.Key );
			entry.Value.print (level + 1);
		}
	}

	public override List<DictEntrySingleWord> getLikelyWords( string prefix, int level = 0 )
	{
		prefix = prefix.ToLower ();
		if (level < prefix.Length) {
			List<DictEntrySingleWord> foundWords = new List<DictEntrySingleWord> ();
			char currentLetter = prefix [level];
			if (entries.ContainsKey (currentLetter)) {
				List<DictEntrySingleWord> subListWords = entries [currentLetter].getLikelyWords (prefix, level + 1);
				foundWords.AddRange (subListWords);
			}
			return foundWords;
		} else {
			return getAllSubWords ();
		}
	}


	public override List<DictEntrySingleWord> getAllSubWords ()
	{
		List<DictEntrySingleWord> foundWords = new List<DictEntrySingleWord> ();
		foreach (KeyValuePair<char, DictEntry> entry in entries) {
			foundWords.AddRange (entry.Value.getAllSubWords ());
		}
		return foundWords;
	}


	public List<DictEntrySingleWord> getSortedLikelyWordsAfterRate(string prefix){
		List<DictEntrySingleWord> temp = this.getLikelyWords (prefix);
		temp.Sort(new DictEntryRateComparable ());
		return temp;
	}

	private class DictEntryRateComparable : IComparer<DictEntrySingleWord>{
		public int Compare(DictEntrySingleWord x, DictEntrySingleWord y){
				int xRate = x.getRate ();
				int yRate = y.getRate ();
				return yRate - xRate;
		}
	}

	public override void insert(string word, int level = 0){
		//Debug.Log ("Test" + level);
		if (word.Length > 0) {
			char currentLetter = char.ToLower(word[level]);
			if (this.entries.ContainsKey (currentLetter)) {
				//Debug.Log ("TestMW: "+ currentLetter +" level: "+level);
				//Debug.Log ("TestMW: " + word);
				this.entries[currentLetter].insert (word, level + 1);
			} else {
				//Debug.Log ("TestSW"+ currentLetter +" level: "+level);
				//Debug.Log ("TestSW-Wort:" + word);
				this.entries.Add (currentLetter, new DictEntrySingleWord (word,this));
			}
		}
	}

	public override void insert(string newWord,string oldWord, int level){
		/* Example: 
		 * Inserted first: "anna" -> MultylineWord with entry a => anna
		 * Inserted secondly: "annanas" -> found entry anna, deleted it, insert new MultylineWord-Entries until both words are equal + one,
		 * then insert for both a SingeLineWord-entry
		 */
		//Debug.Log ("Level: " + level);
		//Debug.Log ("Length oldWord:" + oldWord.Length);
		//Debug.Log ("Length newWOrld:" + newWord.Length);

		if (level < newWord.Length & level < oldWord.Length) {
			//Debug.Log ("InsertChange: " + "NewWorld - " + newWord + "=>" + newWord [level] + "........." + "OldWorld - " + oldWord + "=>" + oldWord [level]);
			if (char.ToLower(newWord [level]).CompareTo (char.ToLower(oldWord [level])) == 0) {
				char currentLetter = char.ToLower(oldWord [level]);
				this.entries.Add (currentLetter, new DictEntryMultyWord ());
				this.entries [currentLetter].insert (newWord, oldWord, level + 1);
			} else {
				this.entries.Add (char.ToLower(oldWord [level]), new DictEntrySingleWord (oldWord, this));
					this.entries.Add (char.ToLower(newWord [level]), new DictEntrySingleWord (newWord, this));
			}
		}else if (level >= oldWord.Length) {
			this.entries.Add (char.ToLower(oldWord [level-1]), new DictEntrySingleWord (oldWord, this));
			//Debug.Log ("Test1");
			if (level < newWord.Length) {
				//Debug.Log ("Test2");
				this.entries.Add (char.ToLower(newWord [level]), new DictEntrySingleWord (newWord, this));
			}
		}else if (level >= newWord.Length) {
			//Debug.Log ("Test3");
			this.entries.Add (char.ToLower(newWord [level-1]), new DictEntrySingleWord (newWord, this));
			if (level < oldWord.Length) {
				//Debug.Log ("Test4");
				this.entries.Add (char.ToLower(oldWord [level]), new DictEntrySingleWord (oldWord, this));
			}
		}


	}
}
