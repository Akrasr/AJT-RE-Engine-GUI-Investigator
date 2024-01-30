using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace REEngine_GUI_Investigator
{
    public class OffsetDataCollector
    {
        List<REEngineValues>[] collectedData = new List<REEngineValues>[44];
        ulong[][] collectedoffsets = new ulong[44][];
        List<byte[]> subelp4dats = new List<byte[]>();
        public byte[] finalbytes;
        ulong[] subelp4offsets;
        ulong subelp4offset;
        public long end;

        public OffsetDataCollector() { }

        public OffsetDataCollector(BinaryReader br) { Read(br); }

        public void AddData(int type, REEngineValues val)
        {
            if (type == 32)
                type = 13;
            if (type == 34)
                type = 15;
            if (type == 26 || type == 31)
                type = 21;
            if (type == 27)
                type = 22;
            if (type == 28 || type == 43)
                type = 23;
            if (type == 35)
                type = 16;
            if (collectedData[type] == null)
                collectedData[type] = new List<REEngineValues>();
            collectedData[type].Add(val);
        }

        public void Read(BinaryReader br)
        {
            long length = br.BaseStream.Length - br.BaseStream.Position;
            end = br.BaseStream.Length;
            finalbytes = br.ReadBytes((int)length);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(finalbytes);
        }

        public void DeleteAllDuplicates()
        {
            for (int i = 0; i < collectedData.Length; i++)
            {
                if (REEngineValues.IsInvestigated(i))
                {
                    if (REEngineValues.GetIsOffset(i))
                    {
                        if (collectedData[i] != null)
                        {
                            REEngineValues.DeleteDuplicates(i, collectedData[i]);
                        }
                    }
                }
            }
        }

        public void SetOffsets(ulong startOffset)
        {
            ulong start = startOffset;
            DeleteAllDuplicates();
            //DeleteAllDuplicatesSubelement4Dat();
            for (int i = 0; i < collectedData.Length; i++)
            {
                if (REEngineValues.IsInvestigated(i))
                {
                    if (REEngineValues.GetIsOffset(i))
                    {
                        if (collectedData[i] != null)
                        {
                            collectedoffsets[i] = new ulong[collectedData[i].Count];
                            for (int j = 0; j < collectedData[i].Count; j++)
                            {
                                collectedoffsets[i][j] = startOffset;
                                startOffset += collectedData[i][j].GetDataLength();
                            }
                        }
                    }
                }
            }
            subelp4offset = startOffset;
            subelp4offsets = new ulong[subelp4dats.Count];
            for (int i = 0; i < subelp4dats.Count; i++)
            {
                subelp4offsets[i] = startOffset;
                startOffset += (ulong)subelp4dats[i].Length;
            }
            end = (long)startOffset;
            finalbytes = new byte[end - (long)start];
            using (MemoryStream ms = new MemoryStream(finalbytes))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                for (int i = 0; i < collectedData.Length; i++)
                {
                    if (REEngineValues.IsInvestigated(i))
                    {
                        if (REEngineValues.GetIsOffset(i))
                        {
                            if (collectedData[i] != null)
                            {
                                for (int j = 0; j < collectedData[i].Count; j++)
                                {
                                    collectedData[i][j].Save(bw);
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < subelp4dats.Count; i++)
                {
                    bw.Write(subelp4dats[i]);
                }
            }
        }

        public void AddSubelement4Dat(byte[] dat)
        {
            subelp4dats.Add(dat);
        }

        public void DeleteAllDuplicatesSubelement4Dat()
        {
            for (int i = 0; i < subelp4dats.Count; i++)
            {
                bool del = false;
                for (int j = 0; j < i; j++)
                {
                    if (CompareBytes(subelp4dats[i], subelp4dats[j]))
                    {
                        del = true;
                        break;
                    }
                }
                if (del)
                {
                    subelp4dats.RemoveAt(i);
                    i--;
                }
            }
        }

        public static bool CompareBytes(byte[] s1, byte[] s2)
        {
            bool res = true;
            if (s1.Length != s2.Length)
                return false;
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i])
                {
                    res = false;
                    break;
                }
            }
            return res;
        }

        public int GetIndexOf(byte[] dat)
        {
            for (int i = 0; i < subelp4dats.Count; i++)
            {
                if (CompareBytes(dat, subelp4dats[i]))
                    return i;
            }
            return -1;
        }

        public ulong GetSubel4pOffset(byte[] dat)
        {
            int ind = GetIndexOf(dat);
            if (ind == -1)
                throw new Exception("Subel4p " + dat + " was not found.");
            return subelp4offsets[ind];
        }

        public ulong GetOffset(int type, REEngineValues val)
        {
            if (type == 32)
                type = 13;
            if (type == 34)
                type = 15;
            if (type == 26 || type == 31)
                type = 21;
            if (type == 27)
                type = 22;
            if (type == 28 || type == 43)
                type = 23;
            if (type == 35)
                type = 16;
            if (REEngineValues.IsInvestigated(type))
            {
                if (REEngineValues.GetIsOffset(type))
                {
                    if (collectedData[type] != null)
                    {
                        int ind = REEngineValues.GetIndexOf(type, val, collectedData[type]);
                        if (ind == -1)
                            throw new Exception("Value " + val.GetValString() + " was not found. Type - " + type);
                        return collectedoffsets[type][ind];
                    }
                }
            }
            return 0;
        }
    }
}
