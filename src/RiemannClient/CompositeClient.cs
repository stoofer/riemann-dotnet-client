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

        public CompositeClient(string hostname = "localhost", int port = 5555, long maxDatagramSize = MaxDatagramSize, int connectTimeout = 0, int receiveTimeout = 0, int sendTimeout = 0)
        {
            tcpClient = new RiemannTcpClient(hostname,port, connectTimeout, receiveTimeout, sendTimeout);
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
            Send(new EventRecord(
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
            await SendAsync(new EventRecord(
                                host: host,
                                service: service,
                                state: state,
                                metric: metric,
                                description: description,
                                timeToLiveInSeconds: ttl,
                                tags: (tags ?? new string[0]).ToArray(),
                                timestamp: timestamp));
        }

        public void Send(params EventRecord[] events)
        {
            try
            {
                SendAsync(events).Wait();
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

        public async Task SendAsync(params EventRecord[] events)
        {
            var buffer = events.ToMessage().Serialize();

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