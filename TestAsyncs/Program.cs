using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TestAsyncs
{
  class Server
  {
    TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 5050));
    bool works;

    public Server()
    {
      listener.Start();
      works = true;
      Task.Run(async () =>
      {
        while (works)
        {
          var sock = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
          ReceivingGood(sock);
        }
      });
    }

    async Task Receiving(TcpClient client)
    {
      while(works)
      {
        var msg = new List<byte[]>();

        var buffer = new byte[8];
        int read = 0;
        while (read < buffer.Length)
          read += await client.GetStream().ReadAsync(buffer, read, buffer.Length - read).ConfigureAwait(false);
        msg.Add(buffer);

        buffer = new byte[16];
        read = 0;
        while (read < buffer.Length)
          read += await client.GetStream().ReadAsync(buffer, read, buffer.Length - read).ConfigureAwait(false);
        msg.Add(buffer);

        Console.WriteLine($"TMessage: {msg[0][0]}");
      }
    }

    async Task ReceivingGood(TcpClient client)
    {
      while (works)
      {
        var msg = new List<byte[]>();

        var header = await Receive(8, client.GetStream());
        var payload = await Receive(16, client.GetStream());

        msg.Add(header);
        msg.Add(payload);

        Console.WriteLine($"TMessage: {msg[0][0]}");
      }
    }

    async Task<byte[]> Receive(int count, NetworkStream stream)
    {
      var buffer = new byte[8];
      int read = 0;
      while (read < buffer.Length)
        read += await stream.ReadAsync(buffer, read, buffer.Length - read).ConfigureAwait(false);
      return buffer;
    }
  }

  class Client
  {
    public static void Run(int count)
    {
      for (int i = 0; i < count; ++i)
      {
        var client = new TcpClient();
        client.Connect(IPAddress.Loopback, 5050);

        var buffer = Fill(i);
        client.GetStream().WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
      }
    }

    static byte[] Fill(int init)
    {
      var buffer = new byte[24];
      for(byte i = 1; i < buffer.Length; ++i)
      {
        buffer[i] = i;
      }
      buffer[0] = (byte)init;
      return buffer;
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      var server = new Server();
      Client.Run(50);
      while (true) ;
    }
  }
}
