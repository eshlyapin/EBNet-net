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
  public class ReliableChannel : Channel
  {
    TcpClient mClient;
    CancellationTokenSource cancellationSource = new CancellationTokenSource();

    public ReliableChannel(TcpClient client, MessageTypeDictionary dict) : base(dict)
    {
      mClient = client;
    }

    public async Task Start()
    {
      Console.WriteLine("called");
      try
      {
        while (!cancellationSource.IsCancellationRequested)
        {
          var headerbuffer = await Receive(TcpMessageHeader.HeaderLength);
          var header = new TcpMessageHeader(new MemoryStream(headerbuffer));

          var buffer = await Receive(header.Length);

          var msgType = TypeDictionary.GetTypeByID(header.TypeID);
          var message = Serializer.Deserialize(msgType, new MemoryStream(buffer)) as Message;
          RaiseMessageReceived(this, message, header);
        }

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    public async Task<byte[]> Receive(int count)
    {
      var result = new byte[count];
      int read = 0;
      while (read < count)
        read += await mClient.GetStream().ReadAsync(result, read, count - read).ConfigureAwait(false);
      return result;
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
  }
}
