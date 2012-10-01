using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
namespace com.aphyr.riemann
{
    //option java_package = "com.aphyr.riemann";
    //option java_outer_classname = "Proto";

    [ProtoContract()]
    public class State 
    {
        [ProtoMember(1)]
        public long? Time{get;set;}
  
        [ProtoMember(2)]
        public string state {get;set;}

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
        public float? metric_f{get;set;}
    }

    [ProtoContract]
    public class Event 
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

        [ProtoMember(7)]
        public string[] Tags{get;set;}

        [ProtoMember(8)]
        public float? TTL{get;set;}
  
        [ProtoMember(15)]
        public float? metric_f{get;set;}
    }

    [ProtoContract]
    public class Query 
    {
        [ProtoMember(1)]
        public string QueryString{get;set;}
    }

    [ProtoContract]
    public class Msg {
      
        [ProtoMember(2)]
        public bool? ok{get;set;}

        [ProtoMember(3)]
        public string error{get;set;}
      
        [ProtoMember(4)]
        public State[] states{get;set;}

        [ProtoMember(5)]
        public Query query{get;set;}

        [ProtoMember(6)]
        public Event[] events { get; set; }
    }
}
