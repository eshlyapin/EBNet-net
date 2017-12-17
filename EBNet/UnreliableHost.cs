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
  class UnreliableHost
  {
    UdpClient mListener { get; set; }
    public IPEndPoint HostEndPoint { get; private set; }
    MessageTypeDictionary mTypeDictionary;
    CancellationTokenSource cancellationSource = new CancellationTokenSource();

    ConcurrentDictionary<int, UnreliableChannel> mClients = new ConcurrentDictionary<int, UnreliableChannel>();

    public UnreliableHost(IPEndPoint ep, MessageTypeDictionary dict)
    {
      HostEndPoint = ep;
      mTypeDictionary = dict;
      mListener = new UdpClient(ep);
      mListener.ExclusiveAddressUse = false;
    }

    public async void Start()
    {
      while (!cancellationSource.IsCancellationRequested)
      {
        var datagram = await mListener.ReceiveAsync().ConfigureAwait(false);

        using (var stream = new MemoryStream(datagram.Buffer))
        {
          var header = new UdpMessageHeader(stream);
          if (mClients.ContainsKey(header.SessionId))
          {
            var channel = mClients[header.SessionId];
            channel.RaiseDatagramReceived(datagram);
          }
        }
      }
    }

    public UnreliableChannel RegisterChannel(int sessionId)
    {
      var channel = new UnreliableChannel(HostEndPoint, mTypeDictionary);
      channel.SessionID = sessionId;
      mClients.TryAdd(sessionId, channel);
      return channel;
    }

    public void Stop()
    {
      cancellationSource.Cancel();
    }
  }
}
