using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCompleteDictionary : MonoBehaviour {

	Dictionary<char,DictEntry> entries;
	// Use this for initialization
	void Start () {
		entries = new Dictionary<char,DictEntry> ();
		entries.Add('a', new DictEntry());
		entries.Add('b', new DictEntry());
		entries.Add('c', new DictEntry());
		entries.Add('d', new DictEntry());
		entries.Add('e', new DictEntry());
		entries.Add('f', new DictEntry());
		entries.Add('g', new DictEntry());
		entries.Add('h', new DictEntry());
		entries.Add('i', new DictEntry());
		entries.Add('j', new DictEntry());
		entries.Add('k', new DictEntry());
		entries.Add('l', new DictEntry());
		entries.Add('m', new DictEntry());
		entries.Add('n', new DictEntry());
		entries.Add('o', new DictEntry());
		entries.Add('p', new DictEntry());
		entries.Add('q', new DictEntry());
		entries.Add('r', new DictEntry());
		entries.Add('s', new DictEntry());
		entries.Add('t', new DictEntry());
		entries.Add('u', new DictEntry());
		entries.Add('v', new DictEntry());
		entries.Add('w', new DictEntry());
		entries.Add('x', new DictEntry());
		entries.Add('y', new DictEntry());
		entries.Add('z', new DictEntry());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public List<string> getLikelyWords(string input){
		List<string> likelyWords = new List<string>();
		//input in zeichen zerlegen
		char[] inputZeichen = input.ToCharArray();
		//für jedes Zeichen Dictionary abwandern
		for (int i = 0; i < inputZeichen; i++) {
			char temp = inputZeichen [i];
			if (entries.ContainsKey (temp)) {
				DictEntry tempEntry= entries [temp];
				if (tempEntry is DictEntryMultyWord) {
					//return getLikelyWords (input);
				}else if(tempEntry is DictEntrySingleWord){
					likelyWords.Add (((DictEntrySingleWord)tempEntry).getWord ());
				}
			}
		}
		return null;
	}

	private List<string> getLikelyWords(char[] inputZeichen,int signPosition){

		char temp = inputZeichen [signPosition];
		if (entries.ContainsKey (temp)) {
			DictEntry tempEntry= entries [temp];
			if (tempEntry is DictEntryMultyWord) {
				return getLikelyWords (inputZeichen);
			}else if(tempEntry is DictEntrySingleWord){
				likelyWords.Add (((DictEntrySingleWord)tempEntry).getWord ());
			}
		}

	}
}
