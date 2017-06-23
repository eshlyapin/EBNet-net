using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtoBuf;

namespace EBNet_net
{
  public class EBNetID : Attribute
  {
    public uint MessageID { get; private set; }
    public EBNetID(uint id)
    {
      MessageID = id;
    }
  }

  public class Message<T> : EBNetBase.Message where T: Message<T>
  {
    public override void ReadFrom(BinaryReader stream)
    {
      Serializer.Merge<T>(stream.BaseStream, (T)this);
    }

    public override void WriteTo(BinaryWriter stream)
    {
      Serializer.Serialize<T>(stream.BaseStream, (T)this);
    }
  }

  public class MessageManager
  {
    List<Tuple<Type, uint>> msgIDs = new List<Tuple<Type, uint>>();

    public MessageManager()
    {
      var result = new List<Type>();
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (var assemly in assemblies)
      {
        var messages = assemly.GetTypes().Where<Type>((t) => t.GetCustomAttribute<EBNetID>() != null && !t.IsAbstract && t.IsSubclassOf(typeof(EBNetBase.Message))).ToArray();
        result.AddRange(messages);
      }

      foreach (var msg in result)
      {
        msgIDs.Add(new Tuple<Type, uint>(msg, msg.GetCustomAttribute<EBNetID>().MessageID));
      }
    }

    public uint GetTypeID(Type type)
    {
      return msgIDs.Single(entry => entry.Item1 == type).Item2;
    }

    public Type GetTypeByID(uint id)
    {
      return msgIDs.Single(entry => entry.Item2 == id).Item1;
    }
  }
}
