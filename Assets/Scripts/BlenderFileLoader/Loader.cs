using UnityEngine;
using BlenderMeshReader;
using System;
using System.Threading;
using System.Collections.Generic;

public class Loader : MonoBehaviour {

    public GameObject meshNode;
    public Material defaultMaterial;

    //List of blender meshes, filled by worker thread. Corresponding triangle array is in triangles at the smae index
    private volatile List<BlenderMesh> blenderMeshes = new List<BlenderMesh>();
    //List of triangles, filled by worker thread
    private volatile List<int[]> triangles = new List<int[]>();
    //True if file is loaded
    private volatile bool loaded = false;
    private volatile string Path = "";


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Update " + DateTime.Now.Millisecond);
        if (loaded)
        {
            LoadFileExecute();
            blenderMeshes = new List<BlenderMesh>();
            triangles = new List<int[]>();
            loaded = false;
            Path = "";
        }
    }

    public void LoadFile(string path)
    {
        //Destroy current game objectes attached to mesh node
        for (int i = 0; i < meshNode.transform.childCount; i++)
        {
            Destroy(meshNode.transform.GetChild(i).gameObject);
        }
        this.Path = path;
        Thread thread = new Thread(new ThreadStart(this.LoadFileWorker));
        thread.Start();
    }

    private void LoadFileWorker()
    {
        BlenderFile b = new BlenderFile(Path);
        blenderMeshes = b.readMeshVertices();
        foreach (BlenderMesh blenderMesh in blenderMeshes)
        {
            triangles.Add(blenderMesh.createTriangleList());
        }
        loaded = true;
        return;
    }


    private void LoadFileExecute()
    {
        for (int i = 0; i < blenderMeshes.Count; i++)
        {
            //Spawn object
            GameObject objToSpawn = new GameObject(blenderMeshes[i].Name);

            //Add Components
            objToSpawn.AddComponent<MeshFilter>();
            objToSpawn.AddComponent<MeshCollider>();
            objToSpawn.AddComponent<MeshRenderer>();

            //Add material
            objToSpawn.GetComponent<MeshRenderer>().material = defaultMaterial;

            //Create Mesh
            Mesh mesh = new Mesh();
            objToSpawn.GetComponent<MeshFilter>().mesh = mesh;

            mesh.name = blenderMeshes[i].Name;

            mesh.vertices = blenderMeshes[i].VertexList;
            mesh.normals = blenderMeshes[i].NormalList;

            mesh.triangles = triangles[i];

            objToSpawn.transform.parent = meshNode.transform;
            //objToSpawn.transform.localPosition = new Vector3(0, 0, 0);
            //objToSpawn.transform.localScale = new Vector3(10, 10, 10);
        }
    }
}
