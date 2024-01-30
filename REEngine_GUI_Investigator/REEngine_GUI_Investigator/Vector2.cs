using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class Vector2 : REEngineValues
    {
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;

        public Vector2() : base() { }
        public Vector2(BinaryReader br) : base(br) { }
        public Vector2(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            x = br.ReadSingle();
            y = br.ReadSingle();
        }

        public override void SetStrings()
        {
            typeString = "vector2";
            valstring = "x = " + x + "; y = " + y;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
        }

        public override ulong GetDataLength()
        {
            return 8;
        }

        public static void DeleteDuplicates(List<Vector2> l)
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
                Vector2 s1 = (Vector2)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (Vector2)l[j]))
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

        public static bool Compare(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
    }
}
