using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using com.aphyr.riemann;

namespace RiemanClientSpike
{
    public class RiemannTcpClient : IDisposable
    {
        private readonly string hostname;
        private readonly int port;
        private TcpClient client;

        public RiemannTcpClient(string hostname = "localhost", int port = 5555)
        {
            this.hostname = hostname;
            this.port = port;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (client == null) return;

            client.Close();
            client = null;
        }

        public void SendEvent(
            string host = null, 
            float? metric = null, 
            string service = null, 
            string state = "ok", 
            string description = null,
            float? ttl = null,
            IEnumerable<string> tags = null,
            DateTime? timestamp = null)
        {
            var stateEntry = new State
                                 {
                                     Host = host,
                                     Service = service,
                                     state = state,
                                     metric_f = metric,
                                     Description = description,
                                     TTL = ttl,
                                     Tags = (tags ?? new string[0]).ToArray(),
                                     Time = timestamp.ToUnixEpochSeconds()
                                 };

            var message = new Msg{states =new[]{stateEntry}};
            
            //TODO - what if it fails....?
            var _ = SendAndReceive(message);
        }

        public IEnumerable<Event> Query(string query)
        {
            var message = new Msg
            {
                states = new []{new State{Description = "test query", Service = "tests", state = "info"}, },
                query = new Query{QueryString = query}
            };

            var repsonse = SendAndReceive(message);
            return repsonse.events ?? new Event[0];
        }

        //TODO - make async.
        private Msg SendAndReceive(Msg message)
        {
            var tcpStream = Connect();
            Serializer.SerializeWithLengthPrefix(tcpStream, message, PrefixStyle.Fixed32BigEndian);

            var response = Serializer.DeserializeWithLengthPrefix<Msg>(tcpStream, PrefixStyle.Fixed32BigEndian);

            //TODO - test cases for sad path.
            
            return response;
        }

        public IObservable<Exception> Errors { get; private set; }
 
        private NetworkStream Connect()
        {
            client = client ?? new TcpClient(hostname, port);
            return client.GetStream();
        }
    }

    public static class Conversions
    {
        public static long? ToUnixEpochSeconds(this DateTime? localDateTime)
        {
            if (!localDateTime.HasValue)
            {
                return null;
            }

            var timeSinceEpoch = (localDateTime.Value.ToUniversalTime() - new DateTime(1970, 1, 1));
            return (int)timeSinceEpoch.TotalSeconds;

        }
    }
}
