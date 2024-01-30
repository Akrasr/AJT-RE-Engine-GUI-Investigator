using System.IO;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class Vector4 : REEngineValues
    {
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;
        [XmlAttribute]
        public float w;

        public Vector4() : base() { }
        public Vector4(BinaryReader br) : base(br) { }
        public Vector4(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
        }

        public override void SetStrings()
        {
            typeString = "vector4";
            valstring = "x = " + x + "; y = " + y + "; z = " + z + "; w = " + w;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
            bw.Write(z);
            bw.Write(w);
        }

        public override ulong GetDataLength()
        {
            return 16;
        }

        public static void DeleteDuplicates(List<Vector4> l)
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
                Vector4 s1 = (Vector4)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (Vector4)l[j]))
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

        public static bool Compare(Vector4 a, Vector4 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }
    }
}
