using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace EBNet
{
  public class Connection
  {
    public int SessionID { get; private set; }

    public ReliableChannel Reliable { get; private set; }
    public UnreliableChannel Unreliable { get; private set; }

    internal Connection(ReliableChannel rel, UnreliableChannel unr, int sessionId)
    {
      SessionID = sessionId;
      Reliable = rel;
      Unreliable = unr;
      Reliable.OnMessageReceived += Handler;
      Unreliable.OnMessageReceived += Handler;
    }

    public Connection(ReliableChannel rel, UnreliableChannel unr, Func<Connection, Message, Message> handler) : this(rel,unr, 0)
    {
      var session = ReceiveSetupMessage(rel);
      if (session == null)
        throw new InvalidOperationException("Connection failed");

      Unreliable.Setup(new IPEndPoint(IPAddress.Parse(session.Address), session.port), session.SessionId);

      OnMessageReceived += handler;
      Reliable.Start();
    }

    SetupSession ReceiveSetupMessage(ReliableChannel channel)
    {
      var header = channel.ReceiveHeader().Result;
      return channel.ReceiveMessage(header).Result as SetupSession;
    }

    async void Handler(Channel channel, Message m, MessageHeader h)
    {
      var resp = OnMessageReceived?.Invoke(this, m);
      if (resp != null)
        await channel.Send(resp, h.MessageID).ConfigureAwait(false);
    }

    public event Func<Connection,Message,Message> OnMessageReceived;
  }
}
