﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OpacitySlider : MonoBehaviour
{
	
	public GameObject gameObjectToChangeOpacity;

    // Use this for initialization
    void Start()
	{
		updateSlider();
    }

	void OnEnable()
	{
		// Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

	void OnDisable()
	{
		// Unregister myself:
		PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

	//Set the slider to the value of f
    public void changeOpacity(float f)
    {
		MeshMaterialControl moc = gameObjectToChangeOpacity.GetComponent<MeshMaterialControl> ();
		if (moc != null) {
			moc.changeOpactiyOfChildren (f);
		}
    }

	// Called if Silder value changed from external tool by event system.
	private void updateSlider(object obj = null){		
		if (gameObjectToChangeOpacity != null) {
			float currentOpacity = 0f;
			if (gameObjectToChangeOpacity.activeSelf) {
				MeshRenderer mr = gameObjectToChangeOpacity.GetComponentInChildren<MeshRenderer> ();
				currentOpacity = mr.material.color.a;
			} else {
				currentOpacity = 0f;
			}

			GetComponent<Slider> ().value = currentOpacity;

		}
	}

	void Update()
	{
		
	}
}