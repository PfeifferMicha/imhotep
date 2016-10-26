using UnityEngine;
using System.Collections;

public class Logo : MonoBehaviour {

	// TODO: Probably move this, or handle it some other way.
	// It's kind of ugly that the Logo starts the UI ?!
	public GameObject PatientSelector;
	void activatePatientSelector () {
		PatientSelector.SetActive (true);
	}
}
