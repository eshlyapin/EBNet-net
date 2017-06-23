using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EBNet_net
{
  public class ConnectionService : EBNetBase.ConnectionService
  {
    public ConnectionService(TcpListener socket)
      : base(socket)
    {
    }

    public ConnectionService(IPEndPoint endpoint)
      : base(endpoint)
    {
    }

    public override void OnFinish()
    {
      //throw new NotImplementedException();
    }

    public override void OnNewConnection(TcpClient socket)
    {
      var connection = new Connection(socket);
      connection.StartReceiving();
    }
  }
}
