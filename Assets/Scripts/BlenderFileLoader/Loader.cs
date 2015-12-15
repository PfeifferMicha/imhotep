using UnityEngine;
using BlenderMeshReader;
using System;
using System.Threading;
using System.Collections.Generic;

public class Loader : MonoBehaviour {

    public GameObject meshNode;
    public Material defaultMaterial;

    //List of lists of UnityMeshes with max. 2^16 vertices per mesh
    private volatile List<List<UnityMesh>> unityMeshes = new List<List<UnityMesh>>();
    //True if file is loaded
    private volatile bool loaded = false;
    private volatile string Path = "";


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (loaded)
        {
            LoadFileExecute();
            unityMeshes = new List<List<UnityMesh>>();
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
        Debug.Log("Start Thread" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond);
        //LoadFileWorker();
    }

    //Runs in own thread
    private void LoadFileWorker()
    {
        BlenderFile b = new BlenderFile(Path);
        List<BlenderMesh> blenderMeshes = new List<BlenderMesh>();
        blenderMeshes = b.readMesh();
        unityMeshes = BlenderFile.createSubmeshesForUnity(blenderMeshes);
        loaded = true;
        return;
    }


    private void LoadFileExecute()
    {
        Debug.Log("Start Execute" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond);
        foreach (List<UnityMesh> um in unityMeshes) {
            foreach (UnityMesh unityMesh in um)
            {
                //Spawn object
                GameObject objToSpawn = new GameObject(unityMesh.Name);

                objToSpawn.transform.parent = meshNode.transform; ;


                //Add Components
                objToSpawn.AddComponent<MeshFilter>();
                //objToSpawn.AddComponent<MeshCollider>(); //TODO need to much time --> own thread?? Dont work in Unity!!
                objToSpawn.AddComponent<MeshRenderer>();
                
                //Add material
                objToSpawn.GetComponent<MeshRenderer>().material = defaultMaterial;
               
                //Create Mesh
                Mesh mesh = new Mesh();
                mesh.name = unityMesh.Name;
                mesh.vertices = unityMesh.VertexList;
                mesh.normals = unityMesh.NormalList;
                mesh.triangles = unityMesh.TriangleList;

                objToSpawn.GetComponent<MeshFilter>().mesh = mesh;
                //objToSpawn.GetComponent<MeshCollider>().sharedMesh = mesh;

                objToSpawn.transform.localPosition = new Vector3(0, 0, 0);
                //objToSpawn.transform.localScale = new Vector3(1, 1, 1);
            }
            
        }
        Debug.Log("End Execute" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond );

    }

}
