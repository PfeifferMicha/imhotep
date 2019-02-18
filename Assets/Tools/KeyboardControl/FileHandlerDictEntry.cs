using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class FileHandlerDictEntry : MonoBehaviour {

	private static string path = "../tco-ImhotepData/Keyboard/databaseDicEntries.txt";
	// Use this for initialization
	void Start () {}

	private static void createPathIfNotExists(){
		if (!Directory.Exists ("../tco-ImhotepData/Keyboard")) {
			Directory.CreateDirectory ("../tco-ImhotepData/Keyboard");
		}
	}
	//Write's a complete new File
	public static void  write(DictEntry dicEntry){
		createPathIfNotExists ();
		StreamWriter writer = new StreamWriter(path,false);
		if (dicEntry is DictEntrySingleWord) {
			DictEntrySingleWord entry = ((DictEntrySingleWord)dicEntry);
			writer.WriteLine (entry.getWord () + "," + entry.getRate ());
		} else {
			List<DictEntrySingleWord> list = ((DictEntryMultyWord)dicEntry).getAllSubWords ();
			foreach (DictEntrySingleWord entry in list) {
				writer.WriteLine (entry.getWord () + "," + entry.getRate ());
			}
		}
		writer.Close ();

	}
	//Add's a new Entry add the end of the file
	public static void  write(DictEntrySingleWord dicEntry){
		createPathIfNotExists ();
		StreamWriter writer = new StreamWriter(path,true);
		if (dicEntry is DictEntrySingleWord) {
			DictEntrySingleWord entry = ((DictEntrySingleWord)dicEntry);
			writer.WriteLine (entry.getWord () + "," + entry.getRate ());
		}
		writer.Close ();
	}
	public static DictEntryMultyWord read(){
		createPathIfNotExists ();
		DictEntryMultyWord entry = new DictEntryMultyWord ();
		try{
			StreamReader reader = new StreamReader (path);
			while (!reader.EndOfStream) {
				string[] line = reader.ReadLine ().Split(',');
				entry.insert (line [0], int.Parse(line [1]));
			}
			reader.Close ();
			return entry;
		}catch (FileNotFoundException e){
			return null;
		}
	}
}
