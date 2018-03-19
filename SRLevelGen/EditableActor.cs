using System;
using System.IO;
using System.Numerics;

public class EditableActor
{
    public int version;
    public Vector2 position;
    public Vector2 size;
    public EditableProperty[] properties;
    public String actorType;

    public EditableActor()
    {

    }

    public EditableActor(int ver, Vector2 pos, Vector2 siz, EditableProperty[] props, String aType)
    {
        version = ver;
        position = pos;
        size = siz;
        properties = props;
        actorType = aType;
    }

    public virtual void Read(BinaryReader reader)
    {
        position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        size = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        actorType = reader.ReadString();
        properties = new EditableProperty[reader.ReadInt32()];
        for (int i = 0; i < properties.Length; i++)
        {
            properties[i] = new EditableProperty();
            properties[i].version = version;
            properties[i].Read(reader);
        }
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(position.X);
        writer.Write(position.Y);
        writer.Write(size.X);
        writer.Write(size.Y);
        writer.Write(actorType);
        writer.Write(properties.Length);
        foreach (EditableProperty property in properties)
            property.Write(writer);
    }

    public override string ToString()
    {
        String result = String.Empty;
        result += "Version: " + version + "\n";
        result += "Position: " + position + "\n";
        result += "Size: " + size + "\n";
        result += "Properties Length: " + properties.Length + "\n";
        result += "Properties: [";
        foreach (EditableProperty property in properties)
            result += property + ",";
        result += "]\n";
        result += "Actor Type: " + actorType + "\n";
        return result;
    }

    public void SetVersion(int ver)
    {
        this.version = ver;
    }

    public void SetPosition(Vector2 pos)
    {
        this.position = pos;
    }

    public void SetSize(Vector2 siz)
    {
        this.size = siz;
    }

    public void SetProperties(EditableProperty[] props)
    {
        this.properties = props;
    }

    public void SetActorType(String actType)
    {
        this.actorType = actType;
    }

    public int GetVersion()
    {
        return version;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public Vector2 GetSize()
    {
        return size;
    }

    public EditableProperty[] GetEditableProperties()
    {
        return properties;
    }

    public String GetActorType()
    {
        return actorType;
    }
}
