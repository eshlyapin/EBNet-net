using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using EBNet;

namespace EBNetTest
{
  class Program
  {
    [MessageTypeID(1)]
    [ProtoContract]
    class HelloMsg : Message<HelloMsg>
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
    class MyPosition : Message<MyPosition>
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
    class TcpTestRequest : Message<TcpTestRequest>
    {
      [ProtoMember(1)]
      public string test;

      public override string ToString()
      {
        return $"tcp text req: {test}";
      }
    }

    [MessageTypeID(4)]
    [ProtoContract]
    class TcpTestResponse : Message<TcpTestResponse>
    {
      [ProtoMember(1)]
      public string test;

      public override string ToString()
      {
        return $"tci text resp: {test}";
      }
    }

    [MessageTypeID(5)]
    [ProtoContract]
    class UdpTestRequest : Message<UdpTestRequest>
    {
      [ProtoMember(1)]
      public string test;

      public override string ToString()
      {
        return $"udp text req: {test}";
      }
    }

    [MessageTypeID(6)]
    [ProtoContract]
    class UdpTestResponse : Message<UdpTestResponse>
    {
      [ProtoMember(1)]
      public string test;

      public override string ToString()
      {
        return $"udp text resp: {test}";
      }
    }

    static Router GetClientRouter()
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
    }

    static void Main(string[] args)
    {
      var tcpEndPoint = new IPEndPoint(IPAddress.Loopback, 5060);
      var udpEndPoint = new IPEndPoint(IPAddress.Loopback, 5061);

      var typeDict = new MessageTypeDictionary();

      var host = new Host(tcpEndPoint, udpEndPoint, GetServerRouter(), typeDict);
      host.Start();

      var client = new TcpClient();
      client.Connect(new IPEndPoint(IPAddress.Loopback, 5060));
      var connection = new Connection(client, GetClientRouter(), typeDict);

      connection.SendReliable(new HelloMsg() { Hello = "Hello world!", SomeInt1 = 42, SomeFloat = 0.0003f }).Wait();
      var resp = connection.SendReliable<HelloMsg>(new HelloMsg() { Hello = "Req msg", SomeInt1 = 0, SomeFloat = 0.0f }).Result;

      var tcpresp = connection.SendReliable<TcpTestResponse>(new TcpTestRequest() { test = "TCPRequest" }).Result;
      Console.WriteLine($"Client received: {tcpresp}");
      var udpresp = connection.SendReliable<UdpTestResponse>(new UdpTestRequest() { test = "UDPRequest" }).Result;
      Console.WriteLine($"Client received: {udpresp}");

      Console.ReadKey();
    }
  }
}
