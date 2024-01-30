using System;
using System.IO;

namespace REEngine_GUI_Investigator
{
    public class MetaData3
    {
        ulong subdataOffset;
        ulong subvarOffset;
        ulong unkdatOffset;
        public SubData[] subdatas;
        public SubVariables[] subvars;
        public UnknownData[] unk;

        public MetaData3()
        {
        }

        public MetaData3(BinaryReader br)
        {
            Read(br);
        }

        public void Read(BinaryReader br)
        {
            subdataOffset = br.ReadUInt64();
            subvarOffset = br.ReadUInt64();
            unkdatOffset = br.ReadUInt64();
            br.BaseStream.Position = (long)subdataOffset;
            long subdatacount = br.ReadInt64();
            subdatas = new SubData[subdatacount];
            for (int i = 0; i < subdatacount; i++)
            {
                subdatas[i] = new SubData(br);
            }
            br.BaseStream.Position = (long)subvarOffset;
            long subvarcount = br.ReadInt64();
            subvars = new SubVariables[subvarcount];
            for (int i = 0; i < subvarcount; i++)
            {
                subvars[i] = new SubVariables(br);
            }
            long unccount = br.ReadInt64();
            unk = new UnknownData[unccount];
            for (int i = 0; i < unccount; i++) 
                unk[i] = new UnknownData(br);
        }

        public void RestoreOffsets(ulong st)
        {
            subdataOffset = st + 24;
            subvarOffset = subdataOffset + 8 + (ulong)(subdatas.Length * 56);
            if (subdatas.Length == 0)
                subvarOffset += 8;
            unkdatOffset = subvarOffset + 8 + (ulong)(subvars.Length * 0x30);
            if (subvars.Length == 0)
                unkdatOffset += 8;
            foreach (SubData s in subdatas)
                s.RestoreAfterDeserialize();
            foreach (SubVariables s in subvars)
                s.RestoreAfterDeserialize();
            foreach (UnknownData s in unk)
                s.RestoreAfterDeserialize();
        }

        public void CollectData(OffsetDataCollector ofc)
        {
            for (int i = 0; i < subdatas.Length; i++)
            {
                subdatas[i].CollectData(ofc);
            }
            for (int i = 0; i < subvars.Length; i++)
            {
                subvars[i].CollectData(ofc);
            }
            for (int i = 0; i < unk.Length; i++)
            {
                unk[i].CollectData(ofc);
            }
        }

        public void GetCollectedData(OffsetDataCollector ofc)
        {
            for (int i = 0; i < subdatas.Length; i++)
            {
                subdatas[i].GetCollectedData(ofc);
            }
            for (int i = 0; i < subvars.Length; i++)
            {
                subvars[i].GetCollectedData(ofc);
            }
            for (int i = 0; i < unk.Length; i++)
            {
                unk[i].GetCollectedData(ofc);
            }
        }

        public ulong MeasureMetadata()
        {
            ulong res = 24;
            res += 8 + (ulong)(subdatas.Length * 56);
            if (subdatas.Length == 0)
                res += 8;
            res += 8 + (ulong)(subvars.Length * 0x30);
            if (subvars.Length == 0)
                res += 8;
            res += 8 + (ulong)(unk.Length * 0x30);
            if (unk.Length == 0)
                res += 8;
            return res;
        }

        public void Show()
        {
            Console.WriteLine("SubData: ");
            for (int i = 0; i < subdatas.Length; i++)
            {
                subdatas[i].Show();
            }
            Console.WriteLine("SubVariables: ");
            for (int i = 0; i < subvars.Length; i++)
            {
                subvars[i].Show();
            }
            Console.WriteLine("Unknown Data: ");
            for (int i = 0; i < unk.Length; i++)
                unk[i].Show();
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(subdataOffset);
            bw.Write(subvarOffset);
            bw.Write(unkdatOffset);
            bw.BaseStream.Position = (long)subdataOffset;
            bw.Write((long)subdatas.Length);
            for (int i = 0; i < subdatas.Length; i++)
            {
                subdatas[i].Save(bw);
            }
            bw.BaseStream.Position = (long)subvarOffset;
            bw.Write((long)subvars.Length);
            for (int i = 0; i < subvars.Length; i++)
            {
                subvars[i].Save(bw);
            }
            bw.BaseStream.Position = (long)unkdatOffset;
            bw.Write((long)unk.Length);
            for (int i = 0; i < unk.Length; i++)
            {
                unk[i].Save(bw);
            }
            //unk.Save(bw);
        }
    }
}
