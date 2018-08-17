using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Superclass of DictEntryMultiyWord and DictEntrySingleWord
 * 
 */
public class TestDictionary {

	public TestDictionary(){
		//this.testMoreEntriesWithSpecialSigns ();
	}

	private void testEmpty(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("");
		Debug.Log ("Leerer Eintrag");
		mw.print ();
		Debug.Log (".........");
	}

	private void testOneEntryOneLetter(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("A");
		Debug.Log ("testOneEntryOneLetter");
		mw.print ();
		Debug.Log (".........");
	}

	private void testOneEntryMoreLetters(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("Anna");
		Debug.Log ("testOneEntryMoreLetters");
		mw.print ();
		Debug.Log (".........");
	}

	private void testTwoEntriesMoreLetters(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("Anna");
		mw.insert ("Annies");
		Debug.Log ("testTwoEntriesMoreLetters");
		mw.print ();
		Debug.Log (".........");
	}

	private void testMoreEntries(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("Anna");
		mw.insert ("Annies");
		mw.insert ("Anke");
		mw.insert ("Ananas");
		mw.insert ("Zeit");
		Debug.Log ("testMoreEntries");
		mw.print ();
		Debug.Log (".........");

	}

	private void testMoreEntriesWithSpecialSigns(){
		DictEntryMultyWord mw = new DictEntryMultyWord ();
		mw.insert ("Anna-Isa");
		mw.insert ("Annies");
		mw.insert ("Anke_H");
		mw.insert ("Ananas");
		mw.insert ("Zeit-Not");
		mw.insert ("Zeit-Krise");
		Debug.Log ("testMoreEntriesWithSpecialSigns");
		mw.print ();
		Debug.Log (".........");
	}
}
	