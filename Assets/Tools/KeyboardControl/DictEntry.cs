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
	public abstract void insert (string word, int level = 0);
	//Never use this method to insert a word, use only: "insert(string word,int level = 0)
	public abstract void insert(string newWord,string oldWord, int level);

}
