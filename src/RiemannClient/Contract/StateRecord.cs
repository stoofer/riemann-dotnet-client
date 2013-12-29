using System;
using ProtoBuf;

namespace RiemannClient.Contract
{
    [ProtoContract()]
    public class StateEntry 
    {
        public StateEntry(
            DateTime? timestamp = null, 
            string state = null, 
            string service = null, 
            string host = null, 
            string description = null, 
            string[] tags = null, 
            float? timeToLiveInSeconds = null)
        {
            Time = timestamp.ToUnixEpochSeconds();
            State = state;
            Service = service;
            Host = host;
            Description = description;
            Tags = tags;
            TTL = timeToLiveInSeconds;
        }

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
    }
}