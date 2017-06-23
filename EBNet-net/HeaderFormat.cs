using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBNetBase;
using System.IO;

namespace EBNet_net
{
  public class HeaderFormat : EBNetBase.PacketFormatDescription
  {
    public byte StartByte { get; } = 111;
    public int MessageID { get; private set; }
    public Type MessageType { get; private set; }

    public override uint WrapperSize
    {
      get
      {
        return sizeof(byte) + sizeof(uint) * 3;
      }
    }

    public override void ParseWrapper(byte[] buffer)
    {
      using (var reader = new BinaryReader(new MemoryStream(buffer)))
      {
        var startByte = reader.ReadByte();
        if (startByte != StartByte)
          throw new Exception("Invalid message header");

        MessageID = reader.ReadInt32();
        var TypeID = reader.ReadUInt32();
        MessageType = MsgManager.GetTypeByID(TypeID);

        PayloadSize = reader.ReadUInt32();
      }
    }

    public override byte[] WrapMessage(Message msg)
    {
      using (var stream = new MemoryStream())
      using (var writer = new BinaryWriter(stream))
      {
        writer.Write(StartByte);

        var messageType = msg.GetType();
        writer.Write(MessageID);
        writer.Write(MsgManager.GetTypeID(messageType));

        long payloadPosition = stream.Position;
        writer.Write((UInt32)0);
        msg.WriteTo(writer);

        PayloadSize = (uint)stream.Position - (uint)payloadPosition - sizeof(uint);
        stream.Position = payloadPosition;
        writer.Write(PayloadSize);

        return stream.ToArray();
      }
    }

    public byte[] WrapMessage(Message msg, int id)
    {
      MessageID = id;
      return WrapMessage(msg);
    }

    static MessageManager MsgManager { get; } = new MessageManager();
  }
}
