using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiemannClient.Contract;

namespace RiemannClient
{
    public class CompositeClient : IDisposable
    {
        private readonly RiemannTcpClient tcpClient;
        private readonly RiemannUdpClient udpClient;
        public const long MaxDatagramSize = 16384;

        public CompositeClient(string hostname = "localhost", int port = 5555, long maxDatagramSize = MaxDatagramSize)
        {
            tcpClient = new RiemannTcpClient(hostname,port);
            udpClient = new RiemannUdpClient(hostname,port, maxDatagramSize);
        } 

        public void Dispose()
        {
            tcpClient.Dispose(); 
            udpClient.Dispose();
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
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
        }

        public async Task SendAsync(params StateEntry[] states)
        {
            var buffer = states.ToMessage().Serialize();

            if (udpClient.CanSend(buffer))
                await udpClient.SendAsync(buffer);
            else
                await tcpClient.SendAndReceiveAsync(buffer);
        }

        public async Task<IEnumerable<EventRecord>> QueryAsync(string query)
        {
            var buffer = query.ToMessage().Serialize();
            var response = await tcpClient.SendAndReceiveAsync(buffer);
            return response.Events ?? new EventRecord[0];
        }
    }
}