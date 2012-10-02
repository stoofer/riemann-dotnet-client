using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using com.aphyr.riemann;

namespace RiemanClientSpike
{
    public class RiemannTcpClient : IDisposable
    {
        private readonly TcpClient client;

        public RiemannTcpClient(string host = "localhost", int port = 5555)
        {
            //TODO - add a connect method instead?
            client = new TcpClient(host, port);
        }

        public void Dispose()
        {
            client.Close();
        }

        public void SendEvent(string host = null, float? metric = null, string service = null, string state = "ok", string description = null)
        {
            var stateEntry = new State
                                 {
                                     Host = host,
                                     Service = service,
                                     state = state,
                                     metric_f = metric,
                                     Description = description
                                 };

            var message = new Msg{states =new[]{stateEntry}};
            var tcpStream = client.GetStream();
            Serializer.SerializeWithLengthPrefix(tcpStream, message, PrefixStyle.Fixed32BigEndian);
            
            //TODO - test cases for sad path.
            var result = Serializer.DeserializeWithLengthPrefix<Msg>(tcpStream, PrefixStyle.Fixed32BigEndian);
        }

        public IEnumerable<Event> Query(string query)
        {
            var message = new Msg
                              {
                                  states = new []{new State{Description = "test query", Service = "tests", state = "info"}, },
                                  query = new Query{QueryString = query}
                              };

            var tcpStream = client.GetStream();
            Serializer.SerializeWithLengthPrefix(tcpStream, message, PrefixStyle.Fixed32BigEndian);
            var result = Serializer.DeserializeWithLengthPrefix<Msg>(tcpStream, PrefixStyle.Fixed32BigEndian);

            return result.events ?? new Event[0];
        }
    }
}
