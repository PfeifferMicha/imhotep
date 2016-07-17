using UnityEngine;
using System.Collections;

public class OpacitySlider : MonoBehaviour
{

    public GameObject gameObjectToChangeOpacity { get; set; }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeOpacity(float f)
    {
        //Debug.Log(f);

        if (f == 0.0f)
        {
            gameObjectToChangeOpacity.SetActive(false);
            return;
        }
        else
        {
            gameObjectToChangeOpacity.SetActive(true);
        }

        foreach (MeshRenderer mr in gameObjectToChangeOpacity.GetComponentsInChildren<MeshRenderer>())
        {
           
            if(f == 1.0f) //Use opaque material
            {
                Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
                mat.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);

                mr.material = new Material(mat);
            }
            else if(f > 0.0 && f < 1.0f) // Use transparent material
            {
                Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
                mat.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);

                mr.material = new Material(mat); 
            }

        }
    }
}
