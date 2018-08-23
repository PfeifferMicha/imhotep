using System.Collections;
using System.Collections.Generic;

/*
 * Superclass of DictEntryMultiyWord and DictEntrySingleWord
 * 
 */
public abstract class DictEntry {

	//prints the whole dictionary
	public abstract void print (int level = 0);
	//Get all likelyWords
	public abstract List<DictEntrySingleWord> getLikelyWords (string prefix, int level = 0);
	//Get all subwords of this branch
	public abstract List<DictEntrySingleWord> getAllSubWords ();
	//insert's a new word
	public abstract DictEntrySingleWord insert (string word,int rate = 0, int level = 0);
	//Never use this method to insert a word, use only: "insert(string word,int level = 0)
	public abstract DictEntrySingleWord insert(string newWord,int newRate,string oldWord,int oldRate, int level);

}
