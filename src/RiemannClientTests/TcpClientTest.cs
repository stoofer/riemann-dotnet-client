using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiemanClientSpike;
using com.aphyr.riemann;

namespace RiemannClientTests
{
    [TestFixture]
    public class TcpClientTest
    {
        [Test]
        public void Publishing_state_is_written_to_riemann_index()
        {
            var serviceName = "TcpClientTest " + Guid.NewGuid();
            var description = Guid.NewGuid().ToString();
            var metric = DateTime.Now.Ticks;
            
//            (use 'riemann.client)
//            (def c (tcp-client :host "1.2.3.4"))
//            (send-event c {:service "foo" :state "ok"})
//            (query c "state = \"ok\"")

            IEnumerable<Event> results;
            
            using (var client = new RiemannTcpClient(host: "127.0.0.1", port: 5555))
            {
                client.SendEvent(host: "tests",
                                 service: serviceName,
                                 state: "ok",
                                 description: description,
                                 metric: metric);

                results = client.Query(string.Format("service=\"{0}\"", serviceName)).ToList();

            }
            Assert.That(results.Any());
            Assert.That(results.Single().Description, Is.EqualTo(description));
            Assert.That(results.Single().Service, Is.EqualTo(serviceName));
            Assert.That(results.Single().State, Is.EqualTo("ok"));
            Assert.That(results.Single().metric_f, Is.EqualTo(metric));
        }
    }
}
