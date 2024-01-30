using System;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class Attribute
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int type;
        [XmlAttribute]
        public int unknown; //usually -1 or 0
        ulong nameOffset;
        bool isOffset;
        ulong valueOffset;
        [XmlAttribute]
        public uint hash;
        uint padding; // usually 0;
        public REEngineValues val;


        public Attribute()
        {
        }


        public Attribute(BinaryReader br)
        {
            Read(br);
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

        public void Read(BinaryReader br)
        {
            type = br.ReadInt32();
            unknown = br.ReadInt32();
            nameOffset = br.ReadUInt64();
            name = new REString(br, nameOffset).str;
            isOffset = REEngineValues.GetIsOffset(type);
            if (isOffset)
            {
                valueOffset = br.ReadUInt64();
                val = REEngineValues.GetValue(type, br, valueOffset);
            }
            else
                val = REEngineValues.GetValue(type, br);
            hash = br.ReadUInt32();
            padding = br.ReadUInt32();
            if (padding != 0)
            {
                throw new Exception("Attribute: padding != 0. Further investigation is required");
            }
        }

        public void Show()
        {
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine(unknown + "-" + type + "-" + name + ": " + val.GetValString());
            //Console.WriteLine("name: " + name);
            //Console.Write("val: ");
            //val.Show();
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(type);
            bw.Write(unknown);
            bw.Write(nameOffset);
            if (isOffset)
            {
                bw.Write(valueOffset);
                //val.Save(bw, valueOffset);
            }
            else val.Save(bw);
            bw.Write(hash);
            bw.Write(padding);
        }

        public void RestoreAfterDeserialize()
        {
            isOffset = REEngineValues.GetIsOffset(type);
            padding = 0;
        }
    }
}
