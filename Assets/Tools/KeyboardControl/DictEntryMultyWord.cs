using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Has a private Dictionary with char as key and DictEntry as value.
 */
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

	//get all likely words sorted by the rate.
	public List<DictEntrySingleWord> getSortedLikelyWordsAfterRate(string prefix){
		List<DictEntrySingleWord> temp = this.getLikelyWords (prefix);
		temp.Sort(new DictEntryRateComparable ());
		return temp;
	}

	//Compare's the rate between two word's
	private class DictEntryRateComparable : IComparer<DictEntrySingleWord>{
		public int Compare(DictEntrySingleWord x, DictEntrySingleWord y){
				int xRate = x.getRate ();
				int yRate = y.getRate ();
				return yRate - xRate;
		}
	}

	public override DictEntrySingleWord insert(string word,int rate = 0,int level = 0){
		//Debug.Log ("Test" + level);
		if (word.Length > 0) {
			char currentLetter = char.ToLower(word[level]);
			if (this.entries.ContainsKey (currentLetter)) {
				//Debug.Log ("TestMW: "+ currentLetter +" level: "+level);
				//Debug.Log ("TestMW: " + word);
				return this.entries[currentLetter].insert (word,rate, level + 1);
			} else {
				//Debug.Log ("TestSW"+ currentLetter +" level: "+level);
				//Debug.Log ("TestSW-Wort:" + word);
				DictEntrySingleWord single = new DictEntrySingleWord (word,rate,this);
				this.entries.Add (currentLetter,single );
				return single;
			}
		}
		return null;
	}
	/*
	 * This method is used, if a new word is inserted and found a single word, e.g. first insert: "Anna", secondly insert: "Annies"
	 * The internal has to change from one DictEntrySingleWord to One MultyLineWord and two SingleLineWords
	 */ 
	public override DictEntrySingleWord insert(string newWord,int newRate,string oldWord,int oldRate, int level){
		/* Example: 
		 * Inserted first: "anna" -> MultylineWord with entry a => anna
		 * Inserted secondly: "annanas" -> found entry anna, deleted it, insert new MultylineWord-Entries until both words are equal + one,
		 * then insert for both a SingleLineWord-entry
		 * at the end return the new SinglelineWord-Entry
		 */
		//Debug.Log ("Level: " + level);
		//Debug.Log ("Length oldWord:" + oldWord.Length);
		//Debug.Log ("Length newWOrld:" + newWord.Length);


		if (level < newWord.Length & level < oldWord.Length) {
			//Debug.Log ("InsertChange: " + "NewWorld - " + newWord + "=>" + newWord [level] + "........." + "OldWorld - " + oldWord + "=>" + oldWord [level]);
			//If both current letters (newWord and oldWord) are equal, add a new DictEntryMultiWord
			if (char.ToLower(newWord [level]).CompareTo (char.ToLower(oldWord [level])) == 0) {
				char currentLetter = char.ToLower(oldWord [level]);
				this.entries.Add (currentLetter, new DictEntryMultyWord ());
				return this.entries [currentLetter].insert (newWord,newRate, oldWord,oldRate, level + 1);
			} else {
				//if both current letters are not equal add for each a new DictEntrySingleWord and return the new one
				this.entries.Add (char.ToLower(oldWord [level]), new DictEntrySingleWord (oldWord,oldRate, this));
				DictEntrySingleWord newSingleEntry = new DictEntrySingleWord (newWord, newRate, this);
				this.entries.Add (char.ToLower(newWord [level]), newSingleEntry);
				return newSingleEntry;
			}
		/*If the new word is bigger than the old world, 
		 * add DictEntrySingleWord for the oldWord one level back
		 * and add a new DictEntrySingleWord for the new Word at the same level
		 */
		}else if (level >= oldWord.Length) {
			this.entries.Add (char.ToLower(oldWord [level-1]), new DictEntrySingleWord (oldWord,oldRate, this));
			//Debug.Log ("Test1");
			if (level < newWord.Length) {
				//Debug.Log ("Test2");
				DictEntrySingleWord newSingleEntry = new DictEntrySingleWord (newWord, newRate, this);
				this.entries.Add (char.ToLower(newWord [level]),newSingleEntry);
				return newSingleEntry;
			}
		}else if (level >= newWord.Length) {
		/*If the old word is bigger than the new world, 
		 * add DictEntrySingleWord for the new Word one level back
		 * and add a new DictEntrySingleWord for the old Word at the same level
		 */
			//Debug.Log ("Test3");
			if (level < oldWord.Length) {
				//Debug.Log ("Test4");
				this.entries.Add (char.ToLower(oldWord [level]), new DictEntrySingleWord (oldWord,oldRate, this));
			}
			DictEntrySingleWord newSingleEntry = new DictEntrySingleWord (newWord, newRate, this);
			this.entries.Add (char.ToLower(newWord [level-1]), newSingleEntry);
			return newSingleEntry;
		}
		return null;
	}
}
