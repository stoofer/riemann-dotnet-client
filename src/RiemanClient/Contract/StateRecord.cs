using ProtoBuf;

namespace RiemanClient.Contract
{
    [ProtoContract()]
    public class StateEntry 
    {
        [ProtoMember(1)]
        public long? Time{get;set;}
  
        [ProtoMember(2)]
        public string State {get;set;}

        [ProtoMember(3)]
        public string Service{get; set;}

        [ProtoMember(4)]
        public string Host{get;set;}
        
        [ProtoMember(5)]
        public string Description{get;set;}

        [ProtoMember(6)]
        public bool? Once{get;set;}

        [ProtoMember(7)]
        public string[] Tags{get;set;}

        [ProtoMember(8)]
        public float? TTL{get;set;}
  
        [ProtoMember(15)]
        public float? Metric{get;set;}
    }
}