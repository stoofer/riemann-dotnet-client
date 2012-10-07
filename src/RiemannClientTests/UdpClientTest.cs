using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RiemannClient;
using RiemannClient.Contract;

namespace RiemannClientTests
{
    [TestFixture]
    public class UdpClientTest
    {
        [Test]
        public void Publishing_state_is_written_to_riemann_index()
        {
            //TODO - tidy this up!
            var serviceName = "UdpClientTest#" + Guid.NewGuid();
            var description = Guid.NewGuid().ToString();
            var metric = DateTime.Now.Ticks;
            const float timeToLive = 110.5f;
            const string host = "udp_tests";
            var tags = new[] {"tag1", "tag2"};

            IEnumerable<EventRecord> results;

            using (var client = new RiemannUdpClient())
            {
                client.Send(timestamp: DateTime.UtcNow,
                            host: host,
                            service: serviceName,
                            state: "ok",
                            ttl: timeToLive,
                            tags: tags,
                            metric: metric,
                            description: description);

                Thread.Sleep(1000);
            }

            using (var client = new RiemannTcpClient())
            {
                results = client.Query(string.Format("service=\"{0}\"", serviceName)).ToList();
            }

            Assert.That(results.Any());
            Assert.That(results.Single().Host, Is.EqualTo(host));
            Assert.That(results.Single().Description, Is.EqualTo(description));
            Assert.That(results.Single().Service, Is.EqualTo(serviceName));
            Assert.That(results.Single().State, Is.EqualTo("ok"));
            Assert.That(results.Single().Tags, Is.EquivalentTo(tags));
            Assert.That(results.Single().metric_f, Is.EqualTo(metric));
            Assert.That(results.Single().TTL, Is.EqualTo(timeToLive));
        }

        
        [Test]
        public void Publishing_state_supports_asynch_mode()
        {
            var host = Guid.NewGuid().ToString();
            const string serviceNameRoot = "UdpClientAsync#";
            Guid.NewGuid().ToString();
            
            IEnumerable<EventRecord> results;

            var states = from idx in Enumerable.Range(1, 100)
                         let state =
                             new StateEntry(host: host,
                                            service: serviceNameRoot + idx,
                                            state: "ok", metric: idx,
                                            timeToLiveInSeconds: 10)
                         group state by idx%10
                         into batches
                         select batches;


            var udpClient = new RiemannUdpClient();
            var tasks = states.Select(s => udpClient.SendAsync(s.ToArray())).ToArray();
            Task.WaitAll(tasks);
            Thread.Sleep(2000);
            using (var tcpClient = new RiemannTcpClient())
            {
                var query = string.Format("service=~ \"{0}%\"", serviceNameRoot);
                results = tcpClient.QueryAsync(query).Result.ToList();
            }
            Assert.That(results.Count(), Is.EqualTo(100));
           
        }

        [Test]
        public void Publishing_messages_over_threshold_is_forbidden()
        {
            using (var client = new RiemannUdpClient())
            {
                Assert.Throws<InvalidOperationException>(() => 
                    client.Send(description: String.Join("-",Enumerable.Repeat("123456789", 1638))));
            }
        }
    }
}