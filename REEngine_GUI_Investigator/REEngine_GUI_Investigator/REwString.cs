using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace REEngine_GUI_Investigator
{
    public class REwString : REEngineValues
    {
        [XmlAttribute]
        public string str;

        public REwString() : base() { }
        public REwString(BinaryReader br) : base(br) { }
        public REwString(string st)
        {
            str = st;
            SetStrings();
        }
        public REwString(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            long oldpos = br.BaseStream.Position;
            int c = 0;
            for (int i = 0; br.ReadInt16() != 0; i += 2)
            {
                c += 2;
            }
            br.BaseStream.Position = oldpos;
            byte[] dat = br.ReadBytes(c);
            str = Encoding.Unicode.GetString(dat);
            SetMockString();
        }

        public override void SetStrings()
        {
            typeString = "wstring";
            valstring = str;
        }

        public override ulong GetDataLength()
        {
            if (str == null)
                return 2;
            GetRealString();
            ulong res = (ulong)(str.Length + 1) * 2;
            SetMockString();
            return res;
        }

        public override void Save(BinaryWriter bw)
        {
            GetRealString();
            if (str == null)
            {
                bw.Write((short)0);
                return;
            }
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            bw.Write(bytes);
            bw.Write((short)0);
        }

        public void SetMockString()
        {
            if (str == null)
                return;
            if (str.Length == 0)
                return;
            string tmps = "";
            byte[] bs = Encoding.Unicode.GetBytes(str);
            for (int i = 0; i < str.Length; i++)
            {
                if (bs[i * 2] == 0x03 && bs[i * 2 + 1] == 0)
                {
                    tmps += "{dsli: " + bs[i * 2] + "}";
                }
                else tmps += str[i];
            }
            str = tmps;
        }

        public void GetRealString()
        {
            str = str.Replace("{dsli: 3}", Encoding.Unicode.GetString(new byte[] { 0x03, 0x00}));
        }

        public static void DeleteDuplicates(List<string> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                bool del = false;
                for (int j = 0; j < i; j++)
                {
                    if (l[i] == l[j])
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

        public static void DeleteDuplicates(List<REwString> l)
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
                REwString s1 = (REwString)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (REwString)l[j]))
                    {
                        del = true;
                        //Console.WriteLine("deleting " + s1.str + " " + ((REwString)l[j]).str + " " + i + " " + j);
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

        public static bool Compare(REwString s1, REwString s2)
        {
            return s1.str == s2.str;
        }
    }
}
