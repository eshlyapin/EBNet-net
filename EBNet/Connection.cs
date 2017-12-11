using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace EBNet
{
  public class Connection
  {
    TcpClient mTcpClient;
    int mSessionId = 0;
    UdpClient mUdpClient;

    MessageTypeDictionary mMsgDict;
    public Router Router { get; private set; }

    ConcurrentDictionary<int, TaskCompletionSource<Message>> mRequested = new ConcurrentDictionary<int, TaskCompletionSource<Message>>();
    int mNextMessageId = 0;

    public Connection(TcpClient tcpClient, Router router, MessageTypeDictionary typeDictionary)
    {
      mTcpClient = tcpClient;
      Router = router;
      mMsgDict = typeDictionary;

      Router.AddHandler<SetupSession>(HandleSessionSetup);
      Router.AddHandler<BadResponse>(HandleBadResponse);
      StartReading();
    }

    public void SetupUnreliable(int sessionId, IPEndPoint endpoint)
    {
      mSessionId = sessionId;
      mUdpClient = new UdpClient();
      mUdpClient.Connect(endpoint);
    }

    async Task StartReading()
    {
      while (true)
      {
        var buffer = new byte[TcpMessageHeader.HeaderLength];
        int read = 0;
        while (read < buffer.Length)
          read += await mTcpClient.GetStream().ReadAsync(buffer, read, buffer.Length - read);

        var header = new TcpMessageHeader(buffer);

        buffer = new byte[header.Length];
        read = 0;
        while (read < buffer.Length)
          read += await mTcpClient.GetStream().ReadAsync(buffer, read, buffer.Length - read);
        var response = HandleMessage(header.TypeID, header.MessageID, new MemoryStream(buffer));

        if (response != null)
          await SendReliable(response, header.MessageID);
      }
    }

    public Message HandleMessage(int typeId, int msgId, MemoryStream stream)
    {
      if (!mRequested.ContainsKey(msgId))
      {
        var msgType = mMsgDict.GetTypeByID(typeId);
        var message = Serializer.Deserialize(msgType, stream) as Message;
        return Router.Handle(this, message);
      }
      else
      {
        var msgType = mMsgDict.GetTypeByID(typeId);
        mRequested[msgId]?.SetResult( Serializer.Deserialize(msgType, stream) as Message );
        return null;
      }
    }

    public async Task<TResp> SendReliable<TResp>(Message msg) where TResp : Message
    {
      mNextMessageId = Interlocked.Increment(ref mNextMessageId);
      var tcs = new TaskCompletionSource<Message>();
      while (!mRequested.TryAdd(mNextMessageId, tcs));
      await SendReliable(msg, mNextMessageId);
      var message = await tcs.Task as TResp;
      while (!mRequested.TryRemove(mNextMessageId, out tcs)) ;
      return message;
    }

    public async Task<TResp> SendUnreliable<TResp>(Message msg) where TResp : Message
    {
      mNextMessageId = Interlocked.Increment(ref mNextMessageId);
      var tcs = new TaskCompletionSource<Message>();
      while (!mRequested.TryAdd(mNextMessageId, tcs)) ;
      await SendUnreliable(msg, mNextMessageId);
      var message = await tcs.Task as TResp;
      while (!mRequested.TryRemove(mNextMessageId, out tcs)) ;
      return message;
    }

    public async Task SendReliable(Message msg, int msgId = 0)
    {
      var msgStream = new MemoryStream();
      msg.WriteTo(msgStream);

      var msgbuffer = msgStream.ToArray();
      var header = new TcpMessageHeader() { TypeID = mMsgDict.GetTypeID(msg.GetType()), Length = msgbuffer.Length, MessageID = msgId };

      msgStream = new MemoryStream();
      header.WriteTo(msgStream);
      var headerbuffer = msgStream.ToArray();

      msgStream = new MemoryStream();
      msgStream.Write(headerbuffer, 0, headerbuffer.Length);
      msgStream.Write(msgbuffer, 0, msgbuffer.Length);

      var result = msgStream.ToArray();
      await mTcpClient.GetStream().WriteAsync(result, 0, result.Length);
    }

    public async Task SendUnreliable(Message msg, int msgId = 0)
    {
      while (mUdpClient == null) ;

      var header = new UdpMessageHeader() { TypeID = mMsgDict.GetTypeID(msg.GetType()), SessionId = mSessionId, MessageID = msgId };

      var msgStream = new MemoryStream();
      header.WriteTo(msgStream);
      msg.WriteTo(msgStream);

      var result = msgStream.ToArray();
      if (mUdpClient != null)
        await mUdpClient.SendAsync(result, result.Length);
    }

    static void HandleSessionSetup(Connection connection, SetupSession session)
    {
      connection.SetupUnreliable(session.SessionId, new IPEndPoint(IPAddress.Parse(session.Address), session.port));
    }

    static void HandleBadResponse(Connection connection, BadResponse session)
    {
      Console.WriteLine("Bad response received");
    }
  }
}
