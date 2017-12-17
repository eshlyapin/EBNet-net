
using System;
using System.IO;

namespace EBNet
{
  public abstract class MessageHeader : Message
  {
    public int TypeID { get; set; }
    public int MessageID { get; set; }

    public MessageHeader() { }

    public MessageHeader(MemoryStream stream)
    {
      ReadFrom(stream);
    }

    public abstract MemoryStream Wrap(Message msg);
  }

  public class TcpMessageHeader : MessageHeader
  {
    public int Length { get; private set; }

    public TcpMessageHeader() { }
    public TcpMessageHeader(MemoryStream stream) : base(stream) { }

    public override void ReadFrom(MemoryStream stream)
    {
      using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
      {
        Length = reader.ReadInt32();
        TypeID = reader.ReadInt32();
        MessageID = reader.ReadInt32();
      }
    }

    public override void WriteTo(MemoryStream stream)
    {
      using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
      {
        writer.Write(Length);
        writer.Write(TypeID);
        writer.Write(MessageID);
      }
    }

    public override MemoryStream Wrap(Message msg)
    {
      using (var stream = new MemoryStream())
      {
        stream.Position = HeaderLength;
        msg.WriteTo(stream);

        Length = (int)stream.Position - HeaderLength;

        stream.Position = 0;
        WriteTo(stream);
        var buffer = stream.ToArray();

        return new MemoryStream(buffer);
      }
    }

    public static int HeaderLength { get { return sizeof(int) + sizeof(int) + sizeof(int); } }
  }

  class UdpMessageHeader : MessageHeader
  {
    public int SessionId { get; set; }

    public UdpMessageHeader() { }
    public UdpMessageHeader(MemoryStream stream) : base(stream) { }

    public override void ReadFrom(MemoryStream stream)
    {
      using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
      {
        SessionId = reader.ReadInt32();
        TypeID = reader.ReadInt32();
        MessageID = reader.ReadInt32();
      }
    }

    public override void WriteTo(MemoryStream stream)
    {
      using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
      {
        writer.Write(SessionId);
        writer.Write(TypeID);
        writer.Write(MessageID);
      }
    }

    public override MemoryStream Wrap(Message msg)
    {
      using (var stream = new MemoryStream())
      {
        WriteTo(stream);
        msg.WriteTo(stream);
        return stream;
      }
    }
  }
}
