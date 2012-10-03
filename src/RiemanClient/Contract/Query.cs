using ProtoBuf;

namespace RiemanClient.Contract
{
    [ProtoContract]
    public class Query 
    {
        public Query()
        {}

        public Query(string query)
        {
            QueryString = query;
        }

        [ProtoMember(1)]
        public string QueryString{get;set;}
    }
}