using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CSharpHelpers
{
    public class TcpClientForwarder
    {
        private readonly object _streamLock = new object();
        private Stream _networkStream;

        private const int ReceiveTimeout = 20;
        private const int SendTimeout = 20;

        private TcpClient Source { get; set; }
        private TcpClientForwarder Destination { get; set; }

        public byte[] Buffer { get; private set; }
        public int Read { get; private set; }

        public bool IsSSL { get; set; }
        public string SslCertificateServerName { get; set; }

        public TcpClientForwarder(TcpClient source)
        {
            this.Source = source;
            this.Source.ReceiveTimeout = ReceiveTimeout;
            this.Source.SendTimeout = SendTimeout;

            this.Buffer = new byte[1024];
            this.Read = 0;
        }

        /// <summary>
        /// The destination to forward data
        /// </summary>
        /// <param name="destination"></param>
        public void Forward(TcpClient destination)
        {
            this.Destination = new TcpClientForwarder(destination);
        }

        /// <summary>
        /// The SSL destination to forward data
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sslCertificateServerName">The server name must match the name on the server certificate</param>
        public void Forward(TcpClient destination, string sslCertificateServerName)
        {
            this.Destination = new TcpClientForwarder(destination) {IsSSL = true, SslCertificateServerName = sslCertificateServerName};
        }

        /// <summary>
        /// Starts transferring data. if the forwarding destination is not set, it will echo back to the source
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            if (this.Destination == null)
                this.Destination = new TcpClientForwarder(this.Source);

            this.Destination.Forward(this.Source);

            var tasks = new Task[2];
            tasks[0] = Task.Run(() => this.Destination.Receive());
            tasks[1] = Task.Run(() => this.Receive());

            Task.WaitAny(tasks, 2000);
        }

        private Stream GetStream()
        {
            lock (_streamLock)
            {
                if (_networkStream != null)
                    return _networkStream;

                if (!this.IsSSL)
                {
                    _networkStream = this.Source.GetStream();
                }
                else
                {
                    var sslStream = new SslStream(this.Source.GetStream());

                    try
                    {
                        sslStream.AuthenticateAsClient(this.SslCertificateServerName);
                        _networkStream = sslStream;
                    }
                    catch (AuthenticationException ex)
                    {
                        this.Source.Close();
                        throw new ApplicationException("SSL Authentication failed - closing the connection.", ex);
                    }
                }
            }

            _networkStream.ReadTimeout = ReceiveTimeout;
            return _networkStream;
        }

        async private Task Send()
        {
            var stream = this.Destination.GetStream();
            await stream.WriteAsync(this.Buffer, 0, this.Read);
        }

        async private Task Receive()
        {
            var stream = this.GetStream();

            while ((this.Read = await stream.ReadAsync(this.Buffer, 0, this.Buffer.Length)) > 0)
                await this.Send();
        }
    }
}
