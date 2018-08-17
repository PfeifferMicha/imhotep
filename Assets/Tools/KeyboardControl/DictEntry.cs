using System.Collections;
using System.Collections.Generic;


public abstract class DictEntry {

	public abstract void print (int level = 0);
	public abstract List<DictEntrySingleWord> getLikelyWords (string prefix, int level = 0);
	public abstract List<DictEntrySingleWord> getAllSubWords ();
	public abstract void insert (string word, int level = 0);
	//Never use this method to insert a word, use only: "insert(string word,int level = 0)
	public abstract void insert(string newWord,string oldWord, int level);

}
