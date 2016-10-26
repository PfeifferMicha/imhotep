using System.Collections;

namespace BlenderMeshReader
{

    class BoundingBox{

        public float[,] vec { get; set; } //An array [8][3] with all corner points of the bounding box
        public string name { get; set; } //The name of the mesh the bounding box belongs to

        public BoundingBox()
        {
            name = "undefined";
            vec = new float[8, 3];
        }

        public override string ToString()
        {
            string result = "";
            result += name + ": [";
            for (int i = 0; i < 8; i++)
            {
                result += "{";
                for(int j = 0; j < 3; j++)
                {
                    if(j != 2)
                    {
                        result += vec[i, j] + ", ";
                    }
                    else
                    {
                        result += vec[i, j];
                    }
                }
                result += "}";
            }
            result += "]";
            return result;
        }


    }
}
