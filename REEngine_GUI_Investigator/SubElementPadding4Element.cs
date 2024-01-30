using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class SubElementPadding4Element
    {
        [XmlAttribute]
        public ulong unknown;
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;
        [XmlAttribute]
        public float w;
        public SubElementPadding4Element()
        {
        }
        public SubElementPadding4Element(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            unknown = br.ReadUInt64();
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
        }

        public void Read(BinaryReader br, long pointer)
        {
            long oldpos = br.BaseStream.Position;
            br.BaseStream.Position = pointer;
            unknown = br.ReadUInt64();
            Read(br);
            br.BaseStream.Position = oldpos;
        }

        public void Show()
        {
            Console.WriteLine("unknown: " + unknown);
            Console.WriteLine("x: " + x);
            Console.WriteLine("y: " + y);
            Console.WriteLine("z: " + z);
            Console.WriteLine("w: " + w);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(unknown);
            bw.Write(x);
            bw.Write(y);
            bw.Write(z);
            bw.Write(w);
        }

        public byte[] ToBytes()
        {
            byte[] res = new byte[24];
            using (MemoryStream ms = new MemoryStream(res))
                using (BinaryWriter bw = new BinaryWriter(ms))
            {
                Save(bw);
            }
            return res;
        }

        public static byte[] ToBytes(SubElementPadding4Element[] elems)
        {
            byte[][] tmp = new byte[elems.Length][];
            int count = 0;
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = elems[i].ToBytes();
                count += tmp[i].Length;
            }
            byte[] res = new byte[count + 4];
            using (MemoryStream ms = new MemoryStream(res))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(tmp.Length);
                for (int i = 0; i < tmp.Length; i++)
                {
                    bw.Write(tmp[i]);
                }
            }
            return res;
        }
    }
}
