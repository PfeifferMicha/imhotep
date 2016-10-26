using System;
using System.Collections.Generic;


namespace BlenderMeshReader
{
    class Structure
    {
        public int Index { get; private set; }
        public string Name { get; private set; } //Name of Structure
        public List<Field> Fields { get; set; }

        public Structure(string name, int index)
        {
            this.Name = name;
            this.Index = index;
            this.Fields = new List<Field>();
        }


        public int getLength()
        {
            int l = 0;
            foreach (Field f in Fields)
            {
                l += f.getLength();
            }
            return l;
        }

        public override string ToString()
        {
            string result = Name + " {" + Index + "}" + " - " + getLength() + "byte" + Environment.NewLine;
            foreach(Field f in Fields)
            {
                result += "    "+f.ToString() + Environment.NewLine;
            }
            return result;
        }

    }
}
