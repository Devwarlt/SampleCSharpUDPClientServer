using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class Client
    {
        private UdpClient _client { get; set; }
        private IPEndPoint _broadcast { get; set; }

        public Client()
        {
            _client = new UdpClient(7171);
            _broadcast = _client.Client.LocalEndPoint as IPEndPoint;
        }

        public void Send(string data)
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(data);

                _client.Send(buffer, buffer.Length);

                Console.WriteLine($"Sent data '{data}' to server!");
            }
            catch { Console.WriteLine("Something went wrong with 'Client.Send' data!"); }
        }

        public void Receive()
        {
            try
            {
                var broadcast = _broadcast;
                var buffer = _client.Receive(ref broadcast);
                var data = Encoding.UTF8.GetString(buffer);

                Console.WriteLine($"Received data '{data}' from server!");
            }
            catch { Console.WriteLine($"Something went wrong with 'Client.Receive' data!"); }

            Thread.Sleep(100);

            Receive();
        }

        public void Connect()
        {
            try
            {
                _client.Connect("127.0.0.1", 7171);

                Console.WriteLine("Connected!");

                Receive();
            }
            catch
            {
                Console.WriteLine("Connection lost! Reconnecting to the server...");

                Thread.Sleep(1000);

                Connect();
            }
        }

        public void Disconnect()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
