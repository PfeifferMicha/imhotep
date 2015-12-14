using UnityEngine;
using BlenderMeshReader;
using System;
using System.Collections.Generic;

public class Loader : MonoBehaviour {

    public GameObject meshNode;
    public Material defaultMaterial;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LoadFile(string path)
    {
        //Destroy current game objectes attached to mesh node
        for (int i = 0; i < meshNode.transform.childCount; i++)
        {
            Destroy(meshNode.transform.GetChild(i).gameObject);
        }

        //Load blender file
        BlenderFile blenderFile = new BlenderFile(path);
        List<BlenderMesh> bm = blenderFile.readMeshVertices();

        foreach(BlenderMesh blenderMesh in bm)
        {
            //Spawn object
            GameObject objToSpawn = new GameObject("Test");

            //Add Components
            objToSpawn.AddComponent<MeshFilter>();
            objToSpawn.AddComponent<MeshCollider>();
            objToSpawn.AddComponent<MeshRenderer>();

            //Add material
            objToSpawn.GetComponent<MeshRenderer>().material = defaultMaterial;

            //Create Mesh
            Mesh mesh = new Mesh();
            objToSpawn.GetComponent<MeshFilter>().mesh = mesh;

            mesh.name = "mesh";

            mesh.vertices = blenderMesh.VertexList;
            mesh.normals = blenderMesh.NormalList;

            //generate triangle list
            List<int> triangle = new List<int>();
            foreach (PolygonListEntry polygon in blenderMesh.PolygonList)
            {
                for(int i=0; i < polygon.Lenght-2; i++)
                {
                    for(int j=0; j < 3; j++)
                    {
                        if (i == 0)
                        {
                            triangle.Add(blenderMesh.LoopList[polygon.StartIndex + j]);
                        }
                        else
                        {
                            if(j != 2)
                            {
                                triangle.Add(blenderMesh.LoopList[polygon.StartIndex + j + i + 1]);
                            }
                            else
                            {
                                triangle.Add(blenderMesh.LoopList[polygon.StartIndex]);
                            }
                        }

                    }

                }
            }
            
            mesh.triangles = triangle.ToArray();

            objToSpawn.transform.parent = meshNode.transform;
            //objToSpawn.transform.localPosition = new Vector3(0, 0, 0);
            objToSpawn.transform.localScale = new Vector3(10, 10, 10);
        }

        

    }
}
