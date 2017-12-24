using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EBNet
{
  public class ReliableChannel : MessageChannel
  {
    TcpClient mClient;
    CancellationTokenSource cancellationSource = new CancellationTokenSource();

    public ReliableChannel(TcpClient client, MessageTypeDictionary dict) : base(dict)
    {
      mClient = client;
    }

    public async Task Start()
    {
      try
      {
        while (!cancellationSource.IsCancellationRequested)
        {
          var header = await ReceiveHeader().ConfigureAwait(false);
          var message = await ReceiveMessage(header).ConfigureAwait(false);
          OnReceived(header, message);
        }

      }
      catch (Exception ex)
      {
        //Console.WriteLine(ex.Message);
      }
    }

    async Task<byte[]> Receive(int count)
    {
      var result = new byte[count];
      int read = 0;
      while (read < count)
        read += await mClient.GetStream().ReadAsync(result, read, count - read).ConfigureAwait(false);
      return result;
    }

    internal async Task<TcpMessageHeader> ReceiveHeader()
    {
      var headerbuffer = await Receive(TcpMessageHeader.HeaderLength).ConfigureAwait(false);
      return new TcpMessageHeader(new MemoryStream(headerbuffer));
    }

    internal async Task<Message> ReceiveMessage(TcpMessageHeader header)
    {
      var buffer = await Receive(header.Length).ConfigureAwait(false);
      var msgType = TypeDictionary.GetTypeByID(header.TypeID);
      return Serializer.Deserialize(msgType, new MemoryStream(buffer)) as Message;
    }

    internal override MessageHeader CreateHeader(Message msg, int messageId)
    {
      return new TcpMessageHeader() { TypeID = TypeDictionary.GetTypeID(msg.GetType()), MessageID = messageId };
    }

    internal override Task Write(MemoryStream source)
    {
      var buffer = source.ToArray();
      return mClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }

    public override void Close()
    {
      cancellationSource.Cancel();
      mClient.Close();
      //TODO:
    }
  }
}
