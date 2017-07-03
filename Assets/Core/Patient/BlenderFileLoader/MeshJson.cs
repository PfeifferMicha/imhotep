using System.Collections.Generic;

/*! This class represents an element in the mesh list in mesh.json
 * It contains the name of a single mesh and the color */
public class MeshListElement
{
    public string name;
    public string color;
}

/*! Used to store the loaded the mesh.json */
public class MeshJson
{
	/*! The relative path to the blend file */
    public string pathToBlendFile;

	/*! List of meshs the blend file contains (with name and color information)*/
    public List<MeshListElement> meshList;

    public MeshJson()
    {
        meshList = new List<MeshListElement>();
    }
}
