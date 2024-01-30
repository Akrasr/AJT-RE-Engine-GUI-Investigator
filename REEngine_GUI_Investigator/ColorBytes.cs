using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class ColorBytes : REEngineValues
    {
        [XmlAttribute]
        public byte r;
        [XmlAttribute]
        public byte g;
        [XmlAttribute]
        public byte b;
        [XmlAttribute]
        public byte a;

        public ColorBytes() : base() { }
        public ColorBytes(BinaryReader br) : base(br) { }
        public ColorBytes(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            r = br.ReadByte();
            g = br.ReadByte();
            b = br.ReadByte();
            a = br.ReadByte();
        }

        public override void SetStrings()
        {
            typeString = "colorb";
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
            return 4;
        }

        public static void DeleteDuplicates(List<ColorBytes> l)
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
                ColorBytes s1 = (ColorBytes)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (ColorBytes)l[j]))
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

        public static bool Compare(ColorBytes s1, ColorBytes s2)
        {
            return s1.r == s2.r && s1.g == s2.g && s1.b == s2.b && s1.a == s2.a;
        }
    }
}
