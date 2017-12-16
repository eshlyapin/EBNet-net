using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EBNet
{
  public class MessageChannel : Channel
  {
    Channel mOwner;
    ConcurrentDictionary<int, TaskCompletionSource<Message>> awaiters = new ConcurrentDictionary<int, TaskCompletionSource<Message>>();
    int MessageID = 0;

    public MessageChannel(Channel owner)
      : base(owner.TypeDictionary)
    {
      mOwner = owner;
      OnMessageReceived += HandleMessage;
    }

    public async Task<T> Send<T>(Message m) where T : Message<T>
    {
      var res = new TaskCompletionSource<Message>();
      awaiters.TryAdd(MessageID, res);
      await Send(m);
      var response = await res.Task;
      awaiters.TryRemove(MessageID, out res);
      Interlocked.Increment(ref MessageID);
      return response as T;
    }

    internal override MessageHeader CreateHeader(Message msg)
    {
      var header = mOwner.CreateHeader(msg);
      header.MessageID = MessageID;
      return header;
    }

    internal override Task Write(MemoryStream source)
    {
      return mOwner.Write(source);
    }

    void HandleMessage(Channel channel, Message msg, MessageHeader header)
    {
      if (awaiters.ContainsKey(header.MessageID))
      {
        var result = awaiters[header.MessageID];
        result.SetResult(msg);
      }
      else
      {
        mOwner.RaiseMessageReceived(channel, msg, header);
      }
    }
  }
}
