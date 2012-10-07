using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;
using RiemanClient;
using RiemanClient.Contract;

namespace RiemannClientTests
{
    //TODO - spin up riemann if not present. 
    [TestFixture]
    public class TcpClientTest
    {
        [Test]
        public void Publishing_state_is_written_to_riemann_index()
        {
            var serviceName = "TcpClientTest#" + Guid.NewGuid();
            var description = Guid.NewGuid().ToString();
            var metric = DateTime.Now.Ticks;
            const float timeToLive = 110.5f;
            const string host = "tests";
            var tags = new[] {"tag1", "tag2"};

            IEnumerable<EventRecord> results;

            using (var client = new RiemannTcpClient())
            {
                client.Send(tags: tags,
                                 host: host,
                                 service: serviceName,
                                 state: "ok",
                                 description: description,
                                 metric: metric,
                                 ttl: timeToLive);

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
        public void Should_convert_time_to_unix_epoch_seconds()
        {
            var time = DateTime.UtcNow;
            int expectedTimestamp = (int)(time - new DateTime(1970, 1, 1)).TotalSeconds;

            List<EventRecord> results;
            using (var client = new RiemannTcpClient())
            {
                const string serviceName = "Should_convert_time_to_unix_epoch_seconds";
                client.SendAsync(host: "tests",
                                 service: serviceName,
                                 state: "ok",
                                 timestamp: time).Wait();

                results = client.Query(string.Format("service=\"{0}\"", serviceName)).ToList();

            }
            Assert.That(results.Any());
            Assert.That(results.Single().Time, Is.EqualTo(expectedTimestamp));
        }

        [Test]
        public void Should_expose_connection_failures()
        {
            using (var client = new RiemannTcpClient(port: 9999, hostname: "monkey-trumpets"))
            {
                Assert.Throws<SocketException>(() => client.Query("anyone there..?"));
                Assert.Throws<SocketException>(() => client.Send(host: "broken"));
            }
        }

        [Test]
        public void Publishing_state_supports_asynch_mode()
        {
            var host = Guid.NewGuid().ToString();
            const string serviceNameRoot = "TcpClientAsync#";
            Guid.NewGuid().ToString();
            
            IEnumerable<EventRecord> results;

            var states = from idx in Enumerable.Range(1, 1000)
                         let state =
                             new StateEntry(host: host,
                                            service: serviceNameRoot + idx,
                                            state: "ok", metric: idx,
                                            timeToLiveInSeconds: 10)
                         group state by idx%10
                         into batches
                         select batches;
                        
            
            var tasks = states.Select(s => new RiemannTcpClient().SendAsync(s.ToArray())).ToArray();
            Task.WaitAll(tasks);
            using (var client = new RiemannTcpClient())
            {
                var query = string.Format("service=~ \"{0}%\"", serviceNameRoot);
                results = client.QueryAsync(query).Result.ToList();
            }
            Assert.That(results.Count(), Is.EqualTo(1000));
           
        }
    }
}
