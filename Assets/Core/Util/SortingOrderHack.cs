using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderHack : MonoBehaviour {

	public string targetSortingLayer = "Transparent";
	public int targetSortingOrder = 1;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().sortingLayerName = targetSortingLayer;
		GetComponent<Renderer> ().sortingOrder = targetSortingOrder;
	}
}
