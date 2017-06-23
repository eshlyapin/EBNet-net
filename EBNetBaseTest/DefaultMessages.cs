using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EBNetBase;
using System.IO;
using System.Net.Sockets;

namespace Test
{
  public class TestConnectionService : ConnectionService
  {
    public TestConnectionService(TcpListener socket)
      : base(socket)
    {
    }

    public override void OnFinish()
    {
    }

    public override void OnNewConnection(TcpClient socket)
    {
      var connection = new TestConnection(socket);
      connection.StartReceiving();
    }
  }

  public class TestConnection : Connection<TestPacketFormat>
  {
    public TestConnection(TcpClient socket)
      :base(socket)
    {
    }

    public override void OnDisconection(Exception ex)
    {
    }

    public override void OnPayloadReady(TestPacketFormat format, byte[] buffer)
    {
      Message msg = null;

      if (format.MessageID == 1)
        msg= new TestMessage1();
      else if (format.MessageID == 2)
        msg = new TestMessage2();
      else if (format.MessageID == 3)
        msg = new TestMessage3();

      using (var stream = new MemoryStream(buffer))
      {
        msg.ReadFrom(stream);
      }
    }
  }

  public class TestMessage1 : Message
  {
    public int ClientID { get; set; }
    public byte[] SomeData { get; set; }

    public TestMessage1()
    {
    }

    public TestMessage1(int clientId, byte[] data)
    {
      ClientID = clientId;
      SomeData = data;
    }

    public override void ReadFrom(BinaryReader stream)
    {
      ClientID = stream.ReadInt32();

      var datasize = stream.ReadInt32();
      SomeData = stream.ReadBytes(datasize);
    }

    public override void WriteTo(BinaryWriter stream)
    {
      stream.Write(ClientID);
      stream.Write(SomeData.Length);
      stream.Write(SomeData);
    }
  }

  public class TestMessage2 : Message
  {
    public int ClientID2 { get; set; }
    public byte[] SomeData { get; set; }

    public TestMessage2()
    {
    }

    public TestMessage2(int clientId, byte[] data)
    {
      ClientID2 = clientId;
      SomeData = data;
    }

    public override void ReadFrom(BinaryReader stream)
    {
      ClientID2 = stream.ReadInt32();

      var datasize = stream.ReadInt32();
      SomeData = stream.ReadBytes(datasize);
    }

    public override void WriteTo(BinaryWriter stream)
    {
      stream.Write(ClientID2);
      stream.Write(SomeData.Length);
      stream.Write(SomeData);
    }
  }

  public class TestMessage3 : Message
  {
    public int ClientID3 { get; set; }
    public byte[] SomeData { get; set; }

    public TestMessage3()
    {
    }

    public TestMessage3(int clientId, byte[] data)
    {
      ClientID3 = clientId;
      SomeData = data;
    }

    public override void ReadFrom(BinaryReader stream)
    {
      ClientID3 = stream.ReadInt32();

      var datasize = stream.ReadInt32();
      SomeData = stream.ReadBytes(datasize);
    }

    public override void WriteTo(BinaryWriter stream)
    {
      stream.Write(ClientID3);
      stream.Write(SomeData.Length);
      stream.Write(SomeData);
    }
  }

  public class TestPacketFormat : PacketFormatDescription
  {
    public static TestPacketFormat Instance { get; } = new TestPacketFormat();

    public override uint WrapperSize { get; } = sizeof(uint) * 2 + sizeof(byte);
    public uint MessageID { get; private set; }
    public uint Version { get; } = 1;

    public override void ParseWrapper(byte[] buffer)
    {
      using (var reader = new BinaryReader(new MemoryStream(buffer)))
      {
        var version = reader.ReadByte();
        MessageID = reader.ReadUInt32();
        PayloadSize = reader.ReadUInt32();
      }
    }

    public override byte[] WrapMessage(Message msg)
    {
      using (var stream = new MemoryStream())
      using (var writer = new BinaryWriter(stream))
      {
        writer.Write((byte)Version);

        if (msg is TestMessage1)
          writer.Write((UInt32)1);
        if (msg is TestMessage2)
          writer.Write((UInt32)2);
        if (msg is TestMessage3)
          writer.Write((UInt32)3);

        var buffer = msg.GetBuffer();
        writer.Write((UInt32)buffer.Length);
        writer.Write(buffer);

        return stream.ToArray();
      }
    }
  }
}
