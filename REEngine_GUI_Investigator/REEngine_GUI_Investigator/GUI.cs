using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace REEngine_GUI_Investigator
{
    public class GUI
    {
        public GUIHeader Header;
        public Element[] elements;
        public SubElementView sew;
        public Metadata1Element[] md1dat;
        public MetaData3 md3;
        public string[] RequiredGuis;
        public string[] RequiredFiles;
        ulong[] elementsOffsets;
        ulong[] RequiredFilesOffsets;
        ulong[] RequiredGuisOffsets;
        ulong clipoffset;
        ulong OffsetInfoOffset;
        OffsetDataCollector ofc;

        public GUI()
        {
        }

        public GUI(string path)
        {
            Read(path);
        }

        public GUI(BinaryReader br)
        {
            Read(br);
        }
        public void Serialize(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer srl = new XmlSerializer(typeof(GUI));
                srl.Serialize(fs, this);
            }
        }

        public void Save(BinaryWriter bw)
        {
            //OffsetInfoOffset = Header.requiredfilesp + 8 + (ulong)(RequiredFiles.Length * 8);
            //RestoreAfterDeserialize();
            byte[] tmpbuf = new byte[ofc.end];
            using (MemoryStream ms = new MemoryStream(tmpbuf))
            using (BinaryWriter tmpwriter = new BinaryWriter(ms))
            {
                Header.Save(tmpwriter);
                tmpwriter.Write((ulong)elements.Length);
                for (int i = 0; i < elements.Length; i++)
                {
                    tmpwriter.Write(elementsOffsets[i]);
                }
                for (int i = 0; i < elements.Length; i++)
                {
                    tmpwriter.BaseStream.Position = (long)elementsOffsets[i];
                    elements[i].Save(tmpwriter);
                }
                tmpwriter.BaseStream.Position = (long)Header.Getsubelementviewp();
                sew.Save(tmpwriter);
                tmpwriter.BaseStream.Position = (long)Header.Getmetadata1p();
                tmpwriter.Write((ulong)md1dat.Length);
                for (int i = 0; i < md1dat.Length; i++)
                {
                    md1dat[i].Save(tmpwriter);
                }
                tmpwriter.BaseStream.Position = (long)Header.Getmetadata2p();
                md3.Save(tmpwriter);
                tmpwriter.BaseStream.Position = (long)Header.Getmetadata3p();
                tmpwriter.Write((ulong)RequiredGuis.Length);
                for (int i = 0; i < RequiredGuisOffsets.Length; i++)
                {
                    tmpwriter.Write(RequiredGuisOffsets[i]);
                }
                tmpwriter.BaseStream.Position = (long)Header.Getrequiredfilesp();
                tmpwriter.Write((ulong)RequiredFiles.Length);
                for (int i = 0; i < RequiredFiles.Length; i++)
                {
                    tmpwriter.Write(RequiredFilesOffsets[i]);
                }
                ofc.Save(tmpwriter);
            }
            using (MemoryStream ms = new MemoryStream(tmpbuf))
                ms.CopyTo(bw.BaseStream);
        }

        public static GUI Deserialize(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer srl = new XmlSerializer(typeof(GUI));
                GUI res = (GUI)srl.Deserialize(fs);
                res.RestoreAfterDeserialize();
                return res;
            }
        }

        public void RestoreAfterDeserialize()
        {
            ofc = new OffsetDataCollector();
            RestoreElements();
            RestoreSubElements();
            RestoreClipEntries();
            RestoreMetadata();
            GetCollectedData();
        }

        public void RestoreElements()
        {
            Header.Setelementsp(0x48 + (ulong)(elements.Length * 0x8));
            elementsOffsets = new ulong[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                elementsOffsets[i] = Header.Getelementsp() + (ulong)(0x30 * i);
            ulong st = Header.Getelementsp() + (ulong)(0x30 * elements.Length);
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].CollectData(ofc);
                elements[i].SetsubelementsListOffset(st);
                int sublen = elements[i].subelements.Length;
                ulong add = (ulong)((1 + sublen) * 8);
                st += add;
                elements[i].SetclipEntriesListOffset(st);
                sublen = elements[i].clipEntries.Length;
                add = (ulong)((1 + sublen) * 8);
                st += add;
            }
            Header.Setsubelementviewp(st);
        }

        public void RestoreSubElements()
        {
            List<SubElementView> subels = new List<SubElementView>();
            foreach (Element el in elements)
            {
                SubElementView[] elsubs = el.subelements;
                if (elsubs != null)
                {
                    subels.AddRange(elsubs);
                }
            }

            //sorting subelements
            for (int i = 0; i < subels.Count - 1; i++)
                for (int j = 0; j < subels.Count - i - 1; j++)
                {
                    if (subels[j].subIndex > subels[j + 1].subIndex)
                    {
                        SubElementView tmps = subels[j];
                        subels[j] = subels[j + 1];
                        subels[j + 1] = tmps;
                    }
                }

            //setting offsets at subelements
            sew.AdaptOffsets(Header.Getsubelementviewp());
            sew.RestoreAttributesOrder();
            sew.RestoreParamHashes();
            sew.CollectData(ofc);
            ulong[] offsets = new ulong[subels.Count];
            ulong curoofs = Header.Getsubelementviewp() + sew.MeasureSubElement();
            for (int i = 0; i < subels.Count; i++)
            {
                offsets[i] = curoofs;
                subels[i].AdaptOffsets(offsets[i]);
                subels[i].RestoreAttributesOrder();
                subels[i].RestoreParamHashes();
                curoofs += subels[i].MeasureSubElement();
                subels[i].CollectData(ofc);
            }

            //restoring references to subelements at elements
            foreach (Element el in elements)
            {
                el.RestoreSubElementsOffsetsList(offsets);
            }

            clipoffset = curoofs;
        }


        public void RestoreClipEntries()
        {
            List<ClipEntry> entrs = new List<ClipEntry>();
            foreach (Element el in elements)
            {
                ClipEntry[] elclips = el.clipEntries;
                if (elclips != null)
                {
                    entrs.AddRange(elclips);
                }
            }

            //sorting clip entries
            for (int i = 0; i < entrs.Count - 1; i++)
                for (int j = 0; j < entrs.Count - i - 1; j++)
                {
                    if (entrs[j].clipIndex > entrs[j + 1].clipIndex)
                    {
                        ClipEntry tmps = entrs[j];
                        entrs[j] = entrs[j + 1];
                        entrs[j + 1] = tmps;
                    }
                }

            //getting offsets for clips
            ulong[] offsets = new ulong[entrs.Count];
            ulong curoofs = clipoffset;
            for (int i = 0; i < entrs.Count; i++)
            {
                offsets[i] = curoofs;
                entrs[i].RestoreAfterDeserialize();
                entrs[i].CollectData(ofc);
                curoofs += entrs[i].MeasureClipEntry();
            }

            //restoring references to clipentries at elements
            foreach (Element el in elements)
            {
                el.RestoreClipsOffsetsList(offsets);
            }

            Header.Setmetadata1p(curoofs);
        }

        public void RestoreMetadata()
        {
            for (int i = 0; i < md1dat.Length; i++)
            {
                md1dat[i].RestoreAfterDeserialize();
                md1dat[i].CollectData(ofc);
            }
            Header.Setmetadata2p(Header.Getmetadata1p() + 8 + (ulong)(md1dat.Length * 48));
            md3.RestoreOffsets(Header.Getmetadata2p());
            md3.CollectData(ofc);
            ulong metadata2l = md3.MeasureMetadata();
            Header.Setmetadata3p(Header.Getmetadata2p() + metadata2l);
            for (int i = 0; i < RequiredGuis.Length; i++)
            {
                ofc.AddData(13, new REwString(RequiredGuis[i]));
            }
            Header.Setrequiredfilesp(Header.Getmetadata3p() + 8 + (ulong)(RequiredGuis.Length * 8));
            for (int i = 0; i < RequiredFiles.Length; i++)
            {
                ofc.AddData(13, new REwString(RequiredFiles[i]));
            }
            OffsetInfoOffset = Header.Getrequiredfilesp() + 8 + (ulong)(RequiredFiles.Length * 8);
        }

        public void GetCollectedData()
        {
            ofc.SetOffsets(OffsetInfoOffset);
            foreach (Element elem in elements)
                elem.GetCollectedData(ofc);
            foreach (Metadata1Element m1el in md1dat)
                m1el.GetCollectedData(ofc);
            md3.GetCollectedData(ofc);
            RequiredGuisOffsets = new ulong[RequiredGuis.Length];
            for (int i = 0; i < RequiredGuis.Length; i++)
            {
                RequiredGuisOffsets[i] = ofc.GetOffset(13, new REwString(RequiredGuis[i]));
            }
            RequiredFilesOffsets = new ulong[RequiredFiles.Length];
            for (int i = 0; i < RequiredFiles.Length; i++)
            {
                RequiredFilesOffsets[i] = ofc.GetOffset(13, new REwString(RequiredFiles[i]));
            }
            sew.GetCollectedData(ofc);
        }

        public void Read(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
                Read(br);
        }

        public void GUITestSave(BinaryWriter bw)
        {
            Element tel = elements[6];
            ClipEntry ten = tel.clipEntries[0];
            Clip tcl = ten.clip;
            tcl.Save(bw);
        }

        public void Read(BinaryReader br)
        {
            Header = new GUIHeader(br);
            br.BaseStream.Position = (long)Header.Getmetadata1p();
            long mt1count = br.ReadInt64();
            md1dat = new Metadata1Element[mt1count];
            for (int i = 0; i < mt1count; i++)
            {
                md1dat[i] = new Metadata1Element(br);
            }
            br.BaseStream.Position = (long)Header.Getmetadata2p();
            md3 = new MetaData3(br);
            br.BaseStream.Position = (long)Header.Getmetadata3p();
            long guicount = br.ReadInt64();
            RequiredGuis = new string[guicount];
            RequiredGuisOffsets = new ulong[guicount];
            for (int i = 0; i < guicount; i++)
            {
                RequiredGuisOffsets[i] = br.ReadUInt64();
                RequiredGuis[i] = new REwString(br, RequiredGuisOffsets[i]).str;
            }
            br.BaseStream.Position = (long)Header.Getrequiredfilesp();
            long requiredcount = br.ReadInt64();
            RequiredFiles = new string[requiredcount];
            RequiredFilesOffsets = new ulong[requiredcount];
            for (int i = 0; i < requiredcount; i++)
            {
                RequiredFilesOffsets[i] = br.ReadUInt64();
                RequiredFiles[i] = new REwString(br, RequiredFilesOffsets[i]).str;
            }
            ofc = new OffsetDataCollector(br);
            br.BaseStream.Position = (long)64;
            long elementscount = br.ReadInt64();
            elementsOffsets = new ulong[elementscount];
            for (long i = 0; i < elementscount; i++)
            {
                elementsOffsets[i] = br.ReadUInt64();
            }
            elements = new Element[elementscount];
            for (long i = 0; i < elementscount; i++)
            {
                br.BaseStream.Position = (long)elementsOffsets[i];
                elements[i] = new Element(br);
            }
            br.BaseStream.Position = (long)Header.Getsubelementviewp();
            sew = new SubElementView(br);
            SetSubviewsIndexes();
            SetClipsIndexes();
        }

        public void SetSubviewsIndexes()
        {
            //Getting Subelements
            List<SubElementView> subels = new List<SubElementView>();
            List<ulong> subeloffss = new List<ulong>();
            foreach (Element el in elements)
            {
                SubElementView[] elsubs = el.subelements;
                if (elsubs != null)
                {
                    subels.AddRange(elsubs);
                    subeloffss.AddRange(el.GetSubElementOffsets());
                }
            }
            //sorting subelements
            for (int i = 0; i < subels.Count - 1; i++)
                for (int j = 0; j < subels.Count - i - 1; j++)
                {
                    if (subeloffss[j] > subeloffss[j + 1])
                    {
                        ulong tmp = subeloffss[j];
                        subeloffss[j] = subeloffss[j + 1];
                        subeloffss[j + 1] = tmp;
                        SubElementView tmps = subels[j];
                        subels[j] = subels[j + 1];
                        subels[j + 1] = tmps;
                    }
                }
            for (int i = 0; i < subels.Count; i++)
            {
                subels[i].subIndex = i;
            }
        }

        public void SetClipsIndexes()
        {
            //Getting Subelements
            List<ClipEntry> clipEntries = new List<ClipEntry>();
            List<ulong> clipoffss = new List<ulong>();
            foreach (Element el in elements)
            {
                ClipEntry[] clips = el.clipEntries;
                if (clips != null)
                {
                    clipEntries.AddRange(clips);
                    clipoffss.AddRange(el.GetClipsOffsets());
                }
            }
            //sorting subelements
            for (int i = 0; i < clipEntries.Count - 1; i++)
                for (int j = 0; j < clipEntries.Count - i - 1; j++)
                {
                    if (clipoffss[j] > clipoffss[j + 1])
                    {
                        ulong tmp = clipoffss[j];
                        clipoffss[j] = clipoffss[j + 1];
                        clipoffss[j + 1] = tmp;
                        ClipEntry tmps = clipEntries[j];
                        clipEntries[j] = clipEntries[j + 1];
                        clipEntries[j + 1] = tmps;
                    }
                }
            for (int i = 0; i < clipEntries.Count; i++)
            {
                clipEntries[i].clipIndex = i;
            }
            for (int i = 0; i < clipEntries.Count; i++)
            {
                ulong addoffset = clipEntries[i].GetAdditionalOffset();
                if (addoffset == 0)
                    continue;
                if (!clipoffss.Contains(addoffset))
                    throw new Exception("ClipEntry Error: AdditionalOffset Not Found");
                clipEntries[i].addclipIndex = clipoffss.IndexOf(addoffset);
            }
        }

        public void Show()
        {
            Header.Show();
            Console.WriteLine("Metadata1:");
            for (int i = 0; i < md1dat.Length; i++)
            {
                md1dat[i].Show();
            }
            Console.WriteLine("Metadata2:");
            md3.Show();
            Console.WriteLine("RequiredGuis:");
            for (int i = 0; i < RequiredGuis.Length; i++)
            {
                Console.WriteLine(RequiredGuis[i]);
            }
            Console.WriteLine("RequiredFiles:");
            for (int i = 0; i < RequiredFiles.Length; i++)
            {
                Console.WriteLine(RequiredFiles[i]);
            }
            Console.WriteLine("Elements:");
            for (long i = 0; i < elements.Length; i++)
            {
                Console.WriteLine("Elements[" + i + "]:");
                elements[i].Show();
            }
            sew.Show();
        }
        public class GUIHeader
        {
            [XmlAttribute]
            public int version;
            byte[] magic = { 0x47, 0x55, 0x49, 0x52 }; //GUIR
            private ulong unknownpointer; //always 0x30
            private ulong metadata1p;
            private ulong metadata2p;
            private ulong metadata3p;
            private ulong requiredfilesp;
            private ulong elementsp;
            private ulong subelementviewp;

            public GUIHeader()
            {
                unknownpointer = 0x30;
            }

            public void Setunknownpointer(ulong v)
            {
                unknownpointer = v;
            }

            public ulong Getunknownpointer()
            {
                return unknownpointer;
            }

            public void Setmetadata1p(ulong v)
            {
                metadata1p = v;
            }

            public ulong Getmetadata1p()
            {
                return metadata1p;
            }

            public void Setmetadata2p(ulong v)
            {
                metadata2p = v;
            }

            public ulong Getmetadata2p()
            {
                return metadata2p;
            }


            public void Setmetadata3p(ulong v)
            {
                metadata3p = v;
            }

            public ulong Getmetadata3p()
            {
                return metadata3p;
            }

            public void Setrequiredfilesp(ulong v)
            {
                requiredfilesp = v;
            }

            public ulong Getrequiredfilesp()
            {
                return requiredfilesp;
            }

            public void Setelementsp(ulong v)
            {
                elementsp = v;
            }

            public ulong Getelementsp()
            {
                return elementsp;
            }

            public void Setsubelementviewp(ulong v)
            {
                subelementviewp = v;
            }

            public ulong Getsubelementviewp()
            {
                return subelementviewp;
            }

            public GUIHeader(BinaryReader br)
            {
                Read(br);
            }

            public void Read(BinaryReader br)
            {
                version = br.ReadInt32();
                byte[] m = br.ReadBytes(4);
                for (int i = 0; i < 4; i++)
                    if (m[i] != magic[i])
                    {
                        Console.WriteLine("wrong magic");
                    }
                unknownpointer = br.ReadUInt64();
                metadata1p = br.ReadUInt64();
                metadata2p = br.ReadUInt64();
                metadata3p = br.ReadUInt64();
                requiredfilesp = br.ReadUInt64();
                elementsp = br.ReadUInt64();
                subelementviewp = br.ReadUInt64();
            }

            public void Show()
            {
                string magicstr = System.Text.Encoding.Default.GetString(magic);
                Console.WriteLine(magicstr);
                Console.WriteLine("version: " + version);
                Console.WriteLine("unknownpointer: " + unknownpointer);
                Console.WriteLine("metadata1p: " + metadata1p);
                Console.WriteLine("metadata2p: " + metadata2p);
                Console.WriteLine("metadata3p: " + metadata3p);
                Console.WriteLine("metadata4p: " + requiredfilesp);
                Console.WriteLine("elementsp: " + elementsp);
                Console.WriteLine("subelementviewp: " + subelementviewp);
            }

            public void Save(BinaryWriter bw)
            {
                bw.Write(version);
                bw.Write(magic);
                bw.Write(unknownpointer);
                bw.Write(metadata1p);
                bw.Write(metadata2p);
                bw.Write(metadata3p);
                bw.Write(requiredfilesp);
                bw.Write(elementsp);
                bw.Write(subelementviewp);
            }
        }
    }
}
