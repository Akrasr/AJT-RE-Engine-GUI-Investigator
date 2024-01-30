using System;
using System.IO;
using System.Xml.Serialization;


namespace REEngine_GUI_Investigator
{
    public class SubVariables
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string _class;
        [XmlAttribute]
        public string paramname;
        [XmlAttribute]
        public int type;
        [XmlAttribute]
        public byte[] hash;
        ulong nameOffset;
        int unknown; //обычно -1
        ulong classOffset;
        ulong paramnameOffset;

        public SubVariables()
        {
        }

        public SubVariables(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            hash = br.ReadBytes(16);
            nameOffset = br.ReadUInt64();
            type = br.ReadInt32();
            unknown = br.ReadInt32();
            if (unknown != -1)
            {
                throw new Exception("SubVariables: unknown != -1. Further investigation is required");
            }
            classOffset = br.ReadUInt64();
            paramnameOffset = br.ReadUInt64();
            name = new REwString(br, nameOffset).str;
            _class = new REString(br, classOffset).str;
            paramname = new REString(br, paramnameOffset).str;
        }

        public void RestoreAfterDeserialize()
        {
            unknown = -1;
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
            ofc.AddData(14, new REString(_class));
            ofc.AddData(14, new REString(paramname));
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
            classOffset = ofc.GetOffset(14, new REString(_class));
            paramnameOffset = ofc.GetOffset(14, new REString(paramname));
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine("name: " + name);
            Console.WriteLine("type: " + type);
            Console.WriteLine("class: " + _class);
            Console.WriteLine("paramname:" + paramname);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(hash);
            bw.Write(nameOffset);
            bw.Write(type);
            bw.Write(unknown);
            bw.Write(classOffset);
            bw.Write(paramnameOffset);
        }
    }
}
