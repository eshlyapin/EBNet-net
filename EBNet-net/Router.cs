using System;
using System.Collections.Generic;
using System.Linq;
using EBNetBase;

namespace EBNet_net
{
  public abstract class MessageHandlerBase
  {
    public abstract void Handle(Connection c, Message msg, HeaderFormat header);
  }

  public class MessageHandler<T> : MessageHandlerBase where T : EBNetBase.Message
  {
    Action<Connection, T, HeaderFormat> handler;

    public MessageHandler(Action<Connection, T, HeaderFormat> handler)
    {
      this.handler = handler;
    }

    public override void Handle(Connection c, Message msg, HeaderFormat header)
    {
      handler(c,(T)msg,header);
    }
  }

  public class Router
  {
    Dictionary<Type, MessageHandlerBase> handlers = new Dictionary<Type, MessageHandlerBase>();

    public void AddHandler<Message>(Action<Connection,Message,HeaderFormat> handler) where Message : EBNetBase.Message
    {
      handlers.Add(typeof(Message), new MessageHandler<Message>(handler));
    }

    public void Handle(Connection sender, Message msg, HeaderFormat header)
    {
      handlers[msg.GetType()]?.Handle(sender, msg, header);
    }
  }
}
