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
    int MessageID = 1;

    public MessageChannel(Channel owner)
      : base(owner.TypeDictionary)
    {
      mOwner = owner;
    }

    public async Task<T> Send<T>(Message m) where T : Message<T>
    {
      mOwner.OnMessageReceived += HandleMessage; //TODO:
      var res = new TaskCompletionSource<Message>();
      awaiters.TryAdd(MessageID, res);  //TODO:
      await Send(m, MessageID).ConfigureAwait(false);
      var response = await res.Task.ConfigureAwait(false);
      awaiters.TryRemove(MessageID, out res);  //TODO:
      Interlocked.Increment(ref MessageID);
      mOwner.OnMessageReceived -= HandleMessage; //TODO:
      return response as T;
    }

    internal override MessageHeader CreateHeader(Message msg, int messageId)
    {
      return mOwner.CreateHeader(msg, MessageID);
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
        mOwner.OnMessageReceived -= HandleMessage; //TODO:
        mOwner.RaiseMessageReceived(channel, msg, header);
        mOwner.OnMessageReceived += HandleMessage; //TODO:
      }
    }

    public override void Close()
    {
      mOwner.Close();
    }
  }
}
