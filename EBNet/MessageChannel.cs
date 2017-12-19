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
  public abstract class MessageChannel : Channel
  {
    ConcurrentDictionary<int, TaskCompletionSource<Message>> awaiters = new ConcurrentDictionary<int, TaskCompletionSource<Message>>();
    int MessageID = 1;

    public MessageChannel(MessageTypeDictionary dict)
      : base(dict)
    {
    }

    public async Task<T> Send<T>(Message m) where T : Message<T>
    {
      var res = new TaskCompletionSource<Message>();
      awaiters.TryAdd(MessageID, res);  //TODO:
      await Send(m, MessageID).ConfigureAwait(false);
      var response = await res.Task.ConfigureAwait(false);
      awaiters.TryRemove(MessageID, out res);  //TODO:
      Interlocked.Increment(ref MessageID);
      return response as T;
    }

    public override void OnReceived(MessageHeader header, Message msg)
    {
      if (awaiters.ContainsKey(header.MessageID))
      {
        var result = awaiters[header.MessageID];
        result.SetResult(msg);
      }
      else
      {
        OnMessageReceived?.Invoke(this, msg, header);
      }
    }

    public event Action<Channel, Message, MessageHeader> OnMessageReceived;
  }
}
