using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class REString : REEngineValues
    {
        [XmlAttribute]
        public string str;

        public REString() : base() { }
        public REString(BinaryReader br) : base(br) { }
        public REString(string st){
            str = st;
            SetStrings();
        }
        public REString(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            long oldpos = br.BaseStream.Position;
            int c = 0;
            for (int i = 0; br.ReadByte() != 0; i += 1)
            {
                c += 1;
            }
            br.BaseStream.Position = oldpos;
            byte[] dat = br.ReadBytes(c);
            str = Encoding.UTF8.GetString(dat);
        }

        public override void SetStrings()
        {
            typeString = "string";
            valstring = str;
        }

        public override void Save(BinaryWriter bw)
        {
            if (str == null)
            {
                bw.Write((byte)0);
                return;
            }
            for (int i = 0; i < str.Length; i++)
                bw.Write((byte)str[i]);
            bw.Write((byte)0);
        }

        public static void DeleteDuplicates(List<REString> l)
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

        public override ulong GetDataLength()
        {
            if (str == null)
                return 2;
            return (ulong)str.Length + 1;
        }

        public static void DeleteDuplicates(List<REEngineValues> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                bool del = false;
                REString s1 = (REString)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (REString)l[j]))
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

        public static bool Compare(REString s1, REString s2)
        {
            return s1.str == s2.str;
        }
    }
}
