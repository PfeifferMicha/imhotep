using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderMeshReader
{

    class BlenderMesh
    {
        public string Name { get; set; }
        //X,Y,Z coordinates of vertices
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        //List of Polygons. First value is start index in LoopList, second value is length of polygon (in general 3)
        public List<PolygonListEntry> PolygonList { get; set; }
        //List of Loops. The value is index in VertexList
        public int[] LoopList { get; set; }


        public BlenderMesh()
        {
            this.Name = "defaultMesh";
            VertexList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
        }

        public BlenderMesh(string name)
        {
            this.Name = name;
            VertexList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
        }

        public int[] createTriangleList()
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
            return triangle.ToArray();
        }


    }
}
