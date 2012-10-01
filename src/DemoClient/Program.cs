using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using com.aphyr.riemann;

namespace DemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = "localhost";
            var port = 5555;

            var message = new Msg
            {
                states = new State[]{
                    new State{Host = "c#", Service = "Demo Client (1)", state = "ok", metric_f = 34.5f},
                    new State{Host = "c#", Service = "Demo Client (2)", state = "smouldering!", metric_f = 34.5f}
                },
                query = new Query { QueryString = "service =~ \"Demo%\"" }
            };

            try
            {
                TcpClient client = new TcpClient(host, port);

                Console.WriteLine("Connected");

                var stream = client.GetStream();
                ProtoBuf.Serializer.SerializeWithLengthPrefix<Msg>(stream, message, ProtoBuf.PrefixStyle.Fixed32BigEndian);


                // Receive the TcpServer.response.
                var response = ProtoBuf.Serializer.DeserializeWithLengthPrefix<Msg>(stream, ProtoBuf.PrefixStyle.Fixed32BigEndian);
                var events = (response.events ?? new Event[0]);
                var states = (response.states ?? new State[0]);

                Console.WriteLine("Received:\n\tOK:{0}\n\tMSG:{1}\n\t{2} events\t{3} states", response.ok, (response.error ?? "<null>"), 
                    events.Count(), states.Count());
                foreach (var e in events)
                {
                    Console.WriteLine("Host:{0}|Service:{1}|Metric:{2}|State:{3}",
                        e.Host, e.Service, e.metric_f, e.State);
                }
                // Close everything.
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
