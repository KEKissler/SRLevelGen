using System;
using System.IO;

namespace SRMapGenerator
{
    //Useful paths for kyle's laptop
    //SR level file location
    // C:\Program Files (x86)\Steam\userdata\84319802\207140\remote
    //this program input output location
    //C:\Users\Owner\Documents\GitHub\SRLevelGen\SRLevelGen
    class Program
    {
        static void Main(string[] args)
        {
            BinaryReader b = new BinaryReader(File.Open(args[0], FileMode.Open));
            Level level = new Level();
            level.Read(b);
            //level.SetTilemaps(new Tilemap[] { new Tilemap(level.GetVersion(), "Collision" ,new int[1000,500]) });
            /*Tilemap[] test = level.GetTilemaps();
            foreach(Tilemap t in test)
            {
                Console.WriteLine(t.GetName());
            }
            level.SetTilemaps(test);*/
            foreach(Tilemap t in level.tilemaps)
            {
                if(t.GetName() == "Collision")
                {
                    //Tilemap toSet = new Tilemap(t.GetVersion(), t.name, /*GEN STUFF GOES HERE*/ new int[1000, 500]);

                    t.SetTilemap(/*GEN STUFF GOES HERE*/ new int[1000, 500]);
                }
            }
            b.Close();
            BinaryWriter w = new BinaryWriter(File.Open(args[1], FileMode.Create));
            level.Write(w);
            w.Close();
            Console.ReadLine();
        }
    }
}
