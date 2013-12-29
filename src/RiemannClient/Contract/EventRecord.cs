using ProtoBuf;
using System;

namespace RiemannClient.Contract
{
    [ProtoContract]
    public class EventRecord 
    {
        public EventRecord()
        {

        }

        public EventRecord(DateTime? timestamp = null, 
            string state = null, 
            string service = null, 
            string host = null, 
            string description = null, 
            string[] tags = null, 
            float? timeToLiveInSeconds = null,
            float? metric = null)
        {
            Time = timestamp.ToUnixEpochSeconds();
            State = state;
            Service = service;
            Host = host;
            Description = description;
            Tags = tags;
            TTL = timeToLiveInSeconds;
            metric_f = metric;
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

        [ProtoMember(7)]
        public string[] Tags{get;set;}

        [ProtoMember(8)]
        public float? TTL{get;set;}
  
        [ProtoMember(15)]
        public float? metric_f{get;set;}
    }
}