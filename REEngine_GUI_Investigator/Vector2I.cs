using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class Vector2I : REEngineValues
    {
        [XmlAttribute]
        public int x;
        [XmlAttribute]
        public int y;

        public Vector2I() : base() { }
        public Vector2I(BinaryReader br) : base(br) { }
        public Vector2I(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            x = br.ReadInt32();
            y = br.ReadInt32();
        }

        public override void SetStrings()
        {
            typeString = "vector2I";
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

        public static void DeleteDuplicates(List<Vector2I> l)
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
                Vector2I s1 = (Vector2I)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (Vector2I)l[j]))
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

        public static bool Compare(Vector2I a, Vector2I b)
        {
            return a.x == b.x && a.y == b.y;
        }
    }
}
