using System.IO;

namespace EBNetBase
{

  public abstract class Message
  {
    public void ReadFrom(Stream stream)
    {
      using (var reader = new BinaryReader(stream))
      {
        ReadFrom(reader);
      }
    }

    public void WriteTo(Stream stream)
    {
      using (var writer = new BinaryWriter(stream))
      {
        WriteTo(writer);
      }
    }

    public byte[] GetBuffer()
    {
      using (var stream = new MemoryStream())
      {
        WriteTo(stream);
        return stream.ToArray();
      }
    }

    public abstract void ReadFrom(BinaryReader stream);
    public abstract void WriteTo(BinaryWriter stream);
  }

  public abstract class PacketFormatDescription
  {
    public abstract uint WrapperSize { get; }
    public uint PayloadSize { get; protected set; }
    public abstract void ParseWrapper(byte[] buffer);
    public abstract byte[] WrapMessage(Message msg);
  }
}
