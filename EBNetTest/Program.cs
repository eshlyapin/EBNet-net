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
        return test;
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
        return test;
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
        return test;
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
        return test;
      }
    }

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

    static async void OnNewServerMessage(Channel c, Message m, MessageHeader header)
    {
      if (m is TcpTestRequest)
        await c.Send(new TcpTestResponse() { test = "ok!" });
    }

    static void TestSer(MessageTypeDictionary typeDict)
    {
      byte[] h1buffer;
      using (var stream = new MemoryStream())
      {
        var h1 = new TcpMessageHeader() { MessageID = 0, TypeID = typeDict.GetTypeID(typeof(TcpTestRequest)) };
        h1.WriteTo(stream);
        h1buffer = stream.ToArray();
      }


      byte[] mbuffer;
      using (var stream = new MemoryStream())
      {
        var m = new TcpTestRequest() { test = "test" };
        m.WriteTo(stream);
        mbuffer = stream.ToArray();
      }

      using (var stream = new MemoryStream(mbuffer))
      {
        var h2 = new TcpMessageHeader(new MemoryStream(h1buffer));
        var m = Serializer.Deserialize(typeDict.GetTypeByID(h2.TypeID), stream);
        Console.WriteLine(m);
      }

    }

    static void Main(string[] args)
    {
      var tcpEndPoint = new IPEndPoint(IPAddress.Loopback, 5060);
      var udpEndPoint = new IPEndPoint(IPAddress.Loopback, 5061);

      var typeDict = new MessageTypeDictionary();
      TestSer(typeDict);

      var host = new Host2(tcpEndPoint, udpEndPoint, /*GetServerRouter(),*/ typeDict);
      List<Connection> cs = new List<Connection>();
      host.OnNewConnection += (c) => { Console.WriteLine("new connection"); c.OnMessageReceived += OnNewServerMessage; cs.Add(c); };
      Task.Run(() => host.Start());

      var client = new TcpClient();
      client.Connect(new IPEndPoint(IPAddress.Loopback, 5060));
      var connection = new Connection(new ReliableChannel(client, typeDict), new UnreliableChannel(typeDict));

      connection.GetReliable().Send(new TcpTestRequest() { test = "simple test" }).Wait();
      connection.GetReliable().Send(new TcpTestRequest() { test = "simple test" }).Wait();
      connection.GetReliable().Send(new TcpTestRequest() { test = "simple test" }).Wait();
      //var mch = new MessageChannel(connection.GetReliable());
      //var r1 = mch.Send<TcpTestResponse>(new TcpTestRequest() { test = "test request" }).Result;

      Console.ReadKey();
      Console.ReadKey();
    }
  }
}
