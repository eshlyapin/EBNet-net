using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EBNet
{
  class ReliableHost
  {
    TcpListener mListener;
    MessageTypeDictionary mTypeDictionary;
    CancellationTokenSource cancellationSource = new CancellationTokenSource();

    public ReliableHost(IPEndPoint ep, MessageTypeDictionary dict)
    {
      mTypeDictionary = dict;
      mListener = new TcpListener(ep);
    }

    public async void Start()
    {
      mListener.Start();
      while (!cancellationSource.IsCancellationRequested)
      {
        var socket = await mListener.AcceptTcpClientAsync().ConfigureAwait(false);
        OnNewConnection(new ReliableChannel(socket, mTypeDictionary));
      }
    }

    public void Stop()
    {
      cancellationSource.Cancel();
    }

    public delegate void NewConnectionHandler(ReliableChannel client);
    public event NewConnectionHandler OnNewConnection;
  }
}
