using System;
using System.IO;

namespace SRMapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryReader b = new BinaryReader(File.Open(args[0], FileMode.Open));
            Level level = new Level();
            level.Read(b);
            b.Close();
            BinaryWriter w = new BinaryWriter(File.Open(args[1], FileMode.Create));
            level.Write(w);
            w.Close();
            Console.ReadLine();
        }
    }
}
