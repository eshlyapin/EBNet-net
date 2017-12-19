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

    public Task Send(Message msg)
    {
      return Send(msg, DefaultMessageId);
    }

    public abstract void Close();

    internal abstract MessageHeader CreateHeader(Message msg, int messageId);
    internal abstract Task Write(MemoryStream source);

    internal Task Send(Message msg, int messageId)
    {
      var header = CreateHeader(msg, messageId);
      return Write(header.Wrap(msg));
    }

    public abstract void OnReceived(MessageHeader header, Message msg);
  }
}
