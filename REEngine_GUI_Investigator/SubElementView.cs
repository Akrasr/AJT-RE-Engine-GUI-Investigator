using System;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class SubElementView
    {
        [XmlAttribute]
        public int subIndex;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string _class;
        [XmlAttribute]
        public byte[] thisHash;
        [XmlAttribute]
        public byte[] elementHash;
        [XmlAttribute]
        public byte[] unknownHash;
        ulong nameOffset;
        ulong classOffset;
        ulong attributesOffset;
        ulong attributesSequenceOffset;
        ulong paramsOffset;
        ulong padding3; //usually 0
        ulong padding4elementsOffset; //usually 0
        [XmlAttribute]
        public ulong unknown1;
        ulong attributesCount;
        public Attribute[] attributes;
        public SubElementParam[] subparams;
        public SubElementPadding4Element[] padding4elements;
        [XmlAttribute]
        public ushort[] attributesSequence;
        byte[] sequencePadding;
        public byte[] unknownDat;


        public SubElementView()
        {
        }


        public SubElementView(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            thisHash = br.ReadBytes(16);
            elementHash = br.ReadBytes(16);
            unknownHash = br.ReadBytes(16);
            nameOffset = br.ReadUInt64();
            name = new REwString(br, nameOffset).str;
            classOffset = br.ReadUInt64();
            attributesOffset = br.ReadUInt64();
            attributesSequenceOffset = br.ReadUInt64();
            paramsOffset = br.ReadUInt64();
            padding3 = br.ReadUInt64();
            if (padding3 != 0)
            {
                throw new Exception("SubElementView: padding3 != 0. Further investigation is required");
            }
            padding4elementsOffset = br.ReadUInt64();
            unknown1 = br.ReadUInt64();
            _class = new REString(br, classOffset).str;
            attributesCount = br.ReadUInt64();
            attributes = new Attribute[attributesCount];
            for (int i = 0; i < (int)attributesCount; i++)
            {
                attributes[i] = new Attribute(br);
            }
            ReadAttrSequence(br);
            SortAttributes();
            ReadParams(br);
            ReadPadding4Elem(br);
        }

        public ulong MeasureSubElement()
        {
            ulong res = 0;
            res += 0x70; //header
            ulong attribseclen = 8 + (ulong)(0x20 * attributes.Length);
            int attribseqseclen = attributes.Length;
            attribseqseclen = attribseqseclen % 4 == 0 ? attribseqseclen : attribseqseclen + (4 - (attribseqseclen % 4));
            attribseqseclen *= 2;
            ulong paramlen = 8 + (ulong)(0x20 * subparams.Length);
            res += attribseclen + (ulong)attribseqseclen + paramlen;
            return res;
        }

        public void AdaptOffsets(ulong subeloffset)
        {
            attributesOffset = subeloffset + 0x70;
            ulong attribseclen = 8 + (ulong)(0x20 * attributes.Length);
            attributesSequenceOffset = attributesOffset + attribseclen;
            int attribseqseclen = attributes.Length;
            attribseqseclen = attribseqseclen % 4 == 0 ? attribseqseclen : attribseqseclen + (4 - (attribseqseclen % 4));
            attribseqseclen *= 2;
            paramsOffset = attributesSequenceOffset + (ulong)attribseqseclen;
        }


        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
            ofc.AddData(14, new REString(_class));
            for (int i = 0; i < attributes.Length; i++)
            {
                attributes[i].CollectData(ofc);
            }
            for (int i = 0; i < subparams.Length; i++)
            {
                subparams[i].CollectData(ofc);
            }
            CollectPadding4Elems(ofc);
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
            classOffset = ofc.GetOffset(14, new REString(_class));
            for (int i = 0; i < attributes.Length; i++)
            {
                attributes[i].GetCollectedData(ofc);
            }
            for (int i = 0; i < subparams.Length; i++)
            {
                subparams[i].GetCollectedData(ofc);
            }
            GetCollectedPadding4Elems(ofc);
        }

        public void CollectPadding4Elems(OffsetDataCollector ofc)
        {
            if (padding4elements != null)
            {
                if (padding4elements.Length != 0)
                {
                    ofc.AddSubelement4Dat(SubElementPadding4Element.ToBytes(padding4elements));
                }
            }
            else if (unknownDat != null)
                if (unknownDat.Length != 0)
                {
                    ofc.AddSubelement4Dat(unknownDat);
                }
        }

        public void GetCollectedPadding4Elems(OffsetDataCollector ofc)
        {
            padding4elementsOffset = 0;
            if (padding4elements != null) {
                if (padding4elements.Length != 0)
                {
                    padding4elementsOffset = ofc.GetSubel4pOffset(SubElementPadding4Element.ToBytes(padding4elements));
                }
            }
            else if (unknownDat != null)
                if (unknownDat.Length != 0)
                {
                    padding4elementsOffset = ofc.GetSubel4pOffset(unknownDat);
                }
        }

        public void RestoreAttributesOrder()
        {
            padding3 = 0;
            attributesCount = (ulong)attributes.Length;
            SortBackAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                attributes[i].RestoreAfterDeserialize();
            }
            return;
        }

        public void RestoreParamHashes()
        {
            if (subparams == null)
                return;
            for (int i = 0; i < subparams.Length; i++)
            {
                subparams[i].RestoreHash();
            }
        }

        public void ReadAttrSequence(BinaryReader br)
        {
            attributesSequence = new ushort[attributesCount];
            ulong paddingLen = (attributesCount * 2) % 8;
            paddingLen = paddingLen == 0 ? paddingLen : 8 - paddingLen;
            for (int i = 0; i < (int)attributesCount; i++)
            {
                attributesSequence[i] = br.ReadUInt16();
            }
            sequencePadding = br.ReadBytes((int)paddingLen);
        }

        public void ReadParams(BinaryReader br)
        {
            br.BaseStream.Position = (long)paramsOffset;
            long paramslen = br.ReadInt64();
            subparams = new SubElementParam[paramslen];
            for (int i = 0; i < (int)paramslen; i++)
            {
                subparams[i] = new SubElementParam(br);
            }
        }

        public void ReadPadding4Elem(BinaryReader br)
        {
            if (padding4elementsOffset == 0)
                return;
            br.BaseStream.Position = (long)padding4elementsOffset;
            int paramslen = br.ReadByte();
            if (paramslen == 0)
            {
                br.BaseStream.Position -= 1;
                unknownDat = br.ReadBytes(160);
                return;
            }
            br.BaseStream.Position += 3;
            padding4elements = new SubElementPadding4Element[paramslen];
            for (int i = 0; i < (int)paramslen; i++)
            {
                padding4elements[i] = new SubElementPadding4Element(br);
            }
        }

        public void SortAttributes()
        {
            Attribute[] tmp = new Attribute[attributesCount];
            for (int i = 0; i < (int)attributesCount; i++)
            {
                tmp[i] = attributes[attributesSequence[i]];
            }
            attributes = tmp;
        }

        public void SortBackAttributes()
        {
            Attribute[] tmp = new Attribute[attributesCount];
            for (int i = 0; i < (int)attributesCount; i++)
            {
                tmp[attributesSequence[i]] = attributes[i];
            }
            attributes = tmp;
        }

        public void Show()
        {
            //Console.WriteLine("Hash: " + REID.ByteArrayToString(GiantHash));
            Console.WriteLine("subIndex: " + subIndex);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine("name: " + name);
            Console.WriteLine("class: " + _class);
            Console.WriteLine("attributes: ");
            for (int i = 0; i < (int)attributesCount; i++)
            {
                attributes[i].Show();
            }
            Console.WriteLine("Params:");
            for (int i = 0; i < (int)subparams.Length; i++)
            {
                subparams[i].Show();
            }
            if (padding4elementsOffset != 0)
            {
                if (padding4elements == null)
                {
                    Console.WriteLine("unknowndat:" + REID.ByteArrayToString(unknownDat));
                }
                else
                {
                    Console.WriteLine("Padding4 elements:");
                    for (int i = 0; i < (int)padding4elements.Length; i++)
                    {
                        padding4elements[i].Show();
                    }
                }
            }
            Console.WriteLine("Unknown1: " + unknown1);
        }

        public void Save(BinaryWriter bw)
        {
            //SortBackAttributes();
            bw.Write(thisHash);
            bw.Write(elementHash);
            bw.Write(unknownHash);
            bw.Write(nameOffset);
            bw.Write(classOffset);
            bw.Write(attributesOffset);
            bw.Write(attributesSequenceOffset);
            bw.Write(paramsOffset);
            bw.Write(padding3);
            bw.Write(padding4elementsOffset);
            bw.Write(unknown1);
            bw.BaseStream.Position = (long)attributesOffset;
            bw.Write(attributesCount);
            for (int i = 0; i < (int)attributesCount; i++)
            {
                attributes[i].Save(bw);
            }
            bw.BaseStream.Position = (long)attributesSequenceOffset;
            for (int i = 0; i < (int)attributesCount; i++)
            {
                bw.Write(attributesSequence[i]);
            }
            bw.BaseStream.Position = (long)paramsOffset;
            bw.Write((ulong)subparams.Length);
            for (int i = 0; i < (int)subparams.Length; i++)
            {
                subparams[i].Save(bw);
            }
        }
    }
}
