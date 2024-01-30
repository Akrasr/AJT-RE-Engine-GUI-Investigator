using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class REDouble : REEngineValues
    {
        [XmlAttribute]
        public double d;

        public REDouble() : base() { }
        public REDouble(BinaryReader br) : base(br) { }
        public REDouble(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            d = br.ReadDouble();
        }

        public override void SetStrings()
        {
            typeString = "double";
            valstring = "" + d;
        }

        public override void Save(BinaryWriter bw)
        {
            bw.Write(d);
        }

        public override ulong GetDataLength()
        {
            return 8;
        }
    }
}
