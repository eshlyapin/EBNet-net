using EBNet_net;
using ProtoBuf;

namespace EBNetTest
{
  [EBNetID(1)]
  [ProtoContract]
  public class LoginRequest : Message<LoginRequest>
  {
    [ProtoMember(1)]
    public string Username { get; set; }

    [ProtoMember(2)]
    public string Password { get; set; }
  }

  [EBNetID(3)]
  [ProtoContract]
  public class LoginResponce : Message<LoginResponce>
  {
    [ProtoMember(1)]
    public bool Status { get; set; }
  }

  [EBNetID(2)]
  [ProtoContract]
  public class EndOfTest : Message<EndOfTest>
  {
    [ProtoMember(1)]
    public bool ok { get; set; }
  }
}
