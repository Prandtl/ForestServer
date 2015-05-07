using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ClientPlayer.ForestObjects;
using Newtonsoft.Json;

namespace ClientPlayer
{
    class Program
    {
        //10.97.24.96
        static void Main(string[] args)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
            }
            catch (Exception)
            {
                throw new Exception("Не удалось подключиться к серверу!");
            }
            if (socket.Connected)
            {
                Inhabitant inhabitant;
                BeforeStart(socket, out inhabitant);
                AfterStart(socket,inhabitant);
            }
        }

        private static void BeforeStart(Socket socket, out Inhabitant inhabitant)
        {
            var buffer = new byte[4096];
            socket.Send(Encoding.UTF8.GetBytes("I am player"));
            var rnd = new Random();
            socket.Receive(buffer);
            var inhabitants = Encoding.UTF8.GetString(buffer).Split(new[] { ' ', '\0' }, StringSplitOptions.RemoveEmptyEntries);
            var myInhabitant = inhabitants[rnd.Next(0, inhabitants.Length)];
            socket.Send(Encoding.UTF8.GetBytes(myInhabitant));
            socket.Receive(buffer);
            var serInhabitant = Encoding.UTF8.GetString(buffer).Replace("\0", "");
            inhabitant = JsonConvert.DeserializeObject<Inhabitant>(serInhabitant, new ForestObjectConverter());
        }

        private static void AfterStart(Socket socket,Inhabitant inhabitant)
        {
            var bot = new GameBot(inhabitant,socket);
            bot.TryReachThePurpose();
        }       
    }
}
