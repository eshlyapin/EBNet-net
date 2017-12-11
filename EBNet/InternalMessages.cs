using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNet
{

  [MessageTypeID(10)]
  [ProtoContract]
  class SetupSession : Message<SetupSession>
  {
    [ProtoMember(1)]
    public int SessionId { get; set; }

    [ProtoMember(2)]
    public string Address { get; set; }

    [ProtoMember(3)]
    public int port { get; set; }
  }

  [MessageTypeID(11)]
  [ProtoContract]
  class BadResponse : Message<BadResponse>
  {

  }
}
