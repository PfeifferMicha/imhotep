using System;


namespace BlenderMeshReader
{
    class Field
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public Field(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public int getLength()
        {
            if (Name.StartsWith("*"))
            {
                return BlenderFile.PointerSize;
            }
            else
            {
                if (Name.Contains("[") && Name.Contains("]"))
                {
                    int start = Name.IndexOf("[");
                    int end = Name.IndexOf("]");
                    return Type.Length * Int32.Parse(Name.Substring(start + 1, end - start - 1));
                }
                else
                {
                    return Type.Length;
                }
            }

        }

        public override string ToString()
        {
            return Name + ": " + Type.ToString() + " - " + getLength() + "byte";
        }
    }
}
