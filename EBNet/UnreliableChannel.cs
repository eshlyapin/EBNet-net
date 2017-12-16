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
    UdpClient mClient { get; set; }

    public UnreliableChannel(IPEndPoint ep, MessageTypeDictionary dict) : base(dict)
    {
      EndPoint = ep;
      mClient.Connect(EndPoint);
    }

    public UnreliableChannel(MessageTypeDictionary dict) : base(dict)
    {

    }

    internal void RaiseDatagramReceived(UdpReceiveResult datagram)
    {
      using (var stream = new MemoryStream(datagram.Buffer))
      {
        var header = new UdpMessageHeader(stream);
        var msgType = TypeDictionary.GetTypeByID(header.TypeID);
        var message = Serializer.Deserialize(msgType, stream) as Message;
        if (EndPoint == null)
          EndPoint = datagram.RemoteEndPoint;
        RaiseMessageReceived(this, message, header);
      }
    }

    internal override MessageHeader CreateHeader(Message msg)
    {
      return new UdpMessageHeader() { TypeID = TypeDictionary.GetTypeID(msg.GetType()), SessionId = SessionID, MessageID = DefaultMessageId };
    }

    internal override Task Write(MemoryStream source)
    {
      var buffer = source.ToArray();
      return mClient.SendAsync(buffer, buffer.Length);
    }
  }
}
