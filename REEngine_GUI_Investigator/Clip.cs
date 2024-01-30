using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class Clip
    {
        uint magic = 0x50494c43;
        [XmlAttribute]
        public int version;
        [XmlAttribute]
        public float clipduration;
        int subelementscount;
        int ClipAttributecount;
        int ValEntrycount;
        ulong subelementsoffset;
        ulong ClipAttributeoffset;
        ulong ValEntryoffset;
        ulong utf8stringsoffset1; //for some reason there are five equal offsets
        ulong vector4soffset;
        ulong utf8stringsoffset3;
        ulong utf8stringsoffset4;
        ulong utf8stringsoffset;
        ulong utf16stringsoffset;
        ulong lengthoffile;
        ulong padding;
        SubElementEntry[] subElements;
        ClipAttribute[] ClipAttributes;
        ValEntry[] ValEntrys;
        public SubElementEntry elem;
        byte[] vector4dat;
        byte[] utf8strings;
        byte[] utf16strings;

        public ulong GetLengthOfFile()
        {
            return lengthoffile;
        }

        public Clip(BinaryReader br)
        {
            Read(br);
        }

        public Clip()
        {
        }

        public Clip(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
                Read(br);
        }

        public void Read(BinaryReader br)
        {
            uint m = br.ReadUInt32();
            if (m != magic)
                throw new Exception("Wrong magic");
            version = br.ReadInt32();
            clipduration = br.ReadSingle();
            subelementscount = br.ReadInt32();
            ClipAttributecount = br.ReadInt32();
            ValEntrycount = br.ReadInt32();
            subelementsoffset = br.ReadUInt64();
            ClipAttributeoffset = br.ReadUInt64();
            ValEntryoffset = br.ReadUInt64();
            utf8stringsoffset1 = br.ReadUInt64();
            vector4soffset = br.ReadUInt64();
            utf8stringsoffset3 = br.ReadUInt64();
            utf8stringsoffset4 = br.ReadUInt64();
            utf8stringsoffset = br.ReadUInt64();
            bool sh = false;
            if (utf8stringsoffset != utf8stringsoffset3 || vector4soffset != utf8stringsoffset1)
            {
                sh = true;
                //throw new Exception("new CLIP place found");
            }
            utf16stringsoffset = br.ReadUInt64();
            lengthoffile = br.ReadUInt64();
            padding = br.ReadUInt64();
            subElements = new SubElementEntry[subelementscount];
            for (int i = 0; i < subelementscount; i++)
            {
                subElements[i] = new SubElementEntry(br);
            }
            elem = subElements[0];
            if (elem.GetSubElementspointer() != 1 && elem.GetSubElementspointer() != 0)
            {
                throw new Exception("SubElementEntry[0]: SubElementspointer != 1 && != 0. Further investigation is required");
            }
            if (elem.GetSubElementsCount() != subelementscount - 1)
            {
                throw new Exception("SubElementEntry[0]: SubElementsCount != gui.subelementscount - 1. Further investigation is required");
            }
            ClipAttributes = new ClipAttribute[ClipAttributecount];
            for (int i = 0; i < ClipAttributecount; i++)
            {
                ClipAttributes[i] = new ClipAttribute(br);
            }
            ValEntrys = new ValEntry[ValEntrycount];
            for (int i = 0; i < ValEntrycount; i++)
            {
                ValEntrys[i] = new ValEntry(br);
            }
            vector4dat = br.ReadBytes((int)(utf8stringsoffset3 - vector4soffset));
            utf8strings = br.ReadBytes((int)(utf16stringsoffset - utf8stringsoffset));
            utf16strings = br.ReadBytes((int)(lengthoffile - utf16stringsoffset));
            SetSubElements();
            SetAttributes();
            if (sh)
            {
                Show();
                Console.ReadLine();
            }
        }

        public void SetSubElements()
        {
            for (int i = 0; i < subelementscount; i++)
            {
                SubElementEntry sub = subElements[i];
                ulong namep = sub.GetNamePointer();
                sub.name = GetText(namep, true);
                SubElementEntry[] subs = new SubElementEntry[sub.GetSubElementsCount()];
                long st = (long)sub.GetSubElementspointer();
                for (long j = 0; j < subs.Length; j++)
                    subs[j] = subElements[j + st];
                sub.includedSubs = subs;
                ClipAttribute[] attrs = new ClipAttribute[sub.GetClipAttributesCount()];
                st = (long)sub.GetClipAttributepointer();
                for (long j = 0; j < attrs.Length; j++)
                    attrs[j] = ClipAttributes[j + st];
                sub.clipattrs = attrs;
            }
        }

        public void SetAttributes()
        {
            for (int i = 0; i < ClipAttributecount; i++)
            {
                ClipAttribute sub = ClipAttributes[i];
                ulong namep = sub.GetNamePointer();
                sub.name = GetText(namep, false);
                if (GetIsSeparated(sub.type))
                {
                    ClipAttribute[] subs = new ClipAttribute[sub.GetEntriesLen()];
                    long st = (long)sub.GetPointer();
                    for (long j = 0; j < subs.Length; j++)
                        subs[j] = ClipAttributes[j + st];
                    sub.clipattrs = subs;
                    sub.valchanges = new ValEntry[0];
                }
                else
                {
                    ValEntry[] vals = new ValEntry[sub.GetEntriesLen()];
                    long st = (long)sub.GetPointer();
                    for (long j = 0; j < vals.Length; j++)
                    {
                        vals[j] = ValEntrys[j + st];
                        vals[j].Setissep(REEngineValues.GetIsOffset(sub.type));
                    }
                    sub.valchanges = vals;
                    sub.SetVals(utf8strings, utf16strings, vector4dat);
                    sub.clipattrs = new ClipAttribute[0];
                }
            }
        }

        public void ShowAllImortantData()
        {
            elem.Show();
        }

        public string GetText(ulong pointer, bool UTF16)
        {
            byte[] textdat = utf8strings;
            Encoding enc = Encoding.UTF8;
            List<byte> bytes = new List<byte>();
            int pointer1 = (int)pointer;
            int add = 1;
            if (UTF16)
            {
                textdat = utf16strings;
                add = 2;
                enc = Encoding.Unicode;
                pointer1 *= 2;
            }
            for (int i = pointer1; textdat[i] != 0; i += add)
            {
                bytes.Add(textdat[i]);
                if (UTF16)
                    bytes.Add(textdat[i + 1]);
            }
            return enc.GetString(bytes.ToArray());
        }
        public void Save(BinaryWriter bw)
        {
            //RestoreAfterDeserialize();
            bw.Write(magic);
            bw.Write(version);
            bw.Write(clipduration);
            bw.Write(subelementscount);
            bw.Write(ClipAttributecount);
            bw.Write(ValEntrycount);
            bw.Write(subelementsoffset);
            bw.Write(ClipAttributeoffset);
            bw.Write(ValEntryoffset);
            bw.Write(utf8stringsoffset1);
            bw.Write(vector4soffset);
            bw.Write(utf8stringsoffset3);
            bw.Write(utf8stringsoffset4);
            bw.Write(utf8stringsoffset);
            bw.Write(utf16stringsoffset);
            bw.Write(lengthoffile);
            bw.Write(padding);
            for (int i = 0; i < subelementscount; i++)
            {
                subElements[i].Save(bw);
            }
            for (int i = 0; i < ClipAttributecount; i++)
            {
                ClipAttributes[i].Save(bw);
            }
            for (int i = 0; i < ValEntrycount; i++)
            {
                ValEntrys[i].Save(bw);
            }
            bw.Write(vector4dat);
            bw.Write(utf8strings);
            bw.Write(utf16strings);
        }

        public void RestoreAfterDeserialize()
        {
            //Console.WriteLine("restoring elements arrays");
            subElements = elem.GetSubElements();
            List<ClipAttribute> clattrs = new List<ClipAttribute>();
            foreach (SubElementEntry sub in subElements)
            {
                clattrs.AddRange(sub.GetAttributes());
            }
            ClipAttributes = clattrs.ToArray();
            List<ValEntry> vals = new List<ValEntry>();
            foreach (ClipAttribute cla in ClipAttributes)
            {
                vals.AddRange(cla.GetValEntries());
            }
            ValEntrys = vals.ToArray();


            //Console.WriteLine("restoring metadata's lists");
            List<string> wsarl = new List<string>();
            foreach (SubElementEntry sub in subElements)
            {
                wsarl.Add(sub.name);
            }
            foreach (ClipAttribute cla in ClipAttributes)
            {
                wsarl.AddRange(cla.GetwStrings());
            }
            REwString.DeleteDuplicates(wsarl);
            List<string> sar8l = new List<string>();
            foreach (ClipAttribute cla in ClipAttributes)
            {
                sar8l.Add(cla.name);
            }
            foreach (ClipAttribute cla in ClipAttributes)
            {
                sar8l.AddRange(cla.Get8Strings());
            }
            REwString.DeleteDuplicates(sar8l);
            List<Vector4> vecsl = new List<Vector4>();
            foreach (ValEntry ent in ValEntrys)
            {
                Vector4 d = ent.GetVector4();
                if (d != null)
                    vecsl.Add(d);
            }
            //Vector4.DeleteDuplicates(vecsl);
            string[] wsar = wsarl.ToArray();
            string[] sar8 = sar8l.ToArray();
            Vector4[] vecs = vecsl.ToArray();


            //Console.WriteLine("restoring metadata's binary lists");
            int[] wsaroffsets = new int[wsar.Length];
            int[] sar8offsets = new int[sar8.Length];
            int offs = 0;
            for (int i = 0; i < wsar.Length; i++)
            {
                wsaroffsets[i] = offs;
                offs += wsar[i].Length + 1;
            }
            int len = offs % 4 == 0 ? offs : (offs + 4 - offs % 4);
            utf16strings = new byte[len * 2];
            using (MemoryStream ms = new MemoryStream(utf16strings))
            using (BinaryWriter bw = new BinaryWriter(ms))
                for (int i = 0; i < wsar.Length; i++)
                {
                    new REwString(wsar[i]).Save(bw);
                }
            offs = 0;
            for (int i = 0; i < sar8.Length; i++)
            {
                sar8offsets[i] = offs;
                offs += sar8[i].Length + 1;
            }
            len = offs % 8 == 0 ? offs : (offs + 8 - offs % 8);
            utf8strings = new byte[len];
            using (MemoryStream ms = new MemoryStream(utf8strings))
            using (BinaryWriter bw = new BinaryWriter(ms))
                for (int i = 0; i < sar8.Length; i++)
                {
                    new REString(sar8[i]).Save(bw);
                }
            vector4dat = new byte[vecs.Length * 16];
            using (MemoryStream ms = new MemoryStream(vector4dat))
            using (BinaryWriter bw = new BinaryWriter(ms))
                for (int i = 0; i < vecs.Length; i++)
                {
                    vecs[i].Save(bw);
                }

            //Console.WriteLine("restoring references");
            int attrc = 0;
            int subc = 0;
            foreach (SubElementEntry sub in subElements)
            {
                sub.RestoreReferences(subc, attrc);
                sub.SetNamePointer((ulong)wsaroffsets[Array.IndexOf(wsar, sub.name)]);
                attrc += sub.GetAttributes().Length;
                subc++;
            }
            attrc = 0;
            int valec = 0;
            foreach (ClipAttribute cla in ClipAttributes)
            {
                cla.RestoreReferences(attrc, valec);
                cla.SetNamePointer((ulong)sar8offsets[Array.IndexOf(sar8, cla.name)]);
                valec += cla.GetValEntries().Length;
                string[] valstrs = cla.GetwStrings(); //setting references to strings in vals
                ulong[] valstroffss = new ulong[valstrs.Length];
                for (int i = 0; i < valstrs.Length; i++)
                {
                    valstroffss[i] = (ulong)wsaroffsets[Array.IndexOf(wsar, valstrs[i])];
                }
                cla.SetwStrings(valstroffss);
                valstrs = cla.Get8Strings();
                valstroffss = new ulong[valstrs.Length];
                for (int i = 0; i < valstrs.Length; i++)
                {
                    valstroffss[i] = (ulong)sar8offsets[Array.IndexOf(sar8, valstrs[i])];
                }
                cla.Set8Strings(valstroffss);
                attrc++;
            }

            //Console.WriteLine("restoring header data");
            subelementscount = subElements.Length;
            ClipAttributecount = ClipAttributes.Length;
            ValEntrycount = ValEntrys.Length;
            subelementsoffset = 0x70;
            padding = 0;
            ClipAttributeoffset = subelementsoffset + (ulong)(40 * subelementscount);
            ValEntryoffset = ClipAttributeoffset + (ulong)(56 * ClipAttributecount);
            utf8stringsoffset1 = ValEntryoffset + (ulong)(32 * ValEntrycount);
            vector4soffset = utf8stringsoffset1;
            utf8stringsoffset3 = vector4soffset + (ulong)(16 * vecs.Length);
            utf8stringsoffset4 = utf8stringsoffset3;
            utf8stringsoffset = utf8stringsoffset4;
            utf16stringsoffset = utf8stringsoffset + (ulong)utf8strings.Length;
            lengthoffile = utf16stringsoffset + (ulong)utf16strings.Length;
        }

        public void Show()
        {
            elem.Show();
        }

        public static bool GetIsSeparated(int type)
        {
            switch (type)
            {
                case 1:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 13:
                case 14:
                case 32:
                case 34:
                case 43:
                    return false;
                case 21:
                case 22:
                case 23:
                case 24:
                case 26:
                case 27:
                case 28:
                case 31:
                case 35:
                    return true;
                default:
                    throw new Exception("Unknown attribute type: " + type);
            }
        }

        public class SubElementEntry
        {
            ushort SubElementsCount;
            ushort ClipAttributesCount;
            uint padding;
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public uint hash1;
            uint namehash;
            ulong namepointer;
            ulong SubElementspointer;
            ulong ClipAttributepointer;
            public SubElementEntry[] includedSubs;
            public ClipAttribute[] clipattrs;

            public SubElementEntry()
            {
            }

            public SubElementEntry(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                SubElementsCount = br.ReadUInt16();
                ClipAttributesCount = br.ReadUInt16();
                padding = br.ReadUInt32();
                if (padding != 0)
                {
                    throw new Exception("SubElementEntry: padding != 0. Further investigation is required");
                }
                hash1 = br.ReadUInt32();
                namehash = br.ReadUInt32();
                namepointer = br.ReadUInt64();
                SubElementspointer = br.ReadUInt64();
                ClipAttributepointer = br.ReadUInt64();
            }

            public void Save(BinaryWriter bw)
            {
                bw.Write(SubElementsCount);
                bw.Write(ClipAttributesCount);
                bw.Write(padding);
                bw.Write(hash1);
                bw.Write(namehash);
                bw.Write(namepointer);
                bw.Write(SubElementspointer);
                bw.Write(ClipAttributepointer);
            }

            public void Show()
            {
                Console.WriteLine("hash1: " + hash1);
                Console.WriteLine("name: " + name);
                Console.WriteLine("Included SubElements:");
                foreach (SubElementEntry sub in includedSubs)
                    sub.Show();
                Console.WriteLine("Included Attributes:");
                foreach (ClipAttribute attr in clipattrs)
                    attr.Show();
            }

            public ulong GetNamePointer()
            {
                return namepointer;
            }

            public ulong GetClipAttributePointer()
            {
                return ClipAttributepointer;
            }

            public ushort GetSubElementsCount()
            {
                return SubElementsCount;
            }

            public ushort GetClipAttributesCount()
            {
                return ClipAttributesCount;
            }

            public ulong GetSubElementspointer()
            {
                return SubElementspointer;
            }

            public ulong GetClipAttributepointer()
            {
                return ClipAttributepointer;
            }

            public SubElementEntry[] GetSubElements()
            {
                SubElementEntry[] res = new SubElementEntry[includedSubs.Length + 1];
                res[0] = this;
                for (int i = 0; i < includedSubs.Length; i++)
                {
                    res[i + 1] = includedSubs[i];
                }
                return res;
            }

            public ClipAttribute[] GetAttributes()
            {
                List<ClipAttribute> attrs = new List<ClipAttribute>();
                attrs.AddRange(GetDirectAttributes());
                foreach(ClipAttribute cla in clipattrs)
                {
                    attrs.AddRange(cla.GetDirectAttributesForSort());
                }
                return attrs.ToArray();
            }

            public ClipAttribute[] GetDirectAttributes()
            {
                if (clipattrs == null)
                    return new ClipAttribute[0];
                return clipattrs;
            }

            public void RestoreReferences(int subindex, int attrindex)
            {
                if (includedSubs != null)
                {
                    SubElementsCount = (ushort)includedSubs.Length;
                    if (includedSubs.Length != 0)
                        SubElementspointer = (ulong)subindex + 1;
                    else SubElementspointer = 0;
                }
                else
                {
                    SubElementspointer = 0;
                    SubElementsCount = 0;
                }
                if (clipattrs != null)
                {
                    ClipAttributesCount = (ushort)clipattrs.Length;
                    if (clipattrs.Length != 0)
                        ClipAttributepointer = (ulong)attrindex;
                    else ClipAttributepointer = 0;
                }
                else
                {
                    ClipAttributepointer = 0;
                    ClipAttributesCount = 0;
                }
                padding = 0;
                namehash = MurMurHash3.Hash(name, false);
                if (clipattrs != null)
                {
                    int sepindex = attrindex + clipattrs.Length;
                    for (int i = 0; i < clipattrs.Length; i++)
                    {
                        ClipAttribute sub = clipattrs[i];
                        if (GetIsSeparated(sub.type))
                        {
                            sub.RestoreAttrReferences(sepindex);
                            sepindex += sub.GetDirectAttributes().Length;
                        }
                    }
                }
            }

            public void SetNamePointer(ulong val)
            {
                namepointer = val;
            }
        }

        public class ClipAttribute
        {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public byte type;
            [XmlAttribute]
            public float startTime;
            [XmlAttribute]
            public float finishTime;
            [XmlAttribute]
            public uint hash1;
            [XmlAttribute]
            public uint namehash;
            ulong namepointer;
            ulong unknown1; //usually 0
            ulong pointer;
            ushort entrieslen;
            short end; //-1
            byte padding;
            ushort unknown3;
            ulong unknown4; //usually 0
            public ClipAttribute[] clipattrs;
            public ValEntry[] valchanges;

            public ClipAttribute()
            {
            }

            public ClipAttribute(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                startTime = br.ReadSingle();
                finishTime = br.ReadSingle();
                hash1 = br.ReadUInt32();
                namehash = br.ReadUInt32();
                namepointer = br.ReadUInt64();
                unknown1 = br.ReadUInt64();
                if (unknown1 != 0)
                {
                    throw new Exception("ClipAttribute: unknown1 != 0. Further investigation is required");
                }
                pointer = br.ReadUInt64();
                entrieslen = br.ReadUInt16();
                end = br.ReadInt16();
                if (end != -1)
                {
                    throw new Exception("ClipAttribute: end != -1. Further investigation is required");
                }
                padding = br.ReadByte();
                if (padding != 0)
                {
                    throw new Exception("ClipAttribute: padding != 0. Further investigation is required");
                }
                type = br.ReadByte();
                unknown3 = br.ReadUInt16();
                if (unknown3 != 0)
                {
                    throw new Exception("ClipAttribute: unknown3 != 0. Further investigation is required");
                }
                unknown4 = br.ReadUInt64();
                if (unknown4 != 0)
                {
                    throw new Exception("ClipAttribute: unknown4 != 0. Further investigation is required");
                }
            }

            public void Save(BinaryWriter bw)
            {
                bw.Write(startTime);
                bw.Write(finishTime);
                bw.Write(hash1);
                bw.Write(namehash);
                bw.Write(namepointer);
                bw.Write(unknown1);
                bw.Write(pointer);
                bw.Write(entrieslen);
                bw.Write(end);
                bw.Write(padding);
                bw.Write(type);
                bw.Write(unknown3);
                bw.Write(unknown4);
            }

            public void SetVals(byte[] string8dat, byte[] string16dat, byte[] vector4dat)
            {
                foreach (ValEntry val1 in valchanges)
                {
                    val1.SetVal(type, string8dat, string16dat);
                    val1.SetAdditionalVector(vector4dat);
                }
            }

            public uint GetUNKHash()
            {
                return hash1;
            }

            public void Show()
            {
                Console.WriteLine("startTime: " + startTime);
                Console.WriteLine("finishTime: " + finishTime);
                Console.WriteLine("hash1: " + hash1);
                Console.WriteLine("name: " + name);
                Console.WriteLine("type: " + type);
                Console.WriteLine("Included Attributes:");
                foreach (ClipAttribute attr in clipattrs)
                    attr.Show();
                Console.WriteLine("Included Values:");
                foreach (ValEntry val1 in valchanges)
                    val1.Show();
            }

            public ulong GetNamePointer()
            {
                return namepointer;
            }

            public ulong GetPointer()
            {
                return pointer;
            }

            public uint GetEntriesLen()
            {
                return entrieslen;
            }

            public float GetstartTime()
            {
                return startTime;
            }

            public float GetfinishTime()
            {
                return finishTime;
            }

            public ClipAttribute[] GetAttributes()
            {
                if (clipattrs != null)
                {
                    if (clipattrs.Length != 0)
                    {
                        entrieslen = (ushort)clipattrs.Length;
                        return clipattrs;
                    }
                }
                return new ClipAttribute[0];
            }

            public ClipAttribute[] GetDirectAttributesForSort()
            {
                List<ClipAttribute> attrs = new List<ClipAttribute>();
                attrs.AddRange(GetDirectAttributes());
                foreach (ClipAttribute cla in clipattrs)
                {
                    attrs.AddRange(cla.GetDirectAttributesForSort());
                }
                return attrs.ToArray();
            }

            public ClipAttribute[] GetDirectAttributes()
            {
                if (clipattrs == null)
                    return new ClipAttribute[0];
                return clipattrs;
            }

            public ValEntry[] GetValEntries()
            {
                if (valchanges != null)
                {
                    if (valchanges.Length != 0)
                    {
                        entrieslen = (ushort)valchanges.Length;
                        return valchanges;
                    }
                }
                return new ValEntry[0];
            }

            public string[] GetwStrings()
            {
                if (type == 13 || type == 32)
                {
                    string[] res = new string[entrieslen];
                    for (int i = 0; i < entrieslen; i++)
                    {
                        ValEntry ent = valchanges[i];
                        res[i] = ((REwString)ent.val).str;
                    }
                    return res;
                }
                return new string[0];
            }

            public void SetwStrings(ulong[] refs)
            {
                if (type == 13 || type == 32)
                {
                    for (int i = 0; i < entrieslen; i++)
                    {
                        ValEntry ent = valchanges[i];
                        ent.Setissep(true);
                        ent.SetDat(REID.LongToByteArray(refs[i]));
                    }
                }
                else if (!GetIsSeparated(type))
                    for (int i = 0; i < entrieslen; i++)
                    {
                        ValEntry ent = valchanges[i];
                        ent.Setissep(false);
                    }
            }

            public string[] Get8Strings()
            {
                if (type == 14)
                {
                    string[] res = new string[entrieslen];
                    for (int i = 0; i < entrieslen; i++)
                    {
                        ValEntry ent = valchanges[i];
                        res[i] = ((REwString)ent.val).str;
                    }
                    return res;
                }
                return new string[0];
            }

            public void Set8Strings(ulong[] refs)
            {
                if (type == 14)
                {
                    for (int i = 0; i < entrieslen; i++)
                    {
                        ValEntry ent = valchanges[i];
                        ent.Setissep(true);
                        ent.SetDat(REID.LongToByteArray(refs[i]));
                    }
                }
            }

            public void RestoreAttrReferences(int attrindex)
            {
                if (GetIsSeparated(type))
                {
                    pointer = (ulong)attrindex;
                    entrieslen = (ushort)clipattrs.Length;
                    int sepindex = attrindex + clipattrs.Length;
                    for (int i = 0; i < clipattrs.Length; i++)
                    {
                        ClipAttribute sub = clipattrs[i];
                        if (GetIsSeparated(sub.type))
                        {
                            sub.RestoreAttrReferences(sepindex);
                            sepindex += sub.GetDirectAttributes().Length;
                        }
                    }
                }
            }

            public void RestoreReferences(int attrindex, int valindex)
            {
                padding = 0;
                unknown1 = 0;
                unknown3 = 0;
                unknown4 = 0;
                end = -1;
                if (GetIsSeparated(type))
                {
                    return;
                } else if (valchanges != null)
                {
                    if (valchanges.Length != 0)
                    {
                        entrieslen = (ushort)valchanges.Length;
                        pointer = (ulong)valindex;
                    }
                    else
                    {
                        entrieslen = 0;
                        pointer = 0;
                    }
                }
                else
                {
                    entrieslen = 0;
                    pointer = 0;
                }
            }

            public void SetNamePointer(ulong val)
            {
                namepointer = val;
            }
        }



        public class ValEntry
        {
            [XmlAttribute]
            public float time;
            [XmlAttribute]
            public uint unknown1; //usually 0
            [XmlAttribute]
            public uint unknown2; //usually 0
            [XmlAttribute]
            public uint unknown3; //usually 0
            bool issep;
            byte[] dat;
            [XmlAttribute]
            public ulong additionalType;
            public REEngineValues val;
            public REEngineValues additionalVal;

            public ValEntry()
            {
            }

            public ValEntry(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                time = br.ReadSingle();
                unknown1 = br.ReadUInt32();
                if (unknown1 != 0)
                {
                    throw new Exception("ValEntry: unknown1 != 0. Further investigation is required");
                }
                unknown2 = br.ReadUInt32();
                unknown3 = br.ReadUInt32();
                dat = br.ReadBytes(8);
                additionalType = br.ReadUInt64();
            }

            public void Setissep(bool b)
            {
                issep = b;
            }

            public void Save(BinaryWriter bw)
            {
                long st = bw.BaseStream.Position;
                bw.Write(time);
                bw.Write(unknown1);
                bw.Write(unknown2);
                bw.Write(unknown3);
                if (issep)
                {
                    bw.Write(dat);
                }
                else
                {
                    val.Save(bw);
                }
                bw.Write(additionalType);
                long end = bw.BaseStream.Position;
                if (end - st != 0x20)
                    throw new Exception("we got em captain");
            }

            public void Show()
            {
                Console.WriteLine("time: " + time);
                Console.WriteLine("unknown1: " + unknown1);
                Console.WriteLine("unknown2: " + unknown2);
                Console.WriteLine("unknown3: " + unknown3);
                Console.WriteLine("value: " + val.GetValString());
                Console.WriteLine("additionalType: " + additionalType);
            }

            public void SetDat(byte[] dat1)
            {
                dat = dat1;
            }

            public void SetAdditionalVector(byte[] vectordat)
            {
                if (unknown2 == 5)
                {
                    //Console.WriteLine("Записываем допвектор " + additionalType);
                    byte[] curvectordat = new byte[16];
                    int st = (int)additionalType * 16;
                    for (int i = 0; i < 16; i++)
                    {
                        curvectordat[i] = vectordat[st + i];
                    }
                    using (MemoryStream ms = new MemoryStream(curvectordat))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        additionalVal = REEngineValues.GetValue(28, br);
                    }
                }
            }

            public void SetVal(int type, byte[] stringutf8dat, byte[] stringutf16dat)
            {
                if (type == 13 || type == 32)
                {
                    ulong offs = REID.ByteArrayToLong(dat);
                    offs *= 2;
                    using (MemoryStream ms = new MemoryStream(stringutf16dat))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        br.ReadBytes((int)offs);
                        val = REEngineValues.GetValue(type, br);
                    }
                    return;
                } else if (type == 14)
                {
                    ulong offs = REID.ByteArrayToLong(dat);
                    using (MemoryStream ms = new MemoryStream(stringutf8dat))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        br.ReadBytes((int)offs);
                        val = REEngineValues.GetValue(type, br);
                    }
                    return;
                }
                using (MemoryStream ms = new MemoryStream(dat))
                using (BinaryReader br = new BinaryReader(ms))
                    val = REEngineValues.GetValue(type, br);
            }

            public float Gettime()
            {
                return time;
            }

            public double GetDouble()
            {
                using (MemoryStream ms = new MemoryStream(dat))
                using (BinaryReader br = new BinaryReader(ms))
                    return br.ReadDouble();
            }

            public long GetLong()
            {
                using (MemoryStream ms = new MemoryStream(dat))
                using (BinaryReader br = new BinaryReader(ms))
                    return br.ReadInt64();
            }

            public Vector4 GetVector4()
            {
                if (unknown2 == 5)
                {
                    //Console.WriteLine("Получаем допвектор " + additionalType);
                    return (Vector4)additionalVal;
                }
                return null;
            }
        }
    }
}
