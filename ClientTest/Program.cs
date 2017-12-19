using EBNet;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClientTest
{
  class Program
  {
    static void Main(string[] args)
    {
      var typeDict = new MessageTypeDictionary();

      using (var client = new TcpClient())
      {
        client.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.201"), 5060));
        //client.Connect(new IPEndPoint(IPAddress.Parse("10.0.2.15"), 5060));
        //client.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5060));
        //client.Connect(new IPEndPoint(IPAddress.Loopback, 5060));
        var connection = new Connection(new ReliableChannel(client, typeDict), new UnreliableChannel(typeDict), OnSomeReceived);

        connection.Reliable.Send(new TcpTestRequest() { test = "simple test" }).Wait();
        connection.Reliable.Send(new TcpTestRequest() { test = "simple test" }).Wait();
        connection.Reliable.Send(new TcpTestRequest() { test = "simple test" }).Wait();

        var r1 = new MessageChannel(connection.Reliable).Send<TcpTestResponse>(new TcpTestRequest() { test = "tcp-req" }).Result;

        for (int i = 0; i < 100; ++i)
        {
          Task.Delay(500).Wait();
          var r2 = new MessageChannel(connection.Unreliable).Send<UdpTestResponse>(new UdpTestRequest() { test = "udp-req" }).Result;
        }
        Console.WriteLine($"received responces: {res}");
        Console.ReadKey();
      }
    }

    static int res = 0;
    private static Message OnSomeReceived(Connection connection, Message msg)
    {
      if (msg is UdpTestResponse)
        res++;
      else
        Console.WriteLine(msg);
      return null;
    }
  }


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
}
