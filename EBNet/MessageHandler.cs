using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EBNet
{
  public class MessageTypeDictionary
  {
    List<Tuple<Type, int>> msgIDs = new List<Tuple<Type, int>>();

    public MessageTypeDictionary(Assembly[] assemblies)
    {
      var result = new List<Type>();
      foreach (var assemly in assemblies)
      {
        var messages = assemly.GetTypes().Where((t) => t.GetCustomAttribute<MessageTypeID>() != null && !t.IsAbstract && t.IsSubclassOf(typeof(Message))).ToArray();
        result.AddRange(messages);
      }

      foreach (var msg in result)
      {
        msgIDs.Add(new Tuple<Type, int>(msg, msg.GetCustomAttribute<MessageTypeID>().TypeID));
      }
    }
    public MessageTypeDictionary() : this(AppDomain.CurrentDomain.GetAssemblies())
    {
    }

    public int GetTypeID(Type type)
    {
      return msgIDs.Single(entry => entry.Item1 == type).Item2;
    }

    public Type GetTypeByID(int id)
    {
      return msgIDs.Single(entry => entry.Item2 == id).Item1;
    }
  }

  public class Router
  {
    Dictionary<Type, Func<Connection, Message, Message>> handlers = new Dictionary<Type, Func<Connection, Message, Message>>();

    public void AddHandler<M>(Action<Connection, M> handler) where M : Message
    {
      AddRequestHandler<M>((connection, message) => { handler(connection, message); return null; });
    }

    public void AddRequestHandler<M>(Func<Connection, M, Message> handler)  where M : Message
    {
      if (handlers.ContainsKey(typeof(M)))
        handlers[typeof(M)] += (connection, message) =>  handler(connection, (M)message);
      else
        handlers.Add(typeof(M), (connection, message) =>  handler(connection, (M)message));
    }

    public void ClearHandler<M>() where M : Message
    {
      if (handlers.ContainsKey(typeof(M)))
        handlers.Remove(typeof(M));
    }

    public Message Handle(Connection sender, Message msg)
    {
      return handlers[msg.GetType()]?.Invoke(sender, msg);
    }
  }
}
