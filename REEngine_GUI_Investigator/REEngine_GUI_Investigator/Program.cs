using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REEngine_GUI_Investigator
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args[0].Replace("\"", "");//args[0].Replace("\"", "");//Console.ReadLine().Replace("\"", "");
            if (!path.EndsWith(".xml"))
            {
                GUI gui = new GUI(path);
                gui.Serialize(path + ".xml");
            }
            else
            {
                GUI gui = GUI.Deserialize(path);
                using (FileStream fs = new FileStream(path.Remove(path.Length - 4), FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                    gui.Save(bw);
            }
            //ConvertAndComp(Console.ReadLine().Replace("\"", ""));
            //DeSerialize(Console.ReadLine().Replace("\"", ""));
        }

        static void ConvDir()
        {
            string[] files = Directory.GetFiles(Console.ReadLine());
            foreach (string s in files)
                if (s.EndsWith(".gui.620035") || s.EndsWith(".gui.620035.en"))
                    ConvertSerDeserAndComp(s);
                    //ConvertAndComp(s);
        }

        static void Serialize(string path)
        {
            GUI gui = new GUI(path);
            gui.Serialize(path + ".xml");
        }

        static void DeSerialize(string path)
        {
            GUI gui = GUI.Deserialize(path);
            using (FileStream fs = new FileStream(path + ".gui", FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
                gui.Save(bw);
        }

        static void Parse(string[] args)
        {
            if (args[0] == "extract") {
                if (args.Length != 1)
                    if (File.Exists(args[1]))
                    {
                        Serialize(args[1]);
                    }
            }
            else if (args[1] == "input")
            {
                if (args.Length != 1)
                    if (File.Exists(args[1]))
                    {
                        DeSerialize(args[1]);
                    }
            }
        }

        static void ConvertAndComp(string path)
        {
            GUI gui = new GUI(path);
            //GUI gui = GUI.Deserialize(path);
            //using (FileStream fs = new FileStream(path + ".cliptest", FileMode.Create, FileAccess.Write))
            //using (BinaryWriter bw = new BinaryWriter(fs))
            //gui.GUITestSave(bw);
            //gui.Show();
            gui.RestoreAfterDeserialize();
            using (FileStream fs = new FileStream(path + ".dessavetest", FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
                gui.Save(bw);
            Console.WriteLine(path);
            Comp(path, path + ".dessavetest");
        }



        static void ConvertSerDeserAndComp(string path)
        {
            GUI gui = new GUI(path);
            gui.Serialize(path + ".serial");
            GUI gui1 = GUI.Deserialize(path + ".serial");
            //using (FileStream fs = new FileStream(path + ".cliptest", FileMode.Create, FileAccess.Write))
            //using (BinaryWriter bw = new BinaryWriter(fs))
            //gui.GUITestSave(bw);
            //gui.Show();
            gui1.RestoreAfterDeserialize();
            using (FileStream fs = new FileStream(path + ".dessavetest", FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
                gui1.Save(bw);
            Console.WriteLine(path);
            Comp(path, path + ".dessavetest");
        }

        static void Comp(string file1, string file2)
        {
            byte[] b1 = File.ReadAllBytes(file1);
            byte[] b2 = File.ReadAllBytes(file2);
            bool eq = true;
            if (b1.Length != b2.Length)
            {
                eq = false;
                Console.WriteLine("The sizes are different.");
                if (b2.Length < b1.Length)
                {
                    byte[] tmp = b2;
                    b2 = b1;
                    b1 = tmp;
                }
            }
            for (long i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    eq = false;
                    Console.WriteLine("Difference in " + BTOS(i) + " byte.");
                }
            }
            if (eq)
                Console.WriteLine("Files are equivalent.");
        }


        static string BTOS(long x)
        {
            string res = "";
            while (x > 0)
            {
                char[] r = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
                res = r[x % 16] + res;
                x /= 16;
            }
            return res;
        }
    }
}
