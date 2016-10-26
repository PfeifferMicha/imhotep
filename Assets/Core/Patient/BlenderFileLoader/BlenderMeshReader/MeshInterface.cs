using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlenderMeshReader
{
    interface MeshInterface
    {
        string Name { get; set; }
        //X,Y,Z coordinates of vertices
        Vector3[] VertexList { get; set; }
        Vector3[] NormalList { get; set; }       
        //List of vertex indices which represent the triangles. Computed from PolygonList and LoopList
        int[] TriangleList { get; set; }
    }
}
