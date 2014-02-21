using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProtoBuf;
using RiemannClient.Contract;

namespace RiemannClient
{
    public sealed class RiemannTcpClient : IDisposable
    {
        private readonly string hostname;
        private readonly int port;
        private readonly int connectTimeout;
        private readonly int receiveTimeout;
        private readonly int sendTimeout;
        private TcpClient client;

        public RiemannTcpClient(string hostname = "localhost", int port = 5555, int connectTimeout = 0, int receiveTimeout = 0, int sendTimeout = 0)
        {
            this.hostname = hostname;
            this.port = port;
            this.connectTimeout = connectTimeout;
            this.sendTimeout = sendTimeout;
            this.receiveTimeout = receiveTimeout;
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


        public async Task<Message> SendAndReceiveAsync(byte[] message)
        {
            var tcpStream = await GetNetworkStream();
            await WriteRequest(message, tcpStream);
            var response = await ReadResponseAsync(tcpStream);
            return response;
        }

        private static async Task WriteRequest(byte[] message, Stream tcpStream)
        {
            var lengthBigEndian = BitConverter.GetBytes(message.Length);
            Array.Reverse(lengthBigEndian);
            tcpStream.Write(lengthBigEndian, 0, lengthBigEndian.Length);
            tcpStream.Write(message, 0, message.Length);
            await tcpStream.FlushAsync();
        }

        private static Task<Message> ReadResponseAsync(Stream tcpStream)
        {
            return Task.Factory.StartNew(() => Serializer.DeserializeWithLengthPrefix<Message>(tcpStream, PrefixStyle.Fixed32BigEndian));
        }

        private async Task<NetworkStream> GetNetworkStream()
        {
            if(client == null)
            {
                var c = new TcpClient();
                if (this.connectTimeout > 0)
                {
                    await Task.WhenAny(c.ConnectAsync(hostname, port), Task.Delay(this.connectTimeout));
                    if (!c.Connected)
                        throw new SocketException(10060);
                }
                else
                    await c.ConnectAsync(hostname, port);
                client = c;
                client.SendTimeout = this.sendTimeout;
                client.ReceiveTimeout = this.receiveTimeout;
            }
            return client.GetStream();
        }
    }
}