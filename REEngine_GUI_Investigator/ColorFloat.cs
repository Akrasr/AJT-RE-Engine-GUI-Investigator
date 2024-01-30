using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class ColorFloat : REEngineValues
    {
        [XmlAttribute]
        public float r;
        [XmlAttribute]
        public float g;
        [XmlAttribute]
        public float b;
        [XmlAttribute]
        public float a;

        public ColorFloat() : base() { }
        public ColorFloat(BinaryReader br) : base(br) { }
        public ColorFloat(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            r = br.ReadSingle();
            g = br.ReadSingle();
            b = br.ReadSingle();
            a = br.ReadSingle();
        }

        public override void SetStrings()
        {
            typeString = "colorf";
            valstring = "r = " + r + "; g = " + g + "; b = " + b + "; a = " + a;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(r);
            bw.Write(g);
            bw.Write(b);
            bw.Write(a);
        }

        public override ulong GetDataLength()
        {
            return 16;
        }

        public static void DeleteDuplicates(List<ColorFloat> l)
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
                ColorFloat s1 = (ColorFloat)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (ColorFloat)l[j]))
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

        public static bool Compare(ColorFloat s1, ColorFloat s2)
        {
            return s1.r == s2.r && s1.g == s2.g && s1.b == s2.b && s1.a == s2.a;
        }
    }
}
