using System;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace REEngine_GUI_Investigator
{
    public class Element
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string _class;
        [XmlAttribute]
        public int len2;
        [XmlAttribute]
        public byte[] hash;
        ulong nameOffset;
        ulong classOffset;
        ulong subelementsListOffset;
        ulong clipEntriesListOffset;
        ulong[] subelementsOffsets;
        ulong[] clipEntriesOffsets;
        public SubElementView[] subelements;
        public ClipEntry[] clipEntries;

        public Element()
        {
        }

        public Element(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            hash = br.ReadBytes(16);
            nameOffset = br.ReadUInt64();
            classOffset = br.ReadUInt64();
            subelementsListOffset = br.ReadUInt64();
            clipEntriesListOffset = br.ReadUInt64();
            name = new REwString(br, nameOffset).str;
            _class = new REString(br, classOffset).str;
            ReadSubElements(br);
            ReadClipEntries(br);
        }

        public void ReadSubElements(BinaryReader br)
        {
            br.BaseStream.Position = (long)subelementsListOffset;
            long len = br.ReadInt64();
            subelementsOffsets = new ulong[len];
            subelements = new SubElementView[len];
            for (int i = 0; i < len; i++)
            {
                subelementsOffsets[i] = br.ReadUInt64();
            }
            for (int i = 0; i < len; i++)
            {
                br.BaseStream.Position = (long)subelementsOffsets[i];
                subelements[i] = new SubElementView(br);
            }
        }

        public void ReadClipEntries(BinaryReader br)
        {
            br.BaseStream.Position = (long)clipEntriesListOffset;
            int len1 = br.ReadInt32();
            len2 = br.ReadInt32();
            //if (len1 != len2)
            //    throw new Exception("Element: len1 != len2. Firther investigation is required.");
            clipEntriesOffsets = new ulong[len1];
            clipEntries = new ClipEntry[len1];
            for (int i = 0; i < len1; i++)
            {
                clipEntriesOffsets[i] = br.ReadUInt64();
            }
            for (int i = 0; i < len1; i++)
            {
                br.BaseStream.Position = (long)clipEntriesOffsets[i];
                //Console.WriteLine(name + "[" + i + "]");
                clipEntries[i] = new ClipEntry(br);
            }
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
            ofc.AddData(14, new REString(_class));
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
            classOffset = ofc.GetOffset(14, new REString(_class));
            for (int i = 0; i < subelements.Length; i++)
            {
                subelements[i].GetCollectedData(ofc);
            }
            for (int i = 0; i < clipEntries.Length; i++)
            {
                clipEntries[i].GetCollectedData(ofc);
            }
        }

        public void RestoreSubElementsOffsetsList(ulong[] offsetsList)
        {
            subelementsOffsets = new ulong[subelements.Length];
            for (int i = 0; i < subelements.Length; i++)
            {
                subelementsOffsets[i] = offsetsList[subelements[i].subIndex];
            }
        }

        public void RestoreClipsOffsetsList(ulong[] offsetsList)
        {
            clipEntriesOffsets = new ulong[clipEntries.Length];
            for (int i = 0; i < clipEntries.Length; i++)
            {
                clipEntriesOffsets[i] = offsetsList[clipEntries[i].clipIndex];
                clipEntries[i].RestoreClipEntryOffset(offsetsList);
            }
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            //Console.WriteLine("padding1: " + padding1);
            //Console.WriteLine("padding2: " + padding2);
            Console.WriteLine("name: " + name);
            Console.WriteLine("class: " + _class);
            Console.WriteLine("SubElements:");
            for (int i = 0; i < subelements.Length; i++)
            {
                Console.WriteLine("Subelements[" + i + "]:");
                subelements[i].Show();
            }
            Console.WriteLine("Clips:");
            for (int i = 0; i < clipEntries.Length; i++)
            {
                Console.WriteLine("clipEntries[" + i + "]:");
                clipEntries[i].Show();
            }
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(hash);
            bw.Write(nameOffset);
            bw.Write(classOffset);
            bw.Write(subelementsListOffset);
            bw.Write(clipEntriesListOffset);
            bw.BaseStream.Position = (long)subelementsListOffset;
            bw.Write((ulong)subelementsOffsets.Length);
            for (int i = 0; i < subelementsOffsets.Length; i++)
                bw.Write(subelementsOffsets[i]);
            bw.Write(clipEntriesOffsets.Length);
            bw.Write(len2);
            for (int i = 0; i < clipEntriesOffsets.Length; i++)
                bw.Write(clipEntriesOffsets[i]);
            for (int i = 0; i < subelementsOffsets.Length; i++)
            {
                bw.BaseStream.Position = (long)subelementsOffsets[i];
                subelements[i].Save(bw);
            }
            for (int i = 0; i < clipEntriesOffsets.Length; i++)
            {
                bw.BaseStream.Position = (long)clipEntriesOffsets[i];
                clipEntries[i].Save(bw);
            }
        }

        public void SetsubelementsListOffset(ulong off)
        {
            subelementsListOffset = off;
        }

        public void SetclipEntriesListOffset(ulong off)
        {
            clipEntriesListOffset = off;
        }

        public ulong[] GetSubElementOffsets()
        {
            return subelementsOffsets;
        }
        public ulong[] GetClipsOffsets()
        {
            return clipEntriesOffsets;
        }
    }
}
