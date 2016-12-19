using UnityEngine;
using System.Collections;

public class Config : MonoBehaviour {

	public bool skipAnimations = false;

	static public Config instance { private set; get; }

	public Config()
	{
		if (instance != null) {
			throw(new System.Exception ("Error: Cannot create more than one instance of Config!"));
		}
		instance = this;
		loadFromCommandLine ();
	}

	void loadFromCommandLine()
	{
		string[] args = System.Environment.GetCommandLineArgs ();

		foreach (string arg in args) {
			if (arg == "--skipAnimations") {
				skipAnimations = true;
			}
		}
	}

	void loadFromFile()
	{
		// TODO
	}

	void saveToFile()
	{
		// TODO
	}
}
