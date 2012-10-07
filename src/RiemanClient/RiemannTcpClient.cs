using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
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
            Send(new StateEntry(
                     host: host,
                     service: service,
                     state: state,
                     metric: metric,
                     description: description,
                     timeToLiveInSeconds: ttl,
                     tags: (tags ?? new string[0]).ToArray(),
                     timestamp: timestamp));
        }

        public async Task SendAsync(
            string host = null, 
            float? metric = null, 
            string service = null, 
            string state = "ok", 
            string description = null,
            float? ttl = null,
            IEnumerable<string> tags = null,
            DateTime? timestamp = null)
        {
            await SendAsync(new StateEntry(
                     host: host,
                     service: service,
                     state: state,
                     metric: metric,
                     description: description,
                     timeToLiveInSeconds: ttl,
                     tags: (tags ?? new string[0]).ToArray(),
                     timestamp: timestamp));
        }

        public void Send(params StateEntry[] states)
        {
            try
            {
                SendAsync(states).Wait();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
        }

        public IEnumerable<EventRecord> Query(string query)
        {
            try
            {
                return QueryAsync(query).Result;
            }
            catch( AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
        }

        public async Task SendAsync(params StateEntry[] states)
        {
            await SendAndReceiveAsync(states.ToMessage());
        }

        public async Task<IEnumerable<EventRecord>> QueryAsync(string query)
        {
            var response = await SendAndReceiveAsync(query.ToMessage());
            return response.Events ?? new EventRecord[0];
        }

        private async Task<Message> SendAndReceiveAsync(Message message)
        {
            var tcpStream = await GetNetworkStream();
            WriteRequest(message, tcpStream);
            var response = await ReadResponseAsync(tcpStream);
            return response;
        }

        private static void WriteRequest(Message message, NetworkStream tcpStream)
        {
            Serializer.SerializeWithLengthPrefix(tcpStream, message, PrefixStyle.Fixed32BigEndian);
        }

        private static Task<Message> ReadResponseAsync(Stream tcpStream)
        {
            return Task.Factory.StartNew(() => Serializer.DeserializeWithLengthPrefix<Message>(tcpStream, PrefixStyle.Fixed32BigEndian));
        }

        private async Task<NetworkStream> GetNetworkStream()
        {
            if(client == null)
            {
                var c = new TcpClient();
                await c.ConnectAsync(hostname, port);
                client = c;
            }
            return client.GetStream();
        }
    }
}