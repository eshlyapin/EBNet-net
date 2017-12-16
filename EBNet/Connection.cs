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
      while (!IsActive) ;
      return Reliable;
    }
    public UnreliableChannel Unreliable { get; private set; }

    internal Connection(ReliableChannel rel, UnreliableChannel unr, bool accepted)
    {
      Reliable = rel;
      Unreliable = unr;
      if (!accepted)
      {
        Reliable.OnMessageReceived += MessageHandler;
        Unreliable.OnMessageReceived += MessageHandler;
      }
      Reliable.Start();
    }

    public Connection(ReliableChannel rel, UnreliableChannel unr) : this(rel, unr, false)
    {
    }

    private void MessageHandler(Channel channel, Message m, MessageHeader h)
    {
      if (m is SetupSession)
      {
        var session = m as SetupSession;
        Unreliable.SessionID = session.SessionId;
        Reliable.OnMessageReceived -= MessageHandler;
        Unreliable.OnMessageReceived -= MessageHandler;
        Reliable.OnMessageReceived += OnMessageReceived;
        Unreliable.OnMessageReceived += OnMessageReceived;
        IsActive = true;
      }
    }

    public event Channel.MessageHandler OnMessageReceived;
  }
}
