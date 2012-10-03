using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ProtoBuf;
using RiemanClient.Contract;

namespace RiemanClient
{
    public sealed class RiemannTcpClient : IDisposable
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

        public void Send(
            string host = null, 
            float? metric = null, 
            string service = null, 
            string state = "ok", 
            string description = null,
            float? ttl = null,
            IEnumerable<string> tags = null,
            DateTime? timestamp = null)
        {
            Send(new StateEntry
            {
                Host = host,
                Service = service,
                State = state,
                Metric = metric,
                Description = description,
                TTL = ttl,
                Tags = (tags ?? new string[0]).ToArray(),
                Time = timestamp.ToUnixEpochSeconds()
            });
        }

        public void Send(params StateEntry[] states)
        {
            var message = new Message {States = states};
            //TODO - what if it fails....?
            var _ = SendAndReceive(message);
        }

        public IEnumerable<EventRecord> Query(string query)
        {
            var message = new Message
                              {
                                  States = new []{new StateEntry{Description = "test query", Service = "tests", State = "info"} },
                                  Query = new Query{QueryString = query}
                              };

            var repsonse = SendAndReceive(message);
            return repsonse.Events ?? new EventRecord[0];
        }

        //TODO - make async.
        private Message SendAndReceive(Message message)
        {
            var tcpStream = Connect();
            Serializer.SerializeWithLengthPrefix(tcpStream, message, PrefixStyle.Fixed32BigEndian);

            var response = Serializer.DeserializeWithLengthPrefix<Message>(tcpStream, PrefixStyle.Fixed32BigEndian);

            //TODO - test cases for sad path.
            
            return response;
        }

        private NetworkStream Connect()
        {
            client = client ?? new TcpClient(hostname, port);
            return client.GetStream();
        }
    }
}