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

    static Message OnNewServerMessage(Connection connection, Message m)
    {
      Console.WriteLine(m);
      if (m is TcpTestRequest)
        return new TcpTestResponse() { test = m + "-ok" };
      else if (m is UdpTestRequest)
        return new UdpTestResponse() { test = m + "-ok" };
      return null;
    }


    static void Main(string[] args)
    {
      //var tcpEndPoint = new IPEndPoint(IPAddress.Loopback, 5060);
      //var udpEndPoint = new IPEndPoint(IPAddress.Loopback, 5061);

      //var tcpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5060);
      //var udpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5061);

      var tcpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.201"), 5060);
      var udpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.201"), 5061);

      var typeDict = new MessageTypeDictionary();

      var host = new Host2(tcpEndPoint, udpEndPoint, typeDict);
      List<Connection> cs = new List<Connection>();
      host.OnNewConnection += (c) => { Console.WriteLine("new connection"); c.OnMessageReceived += OnNewServerMessage; cs.Add(c); };
      Task.Run(() => host.Start());


      Console.ReadKey();
      Console.ReadKey();
    }
  }
}
