using System;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class SubData
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string _class;
        [XmlAttribute]
        public string wclass;
        ulong nameOffset;
        ulong classOffset;
        ulong classwOffset;
        [XmlAttribute]
        public byte type;
        [XmlAttribute]
        public byte unknown;
        [XmlAttribute]
        public byte[] hash;
        short padding1;
        int padding2;
        ulong valueOffset;
        public REEngineValues val;

        public SubData()
        {
        }

        public SubData(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            hash = br.ReadBytes(16);
            nameOffset = br.ReadUInt64();
            classOffset = br.ReadUInt64();
            classwOffset = br.ReadUInt64();
            type = br.ReadByte();
            unknown = br.ReadByte();
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
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = br.ReadUInt64();
                val = REEngineValues.GetValue(type, br, valueOffset);
            } else
            {
                val = REEngineValues.GetValue(type, br);
            }
            name = new REwString(br, nameOffset).str;
            _class = new REString(br, classOffset).str;
            wclass = new REwString(br, classwOffset).str;
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
            ofc.AddData(14, new REString(_class));
            ofc.AddData(13, new REwString(wclass));
            if (REEngineValues.GetIsOffset(type))
                ofc.AddData(type, val);
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
            classOffset = ofc.GetOffset(14, new REString(_class));
            classwOffset = ofc.GetOffset(13, new REwString(wclass));
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = ofc.GetOffset(type, val);
            }
        }

        public void RestoreAfterDeserialize()
        {
            padding1 = 0;
            padding2 = 0;
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine("name: " + name);
            Console.WriteLine("class: " + _class);
            Console.WriteLine("wclass: " + wclass);
            Console.WriteLine("type: " + type);
            Console.WriteLine("unknown: " + unknown);
            Console.WriteLine("val:" + val.GetValString());
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(hash);
            bw.Write(nameOffset);
            bw.Write(classOffset);
            bw.Write(classwOffset);
            bw.Write(type);
            bw.Write(unknown);
            bw.Write(padding1);
            bw.Write(padding2);
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
