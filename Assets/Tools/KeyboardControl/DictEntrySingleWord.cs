using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictEntrySingleWord : DictEntry {

	private string word;
	public DictEntrySingleWord(string word){
		this.word = word;
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
		if (this.word.StartsWith(prefix))
			return getAllSubWords();
		return new List<DictEntrySingleWord> ();
	}
	public override List<DictEntrySingleWord> getAllSubWords ()
	{
		List<DictEntrySingleWord> result = new List<DictEntrySingleWord> ();
		result.Add (this);
		return result;
	}
}
