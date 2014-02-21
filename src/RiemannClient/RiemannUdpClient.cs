using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RiemannClient
{
    public sealed class RiemannUdpClient : IDisposable
    {
        private readonly string hostname;
        private readonly int port;
        private readonly long maxDatagramSize;
        private UdpClient client;

        public RiemannUdpClient(string hostname = "localhost", int port = 5555, long maxDatagramSize = CompositeClient.MaxDatagramSize)
        {
            this.hostname = hostname;
            this.port = port;
            this.maxDatagramSize = maxDatagramSize;
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

        public async Task SendAsync(byte[] buffer)
        {
            EnsureConnected();
            if (!CanSend(buffer))
                throw new InvalidOperationException(string.Format("Cannot send messages greater than {0} bytes over UDP",
                                                                  CompositeClient.MaxDatagramSize));
            await Task<int>.Factory.StartNew(() => client.Send(buffer, buffer.Length, hostname, port));
        }

        public bool CanSend(byte[] buffer)
        {
            return buffer.Length <= maxDatagramSize;
        }


        private void EnsureConnected()
        {
            if (client != null)
                return;
            client =  new UdpClient(port);
        }

    }
}