using System.Collections.Generic;

public class MeshListElement
{
    public string name;
    public string color;
}

/**
    Used to load the mesh.json
    */
public class MeshJson
{
    public string pathToBlendFile;
    public List<MeshListElement> meshList;

    public MeshJson()
    {
        meshList = new List<MeshListElement>();
    }
}
