using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EBNetBase
{
  public abstract class Connection<FormatDescription> where FormatDescription : PacketFormatDescription, new()
  {
    protected TcpClient mSocket { get; set; }
    CancellationTokenSource mTokenSource = new CancellationTokenSource();

    public abstract void OnPayloadReady(FormatDescription format, byte[] buffer);
    public abstract void OnDisconection(Exception ex);

    public Connection(TcpClient socket)
    {
      mSocket = socket;
    }

    public async void StartReceiving()
    {
      while (!mTokenSource.IsCancellationRequested)
      {
        try
        {
          var format = new FormatDescription();
          byte[] buffer = new byte[format.WrapperSize];
          int read = 0;

          do
          {
            read += await mSocket.GetStream().ReadAsync(buffer, read, buffer.Length - read, mTokenSource.Token).ConfigureAwait(false);
          } while (read < buffer.Length);

          format.ParseWrapper(buffer);

          buffer = new byte[format.PayloadSize];
          read = 0;

          if (format.PayloadSize > 0)
          {
            do
            {
              read += await mSocket.GetStream().ReadAsync(buffer, read, buffer.Length - read, mTokenSource.Token).ConfigureAwait(false);
            } while (read < buffer.Length);
          }

          OnPayloadReady(format, buffer);
        }
        catch (Exception ex)
        {
          mSocket.Close();
          OnDisconection(ex);
          break;
        }
      }
    }

    public Task Send(Message msg)
    {
      var format = new FormatDescription();
      var buffer = format.WrapMessage(msg);
      return mSocket.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public void StopReceiving()
    {
      mTokenSource.Cancel();
    }
  }
}
