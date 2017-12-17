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
    int SessionID { get; set; }
    bool IsActive = false;

    ReliableChannel Reliable { get; set; }

    public ReliableChannel GetReliable()
    {
      //TODO:
      while (!IsActive) ;
      return Reliable;
    }

    UnreliableChannel Unreliable { get; set; }

    public UnreliableChannel GetUnreliable()
    {
      //TODO:
      while (!IsActive) ;
      return Unreliable;
    }


    public Connection(ReliableChannel rel, UnreliableChannel unr) 
    {
      Reliable = rel;
      Unreliable = unr;
      Reliable.OnMessageReceived += Handler;
      Unreliable.OnMessageReceived += Handler;
      Reliable.Start();
    }

    private async void Handler(Channel channel, Message m, MessageHeader h)
    {
      if (m is SetupSession)
      {
        var session = m as SetupSession;
        Unreliable.SessionID = session.SessionId;
        Unreliable.Setup(new IPEndPoint(IPAddress.Parse(session.Address), session.port));
        IsActive = true;
      }
      else
      {
        var resp = OnMessageReceived?.Invoke(this, m);
        if (resp != null)
          await channel.Send(resp, h.MessageID);
      }
    }

    public delegate Message MessageHandler(Connection connection, Message msg);
    public event MessageHandler OnMessageReceived;
  }
}
