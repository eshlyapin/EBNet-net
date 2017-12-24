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

      //the UDP socket is also receiving ICMP messages and throwing exceptions when they are received.
      //https://stackoverflow.com/questions/5199026/c-sharp-async-udp-listener-socketexception
      int SIO_UDP_CONNRESET = -1744830452;
      mListener.Client.IOControl(
          (IOControlCode)SIO_UDP_CONNRESET,
          new byte[] { 0, 0, 0, 0 },
          null
      );
    }

    public async void Start()
    {
      while (!cancellationSource.IsCancellationRequested)
      {
        try
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
        catch(SocketException ex)
        {
          //Console.WriteLine(ex.Message);
        }
      }
    }

    public UnreliableChannel RegisterChannel(int sessionId)
    {
      var channel = new UnreliableChannel(mListener, sessionId, mTypeDictionary);
      channel.OnClose += UnregisterChanel;
      mClients.TryAdd(sessionId, channel);
      return channel;
    }

    public void UnregisterChanel(UnreliableChannel channel)
    {
      //TODO:
      UnreliableChannel removed;
      mClients.TryRemove(channel.SessionID, out removed);
      removed.OnClose -= UnregisterChanel;
    }

    public void Stop()
    {
      cancellationSource.Cancel();
    }
  }
}
