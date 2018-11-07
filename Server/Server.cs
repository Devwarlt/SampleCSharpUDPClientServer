using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ServerException : Exception
    {
        public ServerException(int port) : base($"You can only run one server instance with port '{port}' per time.")
        {
        }
    }

    public class Server
    {
        private const int CLIENT_PORT = 7172;
        private const int SERVER_PORT = 7272;

        private UdpClient _server { get; set; }
        private ConcurrentDictionary<string, Client> _clients { get; set; }
        private Thread _listen { get; set; }
        private bool _disposing { get; set; }

        public Server()
        {
            try
            { _server = new UdpClient(SERVER_PORT); }
            catch { throw new ServerException(SERVER_PORT); }
            _clients = new ConcurrentDictionary<string, Client>();
            _listen = new Thread(Listen) { IsBackground = true };
        }

        public void Start()
        {
            _listen.Start();

            Console.WriteLine($"Server is listening on port {SERVER_PORT}...");
        }

        public void Stop()
        {
            _disposing = true;
            _listen.Abort();
            _server.Close();
            _server.Dispose();

            var amount = _clients.Keys.Count;

            if (amount == 0)
                Console.WriteLine("There is no client in the server network to disconnect!");
            else
            {
                Console.WriteLine($"Preparing to disconnect {amount} client{(amount > 1 ? "s" : "")} from the server!");

                _clients.Values.ToList().Select(client =>
                {
                    _clients.TryRemove(client.Remote, out Client disposedClient);

                    Console.WriteLine($"Client {disposedClient.Id} has left from the server network!");

                    return disposedClient;
                }).ToList();
                _clients.Clear();

                Console.WriteLine("All clients have been removed!");
            }
        }

        private void Listen()
        {
            try
            { _server.BeginReceive(OnListen, _server); }
            catch { Console.WriteLine("Something went wrong with 'Server.Listen' data!"); }

            Thread.Sleep(3000);

            Listen();
        }

        private void OnListen(IAsyncResult result)
        {
            if (_disposing)
                return;

            var client = (UdpClient)result.AsyncState;
            var broadcast = new IPEndPoint(IPAddress.Any, CLIENT_PORT);

            try
            {
                var buffer = client.EndReceive(result, ref broadcast);
                var data = Encoding.UTF8.GetString(buffer);
                var remote = $"{broadcast.Address}:{broadcast.Port}";

                if (_clients.ContainsKey(remote))
                    _clients[remote].Handle(data);
                else
                {
                    if (_clients.TryAdd(remote, new Client()
                    {
                        Id = Interlocked.Increment(ref Client.LastId),
                        Server = _server,
                        Remote = remote,
                        ClientIp = broadcast.Address.ToString(),
                        ClientPort = broadcast.Port
                    }))
                        Console.WriteLine($"New client connected!");
                    else
                        Console.WriteLine("Something went wrong with 'Server.Listen._clients' data!");
                }

                Console.WriteLine($"Received data '{data}' from client!");
            }
            catch { Console.WriteLine($"Something went wrong with 'Server.Listen' data!"); }

            client?.BeginReceive(OnListen, result.AsyncState);
        }
    }

    public class Client
    {
        public static int LastId = 0;

        public int Id { get; set; }
        public UdpClient Server { get; set; }
        public string Remote { get; set; }
        public string ClientIp { get; set; }
        public int ClientPort { get; set; }

        public void Handle(string data)
        {
            try
            {
                Console.WriteLine($"Received data '{data}' from client {Id}!");

                Send((new Random().NextDouble() * 1000).ToString());
            }
            catch { Console.WriteLine("Something went wrong with 'Client.Handle' data!"); }
        }

        public void Send(string data)
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(data);

                Server.Send(buffer, buffer.Length, ClientIp, ClientPort);

                Console.WriteLine($"Sent data '{data}' to client {Id}!");
            }
            catch (Exception e)
            {
                if (e is SocketException || e is InvalidOperationException)
                {
                    Console.WriteLine($"Client {Id} is offline!");

                    Connect();

                    return;
                }

                Console.WriteLine($"Something went wrong with 'Client.Send' data! {e}");
            }
        }

        private void Connect()
        {
            try
            { Server.Client.Connect("127.0.0.1", ClientPort); }
            catch
            {
                Console.WriteLine($"Client {Id} is offline! Retrying...");

                Thread.Sleep(1000);

                Connect();
            }
        }
    }
}