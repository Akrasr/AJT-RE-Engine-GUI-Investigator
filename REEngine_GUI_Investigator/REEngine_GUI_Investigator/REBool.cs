using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class REBool : REEngineValues
    {
        [XmlAttribute]
        public bool b;

        public REBool() : base() { }
        public REBool(BinaryReader br) : base(br) { }
        public REBool(BinaryReader br, ulong pointer) : base(br, pointer) { }

        public override void Read(BinaryReader br)
        {
            long l = br.ReadInt64();
            b = l == 1;
        }

        public override void SetStrings()
        {
            typeString = "bool";
            valstring = "" + b;
        }

        public override void Save(BinaryWriter bw)
        {
            long l = b ? 1 : 0;
            bw.Write(l);
        }

        public override ulong GetDataLength()
        {
            return 8;
        }
    }
}
