
using System.IO;

namespace EBNet
{
  class TcpMessageHeader
  {
    public int Length { get; set; }
    public int TypeID { get; set; }
    public int MessageID { get; set; }

    public TcpMessageHeader() { }
    public TcpMessageHeader(byte[] data)
    {
      using (var stream = new MemoryStream(data))
        ReadFrom(stream);
    }

    public TcpMessageHeader(MemoryStream stream)
    {
      ReadFrom(stream);
    }

    public void ReadFrom(MemoryStream stream)
    {
      using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
      {
        Length = reader.ReadInt32();
        TypeID = reader.ReadInt32();
        MessageID = reader.ReadInt32();
      }
    }

    public void WriteTo(MemoryStream stream)
    {
      using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
      {
        writer.Write(Length);
        writer.Write(TypeID);
        writer.Write(MessageID);
      }
    }

    public static int HeaderLength { get { return sizeof(int) + sizeof(int) + sizeof(int); } }
  }

  class UdpMessageHeader
  {
    public int SessionId { get; set; }
    public int TypeID { get; set; }
    public int MessageID { get; set; }

    public UdpMessageHeader() { }
    public UdpMessageHeader(byte[] data)
    {
      using (var stream = new MemoryStream(data))
        ReadFrom(stream);
    }

    public UdpMessageHeader(MemoryStream stream)
    {
      ReadFrom(stream);
    }

    public void ReadFrom(MemoryStream stream)
    {
      using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
      {
        SessionId = reader.ReadInt32();
        TypeID = reader.ReadInt32();
        MessageID = reader.ReadInt32();
      }
    }

    public void WriteTo(MemoryStream stream)
    {
      using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
      {
        writer.Write(SessionId);
        writer.Write(TypeID);
        writer.Write(MessageID);
      }
    }
  }
}
