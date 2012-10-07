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
    public sealed class RiemannUdpClient : IDisposable
    {
        private readonly string hostname;
        private readonly int port;
        private UdpClient client;
        private const long MaxDatagramSize = 16384;

        public RiemannUdpClient(string hostname = "localhost", int port = 5555)
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
        
        public async Task SendAsync(params StateEntry[] states)
        {
            await SendAsync(states.ToMessage());
        }



        private async Task SendAsync(Message message)
        {
            EnsureConnected();
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                var buffer = stream.GetBuffer();
                var length = (int) stream.Length;
                if(length > MaxDatagramSize)
                    throw new InvalidOperationException(string.Format("Cannot send messages grater than {0} bytes over UDP", MaxDatagramSize));
                await Task<int>.Factory.StartNew(() => client.Send(buffer, length, hostname, port));
            }
        }

        private void EnsureConnected()
        {
            if (client != null)
                return;
            client =  new UdpClient(port);
        }

    }
}