using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class RELong : REEngineValues
    {
        [XmlAttribute]
        public long l;

        public RELong() : base() { }
        public RELong(BinaryReader br) : base(br) { }
        public RELong(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            l = br.ReadInt64();
        }

        public override void SetStrings()
        {
            typeString = "long";
            valstring = "" + l;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(l);
        }

        public override ulong GetDataLength()
        {
            return 8;
        }
    }
}
