using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderMeshReader
{

    class BlenderMesh : MeshInterface
    {
        public string Name { get; set; }
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        public List<PolygonListEntry> PolygonList { get; set; }
        public int[] LoopList { get; set; }
        public int[] TriangleList { get; set; }

        public BlenderMesh()
        {
            this.Name = "defaultMesh";
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
            TriangleList = new int[0];
        }

        public BlenderMesh(string name)
        {
            this.Name = name;
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
            TriangleList = new int[0];
        }

        //This method creates the TriangleList of the mesh from PolygonList and LoopList.
        //This method must be called after the VertexList, PolygonList and LoopList are complete
        public void createTriangleList()
        {
            List<int> triangle = new List<int>(); //TODO check what is faster: List (create List, add elements, ToArray()) or Array (calc length, fill array)
            foreach (PolygonListEntry polygon in PolygonList)
            {
                for (int i = 0; i < polygon.Lenght - 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 0)
                        {
                            triangle.Add(LoopList[polygon.StartIndex + j]);
                        }
                        else
                        {
                            if (j != 2)
                            {
                                triangle.Add(LoopList[polygon.StartIndex + j + i + 1]);
                            }
                            else
                            {
                                triangle.Add(LoopList[polygon.StartIndex]);
                            }
                        }

                    }

                }
            }
            TriangleList = triangle.ToArray();
            return;
        }


    }
}
