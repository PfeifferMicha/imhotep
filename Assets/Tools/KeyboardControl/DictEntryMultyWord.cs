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
		//string[] insertWord = word.ToLower ().ToCharArray ();
		char currentLetter = word[level];

		if (entries.ContainsKey (currentLetter)) {
			Debug.Log ("TestMW: "+ currentLetter);
			Debug.Log ("TestMW: " + word);
			//if (this.entries [currentLetter] is DictEntrySingleWord) {
				
			//	this.entries[word[level-1]].insert(			
			//}
			entries[currentLetter].insert (word, level + 1);

		} else {
			Debug.Log ("TestSW"+ currentLetter);
			Debug.Log ("TestSW-Wort:" + word);

			this.entries.Add (currentLetter, new DictEntrySingleWord (word,this));
			//this.entries.Add(
		}
	}

	public override void insert(List<string> word, int level = 0){
		Debug.Log ("tttttt");
	}
}
