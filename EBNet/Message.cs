using ProtoBuf;
using System;
using System.IO;

namespace EBNet
{
  public class MessageTypeID : Attribute
  {
    public int TypeID { get; private set; }
    public MessageTypeID(int id)
    {
      TypeID = id;
    }
  }

  public abstract class Message
  {
    public abstract void ReadFrom(MemoryStream stream);
    public abstract void WriteTo(MemoryStream stream);
  }

  public class Message<T> : Message where T : Message<T>
  {
    public override void ReadFrom(MemoryStream stream)
    {
      Serializer.Merge<T>(stream, (T)this);
    }

    public override void WriteTo(MemoryStream stream)
    {
      Serializer.Serialize<T>(stream, (T)this);
    }
  }
}
