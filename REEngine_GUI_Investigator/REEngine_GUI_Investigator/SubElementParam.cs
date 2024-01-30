using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class SubElementParam
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int type;
        [XmlAttribute]
        public int unknown;
        ulong nameOffset;
        ulong valueOffset;
        ulong hashName;
        public REEngineValues val;

        public SubElementParam()
        {
        }

        public SubElementParam(BinaryReader br)
        {
            Read(br);
        }

        public void RestoreHash()
        {
            hashName = MurMurHash3.Hash(name, false);
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
            if (REEngineValues.GetIsOffset(type))
            {
                valueOffset = br.ReadUInt64();
                val = REEngineValues.GetValue(type, br, valueOffset);
            }
            else
            {
                val = REEngineValues.GetValue(type, br);
            }
            hashName = br.ReadUInt64();
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hashName));
            Console.WriteLine(unknown + "-" + type + "-" + name + ": " + val.GetValString());
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(type);
            bw.Write(unknown);
            bw.Write(nameOffset);
            if (REEngineValues.GetIsOffset(type))
            {
                bw.Write(valueOffset);
                //val.Save(bw, valueOffset);
            }
            else
            {
                val.Save(bw);
            }
            bw.Write(hashName);
        }

    }
}
