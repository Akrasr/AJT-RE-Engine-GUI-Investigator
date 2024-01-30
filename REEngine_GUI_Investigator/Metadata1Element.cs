using System.IO;
using System;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class Metadata1Element
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string _class;
        [XmlAttribute]
        public string wclass;
        ulong nameOffset;
        ulong classOffset;
        [XmlAttribute]
        public int type;
        int unknown;
        ulong valueOffset;
        ulong classwOffset;
        [XmlAttribute]
        public ulong hash;
        public REEngineValues val;

        public Metadata1Element()
        {
        }

        public Metadata1Element(BinaryReader br)
        {
            Read(br);
        }

        public void RestoreAfterDeserialize()
        {
            unknown = -1;
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
            ofc.AddData(14, new REString(_class));
            ofc.AddData(14, new REString(wclass));
            if (REEngineValues.GetIsOffset(type))
                ofc.AddData(type, val);
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
            classOffset = ofc.GetOffset(14, new REString(_class));
            classwOffset = ofc.GetOffset(14, new REString(wclass));
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = ofc.GetOffset(type, val);
            }
        }

        public void Read(BinaryReader br)
        {
            nameOffset = br.ReadUInt64();
            classOffset = br.ReadUInt64();
            type = br.ReadInt32();
            unknown = br.ReadInt32();
            if (unknown != -1)
            {
                throw new Exception("Metadata1Element: unknown != -1. Further investigation is required");
            }
            classwOffset = br.ReadUInt64();
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = br.ReadUInt64();
                val = REEngineValues.GetValue(type, br, valueOffset);
            }
            else
            {
                val = REEngineValues.GetValue(type, br);
            }
            hash = br.ReadUInt64();
            name = new REwString(br, nameOffset).str;
            _class = new REString(br, classOffset).str;
            wclass = new REString(br, classwOffset).str;
        }

        public void Show()
        {
            Console.WriteLine("name: " + name);
            Console.WriteLine("class: " + _class);
            Console.WriteLine("wclass: " + wclass);
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            Console.WriteLine(unknown + "-" + type + "-" + name + ": " + val.GetValString());
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(nameOffset);
            bw.Write(classOffset);
            bw.Write(type);
            bw.Write(unknown);
            bw.Write(classwOffset);
            if (REEngineValues.GetIsOffset(type))
            {
                bw.Write(valueOffset);
                //val.Save(bw, valueOffset);
            }
            else
            {
                val.Save(bw);
            }
            bw.Write(hash);
        }
    }
}
