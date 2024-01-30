using System;
using System.IO;
using System.Xml.Serialization;


namespace REEngine_GUI_Investigator
{
    public class UnknownData
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public byte type;
        [XmlAttribute]
        public byte unknown1;
        [XmlAttribute]
        public byte[] hash;
        ulong nameOffset;
        short padding1;
        int padding2;
        ulong unknown2;
        ulong valueOffset;
        public REEngineValues val;

        public UnknownData()
        {
        }

        public UnknownData(BinaryReader br)
        {
            Read(br);
        }

        public void RestoreAfterDeserialize()
        {
            padding1 = 0;
            padding2 = 0;
            unknown2 = 0;
        }

        public void Read(BinaryReader br)
        {
            hash = br.ReadBytes(16);
            nameOffset = br.ReadUInt64();
            type = br.ReadByte();
            unknown1 = br.ReadByte();
            padding1 = br.ReadInt16();
            if (padding1 != 0)
            {
                throw new Exception("SubData: padding1 != 0. Further investigation is required");
            }
            padding2 = br.ReadInt32();
            if (padding2 != 0)
            {
                throw new Exception("SubData: padding2 != 0. Further investigation is required");
            }
            unknown2 = br.ReadUInt64();
            if (unknown2 != 0)
            {
                throw new Exception("UnknownData: unknown2 != 0. Further investigation is required");
            }
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = br.ReadUInt64();
                val = REEngineValues.GetValue(type, br, valueOffset);
            }
            else
            {
                val = REEngineValues.GetValue(type, br);
            }
            name = new REString(br, nameOffset).str;
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(14, new REString(name));
            if (REEngineValues.GetIsOffset(type))
                ofc.AddData(type, val);
            
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(14, new REString(name));
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = ofc.GetOffset(type, val);
            }
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine("unknown1: " + unknown1);
            Console.WriteLine("unknown2: " + unknown2);
            Console.WriteLine(type + "-" + name + ": " + val.GetValString());
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(hash);
            bw.Write(nameOffset);
            bw.Write(type);
            bw.Write(unknown1);
            bw.Write(padding1);
            bw.Write(padding2);
            bw.Write(unknown2);
            if (REEngineValues.GetIsOffset(type))
            {
                bw.Write(valueOffset);
                //val.Save(bw, valueOffset);
            }
            else
            {
                val.Save(bw);
            }
        }
    }
}
