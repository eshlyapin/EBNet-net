﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EBNet
{
  public class Host2
  {
    ReliableHost rHost;
    UnreliableHost urHost;

    public Host2(IPEndPoint rep, IPEndPoint urep, MessageTypeDictionary dict)
    {
      rHost = new ReliableHost(rep, dict);
      urHost = new UnreliableHost(urep, dict);

      rHost.OnNewConnection += HandleReliableConnection;
    }

    public void Start()
    {
      rHost.Start();
      urHost.Start();
    }

    private async void HandleReliableConnection(ReliableChannel client)
    {
      var sessionId = new Random().Next(); //TODO:
      await client.Send(new SetupSession() { Address = urHost.HostEndPoint.Address.ToString(), port = urHost.HostEndPoint.Port, SessionId = sessionId }).ConfigureAwait(false);
      var channel = urHost.RegisterChannel(sessionId);
      var connection = new Connection(client, channel, sessionId);
      OnNewConnection(connection);
      connection.Reliable.Start();
    }

    public event Action<Connection> OnNewConnection;
  }
}
