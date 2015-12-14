using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderMeshReader
{

    class PolygonListEntry
    {
        public int StartIndex { get; private set; }
        public int Lenght { get; private set; }

        public PolygonListEntry(int startIndex, int length)
        {
            this.StartIndex = startIndex;
            this.Lenght = length;
        }
    }
}
