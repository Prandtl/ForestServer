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
                BeforeStart("Alexander",socket, out inhabitant);
                AfterStart(socket,inhabitant);
            }
        }

        private static void BeforeStart(string name,Socket socket, out Inhabitant inhabitant)
        {
            var buffer = new byte[128];
            socket.Send(Encoding.UTF8.GetBytes(string.Format("player {0}",name)));
            socket.Receive(buffer);
            var input = Encoding.UTF8.GetString(buffer).Split(' ');
            var place = new Coordinates(int.Parse(input[1][2].ToString()), int.Parse(input[1][4].ToString()));
            inhabitant = new Inhabitant(name, int.Parse(input[4]))
            {
                Place = place,
                PrevObject = GetPrevObj(input[5],place),
                Purpose = new Coordinates(int.Parse(input[2][2].ToString()), int.Parse(input[2][4].ToString()))
            };
        }

        private static ForestObject GetPrevObj(string obj,Coordinates place)
        {
            switch (obj)
            {
                case "Trap":
                    return new Trap(place);
                case "Footpath":
                    return new Footpath(place);
            }
            return null;
        }

        private static void AfterStart(Socket socket,Inhabitant inhabitant)
        {
            var bot = new GameBot(inhabitant,socket);
            bot.TryReachThePurpose();
        }       
    }
}
