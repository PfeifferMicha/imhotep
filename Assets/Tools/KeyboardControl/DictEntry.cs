using System.Collections;
using System.Collections.Generic;


public abstract class DictEntry {

	public abstract void print (int level = 0);
	public abstract List<DictEntrySingleWord> getLikelyWords (string prefix, int level = 0);
	public abstract List<DictEntrySingleWord> getAllSubWords ();
	public abstract void insert (string word, int level = 0);
	public abstract void insert(List<string> words, int level);

	/*public void insert(string word){
		word = word.ToLower ();
		char[] inputSigns = word.ToCharArray ();
		DictEntry dicTemp = this;
		DictEntry previousDicTemp = this;
		for (int i = 0; i < inputSigns.Length; i++) {
			Dictionary<char,DictEntry> temp;
			if (dicTemp != null) {				
				if (dicTemp is DictEntryMultyWord) {//Mehrere Einträge: Anschauen im nächsten Lauf (Pointer auf Dictionary setzen)
					temp = ((DictEntryMultyWord)dicTemp).getEntries ();
					//Enthält Buchstabe im Wörterbuch?
					if (!temp.ContainsKey (inputSigns [i])) { //Nein...
						temp [inputSigns [i]] = new DictEntrySingleWord (word);
						break;
					}
					//Vorgänger merken
					previousDicTemp = dicTemp;
					//Zeiger weitersetzen
					dicTemp = temp [inputSigns [i]];
				} 
				else if (dicTemp is DictEntrySingleWord) {//Einzelner Eintrag: Auf Gleichheit der einzelnen Buchstaben überprüfen und anpassen für's Einfügen
					string tempWord = ((DictEntrySingleWord)dicTemp).getWord ();
					char[] tempWordSigns = tempWord.ToCharArray ();
					//Ändern zum MultyWord Entry
					temp = ((DictEntryMultyWord)previousDicTemp).getEntries ();
					temp[inputSigns [i - 1]] = new DictEntryMultyWord ();
					for (int j = i; j < tempWordSigns.Length; j++) {
						if (inputSigns [j].Equals (tempWordSigns [j])) {
							//Neuen leeren MultiWord-Eintrag mit identischen Buchstaben als Key einfügen
							((DictEntryMultyWord)temp [inputSigns [j-1]]).getEntries ().Add (inputSigns [j], new DictEntryMultyWord ());
							//Zeiger neusetzen
							temp = ((DictEntryMultyWord)temp [inputSigns [j - 1]]).getEntries ();
						} else {
							//Beide Wörter neu Einfügen
							((DictEntryMultyWord)temp [inputSigns [j - 1]]).getEntries ().Add (inputSigns [j], new DictEntrySingleWord (word));
							((DictEntryMultyWord)temp [inputSigns [j - 1]]).getEntries ().Add (tempWordSigns [j], new DictEntrySingleWord (tempWord));
							break;
						}
					}
					break;
				}
			}
		}
	}*/
}
