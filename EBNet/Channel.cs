using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNet
{
  public abstract class Channel
  {
    public MessageTypeDictionary TypeDictionary { get; set; }

    protected const int DefaultMessageId = 0;

    public Channel(MessageTypeDictionary dict)
    {
      TypeDictionary = dict;
    }

    internal abstract MessageHeader CreateHeader(Message msg);
    internal abstract Task Write(MemoryStream source);

    public Task Send(Message msg)
    {
      var header = CreateHeader(msg);
      return Write(header.Wrap(msg));
    }

    internal void RaiseMessageReceived(Channel channel, Message m, MessageHeader h)
    {
      OnMessageReceived?.Invoke(channel, m, h);
    }

    public delegate void MessageHandler(Channel channel, Message m, MessageHeader h);
    public event MessageHandler OnMessageReceived;
  }
}
