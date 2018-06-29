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

}
