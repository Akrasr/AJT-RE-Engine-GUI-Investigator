using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class Vector3 : REEngineValues
    {
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;

        public Vector3() : base() { }
        public Vector3(BinaryReader br) : base(br) { }
        public Vector3(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
        }

        public override void SetStrings()
        {
            typeString = "vector3";
            valstring = "x = " + x + "; y = " + y + "; z = " + z;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
            bw.Write(z);
        }

        public override ulong GetDataLength()
        {
            return 12;
        }

        public static void DeleteDuplicates(List<Vector3> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                bool del = false;
                for (int j = 0; j < i; j++)
                {
                    if (Compare(l[i], l[j]))
                    {
                        del = true;
                        break;
                    }
                }
                if (del)
                {
                    l.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void DeleteDuplicates(List<REEngineValues> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                bool del = false;
                Vector3 s1 = (Vector3)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (Vector3)l[j]))
                    {
                        del = true;
                        break;
                    }
                }
                if (del)
                {
                    l.RemoveAt(i);
                    i--;
                }
            }
        }

        public static bool Compare(Vector3 a, Vector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
    }
}
