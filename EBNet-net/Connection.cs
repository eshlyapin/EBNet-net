using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EBNet_net
{
  public class Connection : EBNetBase.Connection<HeaderFormat>
  {
    public static Router Router { get; } = new Router();
    public static Action<Connection, Exception> Disconnection { get; set; }

    public Connection(TcpClient socket)
      : base(socket)
    {
    }

    public Connection(IPEndPoint endpoint)
      : base(new TcpClient())
    {
      _socket.Connect(endpoint);
    }

    public Task SendResponce(EBNetBase.Message message, int id)
    {
      var format = new HeaderFormat();
      var buffer = format.WrapMessage(message, id);
      return _socket.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public Task<EBNetBase.Message> SendRequest(EBNetBase.Message message)
    {
      return Task<EBNetBase.Message>.Factory.StartNew(() =>
      {
        var header = new HeaderFormat();
        var buffer = header.WrapMessage(message, GetNextMsgID());

        _socket.GetStream().WriteAsync(buffer, 0, buffer.Length).Wait();

        return WaitForResponce(header.MessageID);
      });
    }

    public override void OnDisconection(Exception ex)
    {
      Disconnection?.Invoke(this, ex);
    }

    public override void OnPayloadReady(HeaderFormat format, byte[] buffer)
    {
      var message = Activator.CreateInstance(format.MessageType) as EBNetBase.Message;
      message.ReadFrom(new BinaryReader(new MemoryStream(buffer)));

      if (messageReceived.ContainsKey(format.MessageID))
        messageReceived[format.MessageID] = message;
      else
        Router.Handle(this, message, format);
    }

    int GetNextMsgID()
    {
      Interlocked.Increment(ref messageId);
      return messageId;
    }

    EBNetBase.Message WaitForResponce(int id)
    {
      var responce = messageReceived.GetOrAdd(id, value: null);

      while (responce == null)
        responce = messageReceived[id];
      return responce;
    }

    ConcurrentDictionary<int, EBNetBase.Message> messageReceived = new ConcurrentDictionary<int, EBNetBase.Message>();
    int messageId = int.MinValue;
  }
}
