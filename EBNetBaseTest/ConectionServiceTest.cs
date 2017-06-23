using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using EBNetBase;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Test
{
  [TestClass]
  public class ConectionServiceTest
  {
    static TestConnectionService Service { get; set; }

    [ClassInitialize]
    public static void Init(TestContext ctx)
    {
      var socket = new TcpListener(IPAddress.Loopback, 5055);
      Service = new TestConnectionService( socket );
      Service.StartAccepting();
    }

    [ClassCleanup]
    public static void Cleanup()
    {
      Service.StopAccepting();
    }

    [TestMethod]
    public async Task SimpleConnection()
    {
      var socket = new TcpClient();
      socket.Connect(new IPEndPoint(IPAddress.Loopback, 5055));
      var connection = new TestConnection(socket);

      await connection.Send(new TestMessage1(500, new byte[] { 1, 1, 1, 1, 1 }));
      await connection.Send(new TestMessage2(500, new byte[] { 2, 2, 2 }));
      await connection.Send(new TestMessage2(500, new byte[] { 2 }));
      await connection.Send(new TestMessage3(500, new byte[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }));
      await connection.Send(new TestMessage1(500, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }));
    }

    [TestMethod]
    public async Task MultiConnection()
    {
      var tasks = new List<Task>();
      var connections = new List<TestConnection>();

      for (int iConnection = 0; iConnection < 100; ++iConnection)
      {
        var socket = new TcpClient();
        socket.Connect(new IPEndPoint(IPAddress.Loopback, 5055));
        var connection = new TestConnection(socket);
        connections.Add(connection);

        for (byte iSend = 0; iSend < 10; ++iSend)
          tasks.Add(connection.Send(new TestMessage1(100 + iConnection, new byte[] { iSend, iSend, iSend })));
      }
      //Task.WaitAll(tasks.ToArray());
      await Task.WhenAny(tasks);
    }
  }
}
