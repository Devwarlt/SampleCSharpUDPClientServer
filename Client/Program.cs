using System;
using System.Threading;

namespace Client
{
    public class Program
    {
        private static Client _client { get; set; }

        public static void Main(string[] args)
        {
            Console.Title = "(Client) UDP Sample";

            try
            {
                _client = new Client();
                _client.Start();

                var sender = new Thread(() =>
                {
                    do
                    {
                        _client.Send((new Random().NextDouble() * 1000).ToString());

                        Thread.Sleep(1000);
                    } while (true);
                })
                { IsBackground = true };

                sender.Start();

                while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                    ;

                Console.WriteLine("Closing client...");

                sender.Abort();

                _client.Stop();

                Thread.Sleep(1000);

                Environment.Exit(0);
            }
            catch (ClientException)
            {
                Console.WriteLine("Press any key to exit.");

                Console.ReadKey();

                Environment.Exit(1);
            }
        }
    }
}