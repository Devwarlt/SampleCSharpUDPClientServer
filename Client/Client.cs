using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class ClientException : Exception
    {
        public ClientException(int port) : base($"You can only run one client instance with port '{port}' per time.")
        {
        }
    }

    public class Client
    {
        private const int CLIENT_PORT = 7172;
        private const int SERVER_PORT = 7272;
        private const string SERVER_IP = "127.0.0.1";

        private UdpClient _client { get; set; }
        private Thread _listen { get; set; }
        private bool _disposing { get; set; }

        public Client()
        {
            try
            { _client = new UdpClient(CLIENT_PORT); }
            catch { throw new ClientException(CLIENT_PORT); }
            _listen = new Thread(Listen) { IsBackground = true };
        }

        public void Start()
        {
            _listen.Start();

            Console.WriteLine($"Client is listening on port {CLIENT_PORT}...");
        }

        public void Stop()
        {
            _disposing = true;
            _listen.Abort();
            _client.Close();
            _client.Dispose();

            Console.WriteLine("Client has left from the server network!");
        }

        public void Send(string data)
        {
            if (!_client.Client.Connected)
                return;

            try
            {
                var buffer = Encoding.UTF8.GetBytes(data);

                _client.Send(buffer, buffer.Length, SERVER_IP, SERVER_PORT);

                Console.WriteLine($"Sent data '{data}' to server!");
            }
            catch { Console.WriteLine($"Something went wrong with 'Client.Send' data!"); }
        }

        private void Listen()
        {
            if (!_client.Client.Connected)
            {
                Connect();
                return;
            }

            _client?.BeginReceive(OnListen, _client);

            Thread.Sleep(3000);

            Listen();
        }

        private void OnListen(IAsyncResult result)
        {
            if (_disposing)
                return;

            var server = (UdpClient)result.AsyncState;
            var broadcast = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

            try
            {
                var buffer = server.EndReceive(result, ref broadcast);
                var data = Encoding.UTF8.GetString(buffer);

                Handle(data);
            }
            catch
            {
                Connect();
                return;
            }

            server?.BeginReceive(OnListen, result.AsyncState);
        }

        private void Handle(string data) => Console.WriteLine($"Received data '{data}' from server!");

        private void Connect()
        {
            try
            {
                _client.Client.Connect("127.0.0.1", SERVER_PORT);

                Listen();
            }
            catch
            {
                Console.WriteLine("Server is offline! Retrying...");

                Thread.Sleep(1000);

                Connect();
            }

            Thread.Sleep(1000);

            Connect();
        }
    }
}