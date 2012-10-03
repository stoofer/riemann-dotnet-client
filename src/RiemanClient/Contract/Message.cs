using ProtoBuf;

namespace RiemanClient.Contract
{
    [ProtoContract]
    public class Message {
      
        [ProtoMember(2)]
        public bool? Ok{get;set;}

        [ProtoMember(3)]
        public string Error{get;set;}
      
        [ProtoMember(4)]
        public StateEntry[] States{get;set;}

        [ProtoMember(5)]
        public Query Query{get;set;}

        [ProtoMember(6)]
        public EventRecord[] Events { get; set; }
    }
}
