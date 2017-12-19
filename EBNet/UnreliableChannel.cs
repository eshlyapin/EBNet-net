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
    public IPEndPoint RemoteEndPoint { get; private set; }
    public int SessionID { get; private set; }
    UdpClient mClient { get; } = new UdpClient();

    public UnreliableChannel(MessageTypeDictionary dict) : base(dict)
    {
    }

    internal UnreliableChannel(UdpClient client, int sessionId, MessageTypeDictionary dict) : base(dict)
    {
      SessionID = sessionId;
      mClient = client;
    }

    internal void Setup(IPEndPoint remoteEndPoint, int sessionId)
    {
      SessionID = sessionId;
      RemoteEndPoint = remoteEndPoint;
      mClient.Connect(RemoteEndPoint);
      StartReading();
    }
    
    internal async Task StartReading()
    {
      try
      {
        while (true)//TODO:
        {
          var received = await mClient.ReceiveAsync().ConfigureAwait(false);
          RaiseDatagramReceived(received);
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    internal void RaiseDatagramReceived(UdpReceiveResult dgram)
    {
      //TODO: endpoint sets to valid only if something was received
      if (RemoteEndPoint == null)
        RemoteEndPoint = dgram.RemoteEndPoint;
      using (var stream = new MemoryStream(dgram.Buffer))
      {
        var header = new UdpMessageHeader(stream);
        var message = Serializer.Deserialize(TypeDictionary.GetTypeByID(header.TypeID), stream) as Message;
        RaiseMessageReceived(this, message, header);
      }
    }

    internal override MessageHeader CreateHeader(Message msg, int messageId)
    {
      return new UdpMessageHeader() { MessageID = messageId, SessionId = this.SessionID, TypeID = TypeDictionary.GetTypeID(msg.GetType()) };
    }

    internal override Task Write(MemoryStream source)
    {
      var buffer = source.ToArray();
      if (!mClient.Client.Connected)
        return mClient.SendAsync(buffer, buffer.Length, RemoteEndPoint);
      else
        return mClient.SendAsync(buffer, buffer.Length);
    }

    public override void Close()
    {
      if (mClient.Client.Connected)
        mClient.Close();
      else
        OnClose?.Invoke(this);
    }

    //TODO:
    internal Action<UnreliableChannel> OnClose { get; set; }
  }
}
