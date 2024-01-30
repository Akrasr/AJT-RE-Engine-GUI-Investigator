using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    public class REID : REEngineValues
    {
        [XmlAttribute]
        public byte[] id;

        public REID() : base() { }
        public REID(BinaryReader br) : base(br) { }
        public REID(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            id = br.ReadBytes(16);
        }

        public override void SetStrings()
        {
            typeString = "ID";
            valstring = ByteArrayToString(id);
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(id);
        }

        public override ulong GetDataLength()
        {
            return 16;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string ByteArrayToString(uint b)
        {
            byte[] ba = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ba[i] = (byte)(b % 256);
                b /= 256;
            }
            return ByteArrayToString(ba);
        }

        public static string ByteArrayToString(ulong b)
        {
            byte[] ba = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                ba[i] = (byte)(b % 256);
                b /= 256;
            }
            return ByteArrayToString(ba);
        }

        public static string ByteArrayToString(int b)
        {
            byte[] ba = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ba[i] = (byte)(b % 256);
                b /= 256;
            }
            return ByteArrayToString(ba);
        }

        public static ulong ByteArrayToLong(byte[] dat)
        {
            ulong res = 0;
            for (int i = dat.Length - 1; i >= 0; i--)
            {
                res *= 256;
                res += dat[i];
            }
            return res;
        }

        public static byte[] LongToByteArray(ulong b)
        {
            byte[] ba = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                ba[i] = (byte)(b % 256);
                b /= 256;
            }
            return ba;
        }

        public static void DeleteDuplicates(List<REID> l)
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
                REID s1 = (REID)l[i];
                for (int j = 0; j < i; j++)
                {
                    if (Compare(s1, (REID)l[j]))
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

        public static bool Compare(REID s1, REID s2)
        {
            bool res = true;
            for (int i = 0; i < 16; i++)
            {
                if (s1.id[i] != s2.id[i])
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
    }
}
