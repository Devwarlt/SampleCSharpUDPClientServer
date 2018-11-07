using System;
using System.Threading;

namespace Server
{
    public class Program
    {
        private static Server _server { get; set; }

        public static void Main(string[] args)
        {
            Console.Title = "(Server) UDP Sample";

            _server = new Server();
            _server.Start();

            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                ;

            Console.WriteLine("Closing server...");

            _server.Stop();

            Thread.Sleep(1000);

            Environment.Exit(0);
        }
    }
}