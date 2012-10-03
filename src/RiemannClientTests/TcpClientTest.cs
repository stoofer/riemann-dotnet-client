using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using NUnit.Framework;
using RiemanClientSpike;
using com.aphyr.riemann;

namespace RiemannClientTests
{
    //TODO - spin up riemann if not present. 
    [TestFixture]
    public class TcpClientTest
    {
        [Test]
        public void Publishing_state_is_written_to_riemann_index()
        {
            var serviceName = "TcpClientTest " + Guid.NewGuid();
            var description = Guid.NewGuid().ToString();
            var metric = DateTime.Now.Ticks;
            const float timeToLive = 110.5f;
            const string host = "tests";
            var tags = new[] {"tag1", "tag2"};

            IEnumerable<Event> results;

            using (var client = new RiemannTcpClient())
            {
                client.SendEvent(tags: tags,
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

            List<Event> results;
            using (var client = new RiemannTcpClient())
            {
                const string serviceName = "Should_convert_time_to_unix_epoch_seconds";
                client.SendEvent(host: "tests",
                                 service: serviceName,
                                 state: "ok",
                                 timestamp: time);

                results = client.Query(string.Format("service=\"{0}\"", serviceName)).ToList();

            }
            Assert.That(results.Any());
            Assert.That(results.Single().Time, Is.EqualTo(expectedTimestamp));
            //TODO- now provide a nicer API...perhaps with some Noda?
        }

        [Test]
        public void Should_expose_connection_failures()
        {
            using (var client = new RiemannTcpClient(port: 9999, hostname: "monkey-trumpets"))
            {
                Assert.Throws<SocketException>(() => client.Query("anyone there..?"));
            }
        }
    }
}
