using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class ClipEntry
    {
        [XmlAttribute]
        public int clipIndex;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public byte[] hash;
        [XmlAttribute]
        public int addclipIndex = -1;
        [XmlAttribute]
        public ulong unknown1;
        ulong nameOffset;
        ulong clipentryoffset = 0;
        public Clip clip;
        //ClipEntry additionalEntry;

        public ClipEntry()
        {
        }

        public ClipEntry(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            hash = br.ReadBytes(16);
            unknown1 = br.ReadUInt64();
            nameOffset = br.ReadUInt64();
            clipentryoffset = br.ReadUInt64();
            name = new REwString(br, nameOffset).str;
            clip = new Clip(br);
        }

        public void RestoreClipEntryOffset(ulong[] offsets)
        {
            if (addclipIndex != -1)
                clipentryoffset = offsets[addclipIndex];
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            ofc.AddData(13, new REwString(name));
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            nameOffset = ofc.GetOffset(13, new REwString(name));
        }

        public void Show()
        {
            Console.WriteLine("Hash: " + REID.ByteArrayToString(hash));
            Console.WriteLine("name: " + name);
            Console.WriteLine("Clip: ");
            clip.Show();
        }

        public ulong GetAdditionalOffset()
        {
            return clipentryoffset;
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(hash);
            bw.Write(unknown1);
            bw.Write(nameOffset);
            bw.Write(clipentryoffset);
            clip.Save(bw);
        }

        public void RestoreAfterDeserialize()
        {
            clip.RestoreAfterDeserialize();
        }

        public ulong MeasureClipEntry()
        {
            ulong res = 0x28;
            res += clip.GetLengthOfFile();
            return res;
        }
    }
}
