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
      try
      {
        while (!cancellationSource.IsCancellationRequested)
        {
          var buffer = await Receive(TcpMessageHeader.HeaderLength);
          var header = new TcpMessageHeader(new MemoryStream(buffer));

          buffer = await Receive(header.Length);

          Console.Write($"Received: ");
          foreach (var b in buffer)
            Console.Write($"{b}");
          Console.WriteLine();

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
      //Console.Write($"count?: {count} ");
      var result = new byte[count];
      int read = 0;
      while (read < count)
        read += await mClient.GetStream().ReadAsync(result, read, count - read).ConfigureAwait(false);
      //Console.WriteLine($":{result.Length}");
      return result;
    }

    internal override MessageHeader CreateHeader(Message msg)
    {
      return new TcpMessageHeader() { TypeID = TypeDictionary.GetTypeID(msg.GetType()), MessageID = DefaultMessageId };
    }

    internal override Task Write(MemoryStream source)
    {
      var buffer = source.ToArray();
      return mClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }
  }
}
