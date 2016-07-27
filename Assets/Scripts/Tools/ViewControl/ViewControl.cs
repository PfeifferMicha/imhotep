using UnityEngine;
using System.Collections;

public class ViewControl : MonoBehaviour {

	private GameObject mainPane;
	private GameObject editPane;
	// Use this for initialization
	void Start () {
		mainPane = transform.FindChild ("Canvas/MainPane").gameObject;
		editPane = transform.FindChild ("Canvas/EditPane").gameObject;

		showMainPane ();
	}

	public void showMainPane()
	{
		editPane.SetActive (false);
		mainPane.SetActive (true);
	}
	public void showEditPane()
	{
		mainPane.SetActive (false);
		editPane.SetActive (true);
	}

	public void saveNewView()
	{
	}
}
