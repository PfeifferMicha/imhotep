using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCompleteDictionary : MonoBehaviour {

	DictEntry entries;
	public AutoCompleteDictionary(){
		entries = new DictEntryMultyWord ();
		entries.insert ("Anna");
		entries.insert ("Anton");
		entries.insert ("Ananas");
		entries.insert ("Annies");
		entries.insert ("Zeit");
		entries.print ();
		string stringlist = "";
		entries.getLikelyWords ("Anna");
		entries.getLikelyWords ("Anna");
		entries.getLikelyWords ("Anna");
		entries.getLikelyWords ("Anna");

		entries.getLikelyWords ("Ananas");
		entries.getLikelyWords ("Ananas");
		entries.getLikelyWords ("Ananas");
		entries.getLikelyWords ("Zeit");


		foreach (DictEntrySingleWord s in ((DictEntryMultyWord)entries).getSortedLikelyWordsAfterRate(""))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "All Words: " + stringlist );
		/*foreach (DictEntrySingleWord s in entries.getAllSubWords())
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "All Words: " + stringlist );


		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "A" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for A: " + stringlist );


		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "An" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for An: " + stringlist );

		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "Ant" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for Ant: " + stringlist );

		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "Anto" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for Anto: " + stringlist );

		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "Anta" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for Anta: " + stringlist );


		stringlist = "";
		foreach (DictEntrySingleWord s in entries.getLikelyWords( "Ze" ))
			stringlist = stringlist + s.getWord() + " ";
		Debug.Log( "Found for Ze: " + stringlist );*/
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}



}
