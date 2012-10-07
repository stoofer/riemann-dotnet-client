using ProtoBuf;

namespace RiemannClient.Contract
{
    [ProtoContract]
    public class Query 
    {
        public Query(string query)
        {
            QueryString = query;
        }

        [ProtoMember(1)]
        public string QueryString{get;set;}
    }
}