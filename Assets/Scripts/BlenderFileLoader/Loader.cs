using UnityEngine;
using BlenderMeshReader;
using System;
using System.Threading;
using System.Collections.Generic;

public class Loader : MonoBehaviour {

    public GameObject meshNode;
    public Material defaultMaterial;

    //List of blender meshes, filled by worker thread
    private volatile List<BlenderMesh> blenderMeshes = new List<BlenderMesh>();
    //List of lists of UnityMeshes with max. 2^16 vertices per mesh
    private volatile List<List<UnityMesh>> unityMeshes = new List<List<UnityMesh>>();
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
        //LoadFileWorker();
    }

    //Runs in own thread
    private void LoadFileWorker()
    {
        BlenderFile b = new BlenderFile(Path);
        blenderMeshes = b.readMesh();
        unityMeshes = BlenderFile.createSubmeshesForUnity(blenderMeshes);
        loaded = true;
        return;
    }


    private void LoadFileExecute()
    {
        foreach (List<UnityMesh> um in unityMeshes) {
            foreach (UnityMesh blenderMesh in um)
            {
                //Spawn object
                GameObject objToSpawn = new GameObject(blenderMesh.Name);

                //Add Components
                objToSpawn.AddComponent<MeshFilter>();
                objToSpawn.AddComponent<MeshCollider>();
                objToSpawn.AddComponent<MeshRenderer>();

                //Add material
                objToSpawn.GetComponent<MeshRenderer>().material = defaultMaterial;

                //Create Mesh
                Mesh mesh = new Mesh();
                objToSpawn.GetComponent<MeshFilter>().mesh = mesh;

                mesh.name = blenderMesh.Name;

                mesh.vertices = blenderMesh.VertexList;
                mesh.normals = blenderMesh.NormalList;

                mesh.triangles = blenderMesh.TriangleList;

                objToSpawn.transform.parent = meshNode.transform;
                objToSpawn.transform.localPosition = new Vector3(0, 0, 0);
                //objToSpawn.transform.localScale = new Vector3(10, 10, 10);
            }
        }
    }
}
