using System;
using System.IO;

public class EditableProperty
{
    public int version;
    public String name;
    public String value;

    public EditableProperty()
    {

    }

    public EditableProperty(int ver, String nam, String val)
    {
        version = ver;
        name = nam;
        value = val;
    }

    public virtual void Read(BinaryReader reader)
    {
        name = reader.ReadString();
        value = reader.ReadString();
    }

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(name);
        writer.Write(value);
    }

    public override string ToString()
    {
        String result = String.Empty;
        result += "Version: " + version + "\n";
        result += "Name: " + name + "\n";
        result += "Value: " + value + "\n";
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

    public void SetValue(String val)
    {
        this.value = val;
    }

    public int GetVersion()
    {
        return version;
    }

    public String GetName()
    {
        return name;
    }

    public String GetValue()
    {
        return value;
    }
}
