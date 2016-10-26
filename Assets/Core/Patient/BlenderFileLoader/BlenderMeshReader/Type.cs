using System;


namespace BlenderMeshReader
{
    class Type
    {
        public string Name { get; set; }
        public short Length { get; set; }

        public Type (string name, short length)
        {
            this.Name = name;
            this.Length = length;
        }

        public Type(string name)
        {
            this.Name = name;
            this.Length = 0;
        }

        public override string ToString()
        {
            return "(" + Name + " | " + Length + "byte)";
        }
    }
}
