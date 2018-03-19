using System;
using System.IO;

public class Tilemap
{
    public int version;
    public String name;
    public int[,] tilemap;

    public Tilemap()
    {

    }

    public Tilemap(int ver, String nam, int[,] tiles)
    {
        version = ver;
        name = nam;
        tilemap = tiles;
    }

    public virtual void Read(BinaryReader reader)
    {
        name = reader.ReadString();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        tilemap = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                tilemap[x, y] = reader.ReadInt32();
        }
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(name);
        int width = tilemap.GetLength(0);
        int height = tilemap.GetLength(1);
        writer.Write(width);
        writer.Write(height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                writer.Write(tilemap[x, y]);
        }
    }

    public override string ToString()
    {
        String result = String.Empty;
        result += "Version: " + version + "\n";
        result += "Name: " + name + "\n";
        result += "Tilemap: [\n";
        //for (int i = 0; i < tilemap.GetLength(0); i++)
        //{
        //    for (int j = 0; j < tilemap.GetLength(1); j++)
        //    {
        //        result += tilemap[i, j] + ",";
        //    }
        //    result += "\n";
        //}
        result += "]\n";
        return result;
    }

    public void SetVersion(int ver)
    {
        this.version = ver;
    }

    public void SetName(String nam)
    {
        this.name = nam;
    }

    public void SetTilemap(int[,] tmap)
    {
        this.tilemap = tmap;
    }

    public int GetVersion()
    {
        return version;
    }

    public String GetName()
    {
        return name;
    }

    public int[,] GetTilemap()
    {
        return tilemap;
    }
}
