using System;
using System.IO;

public class Level
{
    public const int VERSION = 6;

    public int version;
    public EditableActor[] actors;
    public Tilemap[] tilemaps;
    public String theme;
    public bool isSingleplayer;
    public int bombTimer = 60;
    public String author;
    public String name;
    public String description;
    public long publishTo;

    public Level()
    {

    }

    public Level(int ver, EditableActor[] acts, Tilemap[] tmaps, String them, bool isSp, int bombTime, String auth, String nam, String desc, long publish)
    {
        version = ver;
        actors = acts;
        tilemaps = tmaps;
        theme = them;
        isSingleplayer = isSp;
        bombTimer = bombTime;
        author = auth;
        name = nam;
        description = desc;
        publishTo = publish;
    }

    public virtual void Read(BinaryReader reader)
    {
        version = reader.ReadInt32();

        int nActors = reader.ReadInt32();
        actors = new EditableActor[nActors];

        for (int i = 0; i < actors.Length; i++)
        {
            actors[i] = new EditableActor();
            actors[i].version = version;
            actors[i].Read(reader);
        }

        int nTilemaps = reader.ReadInt32();
        tilemaps = new Tilemap[nTilemaps];

        for (int i = 0; i < tilemaps.Length; i++)
        {
            tilemaps[i] = new Tilemap();
            tilemaps[i].version = version;
            tilemaps[i].Read(reader);
        }
        theme = reader.ReadString();
        if (version >= 2)
            isSingleplayer = reader.ReadBoolean();
        if (isSingleplayer && version >= 3)
            bombTimer = reader.ReadInt32();
        if (version >= 4)
            author = reader.ReadString();
        if (version >= 5)
        {
            name = reader.ReadString();
            description = reader.ReadString();
        }
        if (version >= 6)
            publishTo = reader.ReadInt64();
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(VERSION);
        writer.Write(actors.Length);
        foreach (EditableActor actor in actors)
            actor.Write(writer);
        writer.Write(tilemaps.Length);
        foreach (Tilemap tilemap in tilemaps)
            tilemap.Write(writer);
        writer.Write(theme);
        if (VERSION >= 2)
        {
            writer.Write(isSingleplayer);
        }
        if (isSingleplayer && VERSION >= 3)
            writer.Write(bombTimer);
        if (VERSION >= 4)
        {
            if (author == null)
                author = String.Empty;

            writer.Write(author);
        }

        if (VERSION >= 5)
        {
            if (description == null)
                description = String.Empty;

            if (name == null)
                name = String.Empty;

            writer.Write(name);
            writer.Write(description);
        }

        if (VERSION >= 6)
            writer.Write(publishTo);
    }

    public override string ToString()
    {
        String result = String.Empty;
        result += "Version: " + version + "\n";
        result += "Actors Length: " + actors.Length + "\n";
        result += "Actors: [";
        result += "Tilemaps: [";
        foreach (EditableActor actor in actors)
            result += actor + ",";
        result += "]\n";
        result += "Tilemaps Length: " + tilemaps.Length + "\n";
        result += "Tilemaps: [";
        foreach (Tilemap tilemap in tilemaps)
            result += tilemap + ",";
        result += "]\n";
        result += "Theme: " + theme + "\n";
        result += "isSinglePlayer: " + isSingleplayer + "\n";
        result += isSingleplayer ? ("Bomb Timer: " + bombTimer + "\n") : "";
        result += "Author: " + author + "\n";
        result += "Name: " + name + "\n";
        result += "Description: " + description + "\n";
        result += "Publish ID: " + publishTo + "\n";
        return result;
    }

    public void SetVersion(int ver)
    {
        this.version = ver;
    }

    public void SetActors(EditableActor[] acts)
    {
        this.actors = acts;
    }

    public void SetTilemaps(Tilemap[] tmaps)
    {
        this.tilemaps = tmaps;
    }

    public void SetTheme(String them)
    {
        this.theme = them;
    }

    public void SetSingleplayer(bool isSp)
    {
        this.isSingleplayer = isSp;
    }

    public void SetBombTimer(int bombTime)
    {
        this.bombTimer = bombTime;
    }

    public void SetAuthor(String auth)
    {
        this.author = auth;
    }

    public void SetName(String nam)
    {
        this.name = nam;
    }

    public void SetDescription(String desc)
    {
        this.description = desc;
    }

    public void SetPublishTo(long publish)
    {
        this.publishTo = publish;
    }

    public int GetVersion()
    {
        return version;
    }

    public EditableActor[] GetActors()
    {
        return actors;
    }

    public Tilemap[] GetTilemaps()
    {
        return tilemaps;
    }

    public String GetTheme()
    {
        return theme;
    }

    public bool GetSingleplayer()
    {
        return isSingleplayer;
    }

    public int GetBombTimer()
    {
        return bombTimer;
    }

    public String GetAuthor()
    {
        return author;
    }

    public String GetName()
    {
        return name;
    }

    public String GetDescription()
    {
        return description;
    }

    public long GetPublishTo()
    {
        return publishTo;
    }

}
