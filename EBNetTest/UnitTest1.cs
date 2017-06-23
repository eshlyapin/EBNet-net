using System;
using ProtoBuf;
using EBNet_net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EBNetTest
{
  [TestClass]
  public class UnitTest1
  {
    public void WaitForEnd()
    {
      bool exit = false;
      Connection.Router.AddHandler<EndOfTest>((con, request, header) => exit = true);
      while (!exit) ;
    }

    [TestMethod]
    public void TestMethod1()
    {
      var socket = new TcpListener(new IPEndPoint(IPAddress.Loopback, 5555));
      socket.Start();
      var service = new ConnectionService(socket);
      service.StartAccepting();


      var cons = new ConcurrentBag<Connection>();

      Connection.Router.AddHandler<LoginRequest>((con, request, header) =>
      {
        var res = new LoginResponce();
        if (request.Username.Contains("user") && request.Password.Contains("123"))
          res.Status = true;
        else
          res.Status = false;
        con.SendResponce(res, header.MessageID).Wait();
        cons.Add(con);
      });

      WaitForEnd();
      while (true) ;
      service.StopAccepting();
    }
  }
}
