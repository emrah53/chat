using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static List<TcpClient> Clients = new List<TcpClient>();
        static Timer SendClientUpdateTimer;
        static List<string> Username = new List<string>();

        static void Main(string[] args)
        {
            var tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();

            SendClientUpdateTimer = new Timer(SendClientUpdate, null, 0, 5000);

            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                Clients.Add(client);

                var thread = new Thread(()=>HandleClient(client));
                thread.Start();

                // Console.WriteLine("Verbundn: {0}", client.Client.RemoteEndPoint);
                Console.WriteLine("Es sind {0} Teilnehmer online", Clients.Count);
            }
        }

        private static void SendClientUpdate(object state)
        {
            string message = string.Format("userCount|{0} USER", Clients.Count);
            var byteMessage = Encoding.UTF8.GetBytes(message);
            Broadcast(byteMessage);
        }

        private static void Broadcast(byte[] buffer)
        {
            Parallel.ForEach(Clients, (otherClient) =>
            {
                var otherStream = otherClient.GetStream();
                otherStream.Write(buffer, 0, buffer.Length);
            });
        }

        static void HandleClient(TcpClient client)
        {
            while(true)
            {
                var stream = client.GetStream();

                var buffer = new byte[1024];
                var byteCount = stream.Read(buffer, 0, buffer.Length);

                if (byteCount > 0)
                {
                    // Broadcast (parallel) Anfang
                    Parallel.ForEach(Clients, (otherClient) =>
                    {
                        var otherStream = otherClient.GetStream();
                        otherStream.Write(buffer, 0, buffer.Length);
                    });
                    // Broadcast (parallel) Ende

                    // Broadcast Anfang
                    //foreach ( var otherClient in Clients)
                    //{
                    //    var otherStream = otherClient.GetStream();
                    //    otherStream.Write(buffer, 0, buffer.Length);
                    //}
                    // Broadcast Ende

                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    // Console.WriteLine(data);
                }
            }
        }

    }
}
