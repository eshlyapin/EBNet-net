using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EBNetBase
{
  public abstract class Connection<FormatDescription> where FormatDescription : PacketFormatDescription, new()
  {
    protected TcpClient _socket { get; set; }
    CancellationTokenSource _tokenSource = new CancellationTokenSource();

    public abstract void OnPayloadReady(FormatDescription format, byte[] buffer);
    public abstract void OnDisconection(Exception ex);

    public Connection(TcpClient socket)
    {
      _socket = socket;
    }

    public async void StartReceiving()
    {
      while (!_tokenSource.IsCancellationRequested)
      {
        try
        {
          var format = new FormatDescription();
          byte[] buffer = new byte[format.WrapperSize];
          int read = 0;

          do
          {
            read += await _socket.GetStream().ReadAsync(buffer, read, buffer.Length - read, _tokenSource.Token).ConfigureAwait(false);
          } while (read < buffer.Length);

          format.ParseWrapper(buffer);

          buffer = new byte[format.PayloadSize];
          read = 0;

          if (format.PayloadSize > 0)
          {
            do
            {
              read += await _socket.GetStream().ReadAsync(buffer, read, buffer.Length - read, _tokenSource.Token).ConfigureAwait(false);
            } while (read < buffer.Length);
          }

          OnPayloadReady(format, buffer);
        }
        catch (Exception ex)
        {
          _socket.Close();
          OnDisconection(ex);
          break;
        }
      }
    }

    public Task Send(Message msg)
    {
      var format = new FormatDescription();
      var buffer = format.WrapMessage(msg);
      return _socket.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public void StopReceiving()
    {
      _tokenSource.Cancel();
    }
  }
}
