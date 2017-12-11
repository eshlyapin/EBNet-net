using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EBNet
{
  public class Host
  {
    Random rnd = new Random();
    IPEndPoint mTcpEndPoint;
    IPEndPoint mUdpEndPoint;

    TcpListener mTcpListener;
    UdpClient mUdpListener;

    Router mRouter;
    MessageTypeDictionary mDictionary;

    Dictionary<int, Connection> mConnections = new Dictionary<int, Connection>();

    public Host(IPEndPoint tcpEndPoint, IPEndPoint udpEndPoint, Router router, MessageTypeDictionary dictionary)
    {
      mTcpEndPoint = tcpEndPoint;
      mUdpEndPoint = udpEndPoint;

      mTcpListener = new TcpListener(tcpEndPoint);
      mUdpListener = new UdpClient(udpEndPoint);

      mRouter = router;
      mDictionary = dictionary;
    }

    public void Start()
    {
      mTcpListener.Start();
      Task.Factory.StartNew(async () =>
      {
        while (true)
        {
          var tcpClient = mTcpListener.AcceptTcpClient();
          var connection = new Connection(tcpClient, mRouter, mDictionary);
          var sessionID = rnd.Next();
          mConnections.Add(sessionID, connection);

          var session = new SetupSession() { SessionId = sessionID, Address = mUdpEndPoint.Address.ToString(), port = mUdpEndPoint.Port };
          await connection.SendReliable(session);
          connection.SetupUnreliable(sessionID, new IPEndPoint(IPAddress.Parse(session.Address), session.port));
        }
      });

      Task.Factory.StartNew(async () =>
      {
        while (true)
        {
          var received = await mUdpListener.ReceiveAsync();
          using (var stream = new MemoryStream(received.Buffer))
          {
            var header = new UdpMessageHeader(stream);
            if (mConnections.ContainsKey(header.SessionId))
            {
              var connection = mConnections[header.SessionId];
              var response = connection.HandleMessage(header.TypeID, header.MessageID, stream);

              if (response != null)
                await connection.SendUnreliable(response, header.MessageID);
            }
          }
        }
      });
    }

    public void Stop()
    {

    }
  }
}
