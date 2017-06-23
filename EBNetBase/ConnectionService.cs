using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EBNetBase
{
  public abstract class ConnectionService
  {
    CancellationTokenSource cts = new CancellationTokenSource();
    TcpListener _socket;

    public ConnectionService(TcpListener socket)
    {
      _socket = socket;
    }

    public ConnectionService(IPEndPoint endpoint)
    {
      _socket = new TcpListener(endpoint);
    }

    public Task StartAccepting()
    {
      _socket.Start();
      return Task.Factory.StartNew(() =>
      {
        while (true)
        {
          if (cts.IsCancellationRequested)
            break;

          try
          {
            var client = _socket.AcceptTcpClient();
            OnNewConnection(client);
          }
          catch (SocketException ex)
          {
            break;
          }
        }
        OnFinish();
      }, cts.Token);
    }

    public void StopAccepting()
    {
      cts.Cancel();
      _socket.Stop();
    }

    public abstract void OnNewConnection(TcpClient socket);
    public abstract void OnFinish();
  }
}
