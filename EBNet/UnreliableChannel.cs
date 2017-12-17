using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EBNet
{
  public class UnreliableChannel : Channel
  {
    public IPEndPoint EndPoint { get; private set; }
    public int SessionID { get; set; }
    UdpClient mClient { get; set; } = new UdpClient();

    public UnreliableChannel(MessageTypeDictionary dict) : base(dict)
    {

    }

    internal UnreliableChannel(IPEndPoint localEp, MessageTypeDictionary dict) : base(dict)
    {
      mClient = new UdpClient(localEp);
    }

    internal void Setup(IPEndPoint ep)
    {
      EndPoint = ep;
      mClient.Connect(ep);
    }

    internal void RaiseDatagramReceived(UdpReceiveResult datagram)
    {
      using (var stream = new MemoryStream(datagram.Buffer))
      {
        var header = new UdpMessageHeader(stream);
        var msgType = TypeDictionary.GetTypeByID(header.TypeID);
        var message = Serializer.Deserialize(msgType, stream) as Message;
        if (EndPoint == null)
          Setup(datagram.RemoteEndPoint);
        RaiseMessageReceived(this, message, header);
      }
    }

    internal override MessageHeader CreateHeader(Message msg, int messageId)
    {
      return new UdpMessageHeader() { TypeID = TypeDictionary.GetTypeID(msg.GetType()), SessionId = SessionID, MessageID = messageId };
    }

    internal async override Task Write(MemoryStream source)
    {
      var buffer = source.ToArray();
      Console.WriteLine($"Write to: {EndPoint.Address}:{EndPoint.Port},{mClient.Available}");
      await mClient.SendAsync(buffer, buffer.Length);
    }
  }
}
