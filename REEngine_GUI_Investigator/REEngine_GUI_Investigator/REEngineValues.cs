using System.IO;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace REEngine_GUI_Investigator
{
    [XmlInclude(typeof(ColorBytes))]
    [XmlInclude(typeof(ColorFloat))]
    [XmlInclude(typeof(REBool))]
    [XmlInclude(typeof(REDouble))]
    [XmlInclude(typeof(REID))]
    [XmlInclude(typeof(RELong))]
    [XmlInclude(typeof(REString))]
    [XmlInclude(typeof(REwString))]
    [XmlInclude(typeof(Vector2))]
    [XmlInclude(typeof(Vector2I))]
    [XmlInclude(typeof(Vector3))]
    [XmlInclude(typeof(Vector4))]
    public abstract class REEngineValues
    {
        protected string typeString;
        protected string valstring;

        public REEngineValues()
        {

        }

        public REEngineValues(BinaryReader br)
        {
            Read(br);
            SetStrings();
        }

        public REEngineValues(BinaryReader br, ulong pointer)
        {
            Read(br, pointer);
            SetStrings();
        }

        public string GetTypeString()
        {
            return typeString;
        }

        public string GetValString()
        {
            return valstring;
        }

        public abstract void Read(BinaryReader br);

        public void Read(BinaryReader br, ulong pointer)
        {
            long oldpos = br.BaseStream.Position;
            br.BaseStream.Position = (long)pointer;
            Read(br);
            br.BaseStream.Position = oldpos;
        }

        public void Show()
        {
            Console.WriteLine(typeString + ": " + valstring);
        }

        public abstract void SetStrings();

        public abstract void Save(BinaryWriter bw);

        public void Save(BinaryWriter bw, ulong pointer)
        {
            long oldpos = bw.BaseStream.Position;
            bw.BaseStream.Position = (long)pointer;
            Save(bw);
            bw.BaseStream.Position = oldpos;
        }

        public static bool GetIsOffset(int type)
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
                    return false;
                case 13:
                case 14:
                case 15:
                case 16:
                case 21:
                case 22:
                case 23:
                case 24:
                case 26:
                case 27:
                case 28:
                case 31:
                case 32:
                case 34:
                case 35:
                case 43:
                    return true;
                default:
                    throw new Exception("Unknown attribute type: " + type);
            }
        }

        public static REEngineValues GetValue(int type, BinaryReader br)
        {
            switch (type)
            {
                case 1:
                    return new REBool(br);
                case 3:
                case 5:
                case 6:
                case 7:
                case 8:
                case 4:
                case 9:
                    return new RELong(br);
                case 10:
                case 11:
                    return new REDouble(br);
                case 15:
                case 34:
                    return new REID(br);
                case 14:
                    return new REString(br);
                case 13:
                case 32:
                    return new REwString(br);
                case 21:
                case 26:
                case 31:
                    return new Vector2(br);
                case 23:
                case 28:
                case 43:
                    return new Vector4(br);
                case 22:
                case 27:
                    return new Vector3(br);
                case 24:
                    return new ColorBytes(br);
                //return new ColorFloat(br);
                case 16:
                case 35:
                    return new Vector2I(br);
                default:
                    throw new Exception("Unknown attribute type: " + type);
            }
        }

        public static REEngineValues GetValue(int type, BinaryReader br, ulong pos)
        {
            ulong oldpos = (ulong)br.BaseStream.Position;
            br.BaseStream.Position = (long)pos;
            REEngineValues res = GetValue(type, br);
            br.BaseStream.Position = (long)oldpos;
            return res;
        }

        public abstract ulong GetDataLength();

        public static bool IsInvestigated(int type)
        {
            switch (type)
            {
                case 1:
                case 3:
                case 5:
                case 6:
                case 7:
                case 8:
                case 4:
                case 9:
                case 10:
                case 15:
                case 16:
                case 11:
                case 34:
                case 14:
                case 43:
                case 13:
                case 32:
                case 26:
                case 27:
                case 23:
                case 21:
                case 31:
                case 24:
                case 22:
                case 28:
                case 35:
                    return true;
                default:
                    return false;
            }
        }

        public static void DeleteDuplicates(int type, List<REEngineValues> vals)
        {
            switch (type)
            {
                case 34:
                case 15:
                    REID.DeleteDuplicates(vals);
                    break;
                case 14:
                    REString.DeleteDuplicates(vals);
                    break;
                case 13:
                case 32:
                    REwString.DeleteDuplicates(vals);
                    break;
                case 21:
                case 31:
                case 26:
                    Vector2.DeleteDuplicates(vals);
                    break;
                case 23:
                case 28:
                case 43:
                    Vector4.DeleteDuplicates(vals);
                    break;
                case 22:
                case 27:
                    Vector3.DeleteDuplicates(vals);
                    break;
                case 24:
                    ColorBytes.DeleteDuplicates(vals);
                    break;
                /*case 28:
                    ColorFloat.DeleteDuplicates(vals);
                    break;*/
                case 16:
                case 35:
                    Vector2I.DeleteDuplicates(vals);
                    break;
                default:
                    throw new Exception("Unknown attribute type: " + type);
            }
        }
        public static bool Compare(int type, REEngineValues val1, REEngineValues val2)
        {
            switch (type)
            {
                case 1:
                    return ((REBool)val1).b == ((REBool)val2).b;
                case 3:
                case 5:
                case 6:
                case 7:
                case 8:
                case 4:
                case 9:
                    return ((RELong)val1).l == ((RELong)val2).l;
                case 10:
                case 11:
                    return ((REDouble)val1).d == ((REDouble)val2).d;
                case 15:
                case 34:
                    return REID.Compare((REID)val1, (REID)val2);
                case 14:
                    return REString.Compare((REString)val1, (REString)val2);
                case 13:
                case 32:
                    return REwString.Compare((REwString)val1, (REwString)val2);
                case 21:
                case 26:
                case 31:
                    return Vector2.Compare((Vector2)val1, (Vector2)val2);
                case 23:
                case 28:
                    return Vector4.Compare((Vector4)val1, (Vector4)val2);
                case 22:
                case 27:
                    return Vector3.Compare((Vector3)val1, (Vector3)val2);
                case 24:
                    return ColorBytes.Compare((ColorBytes)val1, (ColorBytes)val2);
                //case 28:
                //  return ColorFloat.Compare((ColorFloat)val1, (ColorFloat)val2);
                case 16:
                case 35:
                    return Vector2I.Compare((Vector2I)val1, (Vector2I)val2);
                default:
                    throw new Exception("Unknown attribute type: " + type);
            }
        }

        public static int GetIndexOf(int type, REEngineValues val, List<REEngineValues> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Compare(type, val, list[i]))
                    return i;
            }
            return -1;
        }
    }
}
