using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using EBNet;
using System.IO;

namespace EBNetTest
{
  [MessageTypeID(1)]
  [ProtoContract]
  public class HelloMsg : Message<HelloMsg>
  {
    [ProtoMember(1)]
    public string Hello { get; set; }

    [ProtoMember(2)]
    public int SomeInt1 { get; set; }

    [ProtoMember(3)]
    public float SomeFloat { get; set; }

    public override string ToString()
    {
      return $"{Hello} - {SomeInt1} - {SomeFloat}";
    }
  }

  [MessageTypeID(2)]
  [ProtoContract]
  public class MyPosition : Message<MyPosition>
  {
    [ProtoMember(1)]
    public float x;

    [ProtoMember(2)]
    public float y;

    public override string ToString()
    {
      return $"x: {x} - y: {y}";
    }
  }

  [MessageTypeID(3)]
  [ProtoContract]
  public class TcpTestRequest : Message<TcpTestRequest>
  {
    [ProtoMember(1)]
    public string test;

    public override string ToString()
    {
      return test;
    }
  }

  [MessageTypeID(4)]
  [ProtoContract]
  public class TcpTestResponse : Message<TcpTestResponse>
  {
    [ProtoMember(1)]
    public string test;

    public override string ToString()
    {
      return test;
    }
  }

  [MessageTypeID(5)]
  [ProtoContract]
  public class UdpTestRequest : Message<UdpTestRequest>
  {
    [ProtoMember(1)]
    public string test;

    public override string ToString()
    {
      return test;
    }
  }

  [MessageTypeID(6)]
  [ProtoContract]
  public class UdpTestResponse : Message<UdpTestResponse>
  {
    [ProtoMember(1)]
    public string test;

    public override string ToString()
    {
      return test;
    }
  }

  class Program
  {
    /*static Router GetClientRouter()
    {
      var router = new Router();
      router.AddHandler<HelloMsg>( (c, m) => { Console.WriteLine($"Client received: {m}"); });
      router.AddHandler<MyPosition>( (c, m) => { Console.WriteLine($"Client received: {m}"); });
      router.AddHandler<TcpTestResponse>((c, m) => { Console.WriteLine($"Client received: {m}"); });
      router.AddHandler<UdpTestResponse>((c, m) => { Console.WriteLine($"Client received: {m}"); });
      return router;
    }

    static Router GetServerRouter()
    {
      var router = new Router();
      router.AddRequestHandler<HelloMsg>((c, m) => { Console.WriteLine($"Server received: {m}"); return m; });
      router.AddHandler<MyPosition>(async (c, m) => { Console.WriteLine($"Server received: {m}"); m.x += 1; m.y += 1; await c.SendReliable(m); });
      router.AddRequestHandler<TcpTestRequest>((c, m) => { Console.WriteLine($"Server received: {m}"); return new TcpTestResponse() { test = $"ok! {m.test}" }; });
      router.AddRequestHandler<UdpTestRequest>((c, m) => { Console.WriteLine($"Server received: {m}"); return new UdpTestResponse() { test = $"ok! {m.test}" }; });
      return router;
    }*/

    static Message OnNewServerMessage(Connection connection, Message m)
    {
      Console.WriteLine(m);
      if (m is TcpTestRequest)
        return new TcpTestResponse() { test = m + "-ok" };
      else if (m is UdpTestRequest)
        return new UdpTestResponse() { test = m + "-ok" };
      //connection.GetUnreliable().Send(new UdpTestResponse() { test = m + "-ok" });
      return null;
    }


    static void Main(string[] args)
    {
      var tcpEndPoint = new IPEndPoint(IPAddress.Loopback, 5060);
      var udpEndPoint = new IPEndPoint(IPAddress.Loopback, 5061);

      //var tcpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5060);
      //var udpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5061);

      //var tcpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.201"), 5060);
      //var udpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.201"), 5061);

      var typeDict = new MessageTypeDictionary();

      var host = new Host2(tcpEndPoint, udpEndPoint, /*GetServerRouter(),*/ typeDict);
      List<Connection> cs = new List<Connection>();
      host.OnNewConnection += (c) => { Console.WriteLine("new connection"); c.OnMessageReceived += OnNewServerMessage; cs.Add(c); };
      Task.Run(() => host.Start());


      Console.ReadKey();
      Console.ReadKey();
    }
  }
}
