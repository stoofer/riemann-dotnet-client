using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.aphyr.riemann;
namespace RiemannClientTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var host = "localhost";
            var port = 5555;

            var message = new Msg
            {
                query = new Query { QueryString = "todo" }
            };
                
            try
            {
                TcpClient client = new TcpClient(host, port);

                Console.WriteLine("Connected");

                var stream = client.GetStream();
                ProtoBuf.Serializer.SerializeWithLengthPrefix<Msg>(stream, message, ProtoBuf.PrefixStyle.Fixed32);


                // Receive the TcpServer.response.
                var response = ProtoBuf.Serializer.Deserialize<Msg>(stream);

                Console.WriteLine("Received: OK:{0}, MSG: {1}", response.ok, (response.error ?? "no error"));

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
