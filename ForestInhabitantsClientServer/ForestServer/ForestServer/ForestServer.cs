using System;
using System.Collections;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ForestServer.ForestObjects;
using Newtonsoft.Json;
using NServiceBus;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace ForestServer
{
    public class ForestServer
    {
        private List<Tuple<string,Settings>> settings = new List<Tuple<string, Settings>>();
        private readonly List<Player> players = new List<Player>();
        private readonly List<Socket> visualisators = new List<Socket>();       
        private readonly List<Thread> games = new List<Thread>();

        public ForestServer(GameMode mode)
        {
            if (mode == GameMode.Tournament)
            {
                for (int i = 1; i <= 2; i++)
                {
                    var settingsPath = string.Format(@"settings_{0}", DateTime.Now);
                    var rnd = new Random();
                    XmlLoader.SaveData(settingsPath,
                        LoadSettings(string.Format(@"Maps\Maze{0}.txt", i), 2, rnd.Next(0,3)));
                    settings.Add(Tuple.Create(settingsPath,XmlLoader.DeserializeSettings(settingsPath)));
                    Thread.Sleep(10);
                }
            }
        }

        public void Start(string ipAddr,int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ipAddr), port);
            var listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(10);
            Console.WriteLine("Прослушиваю порт {0}:",port);
            WaitConnections(listener);
            for (int i=0;i<settings.Count;i++)
            {
                var game = new Game(true,settings[i].Item2, visualisators[i], players[i],players[i+1]);
                games.Add(new Thread(game.Start));
            }
            foreach (var game in games)
                game.Start();     
            while (games.Any(worker => worker.IsAlive)) { }
        }      

        private void WaitConnections(Socket listener)
        {
            while (visualisators.Count != 2 || players.Count != 4)
            {
                var buffer = new byte[128];
                var socket = listener.Accept();
                if (socket.Connected)
                {
                    socket.Receive(buffer);
                    var input = Encoding.UTF8.GetString(buffer).Replace("\0", "").Split(' ');
                    if (input[0].ToLower().Equals("player"))
                    {
                        Console.WriteLine("Игрок {0} подключился",input[1]);
                        players.Add(new Player(input[1],socket));
                    }
                    else if (input[0].ToLower().Equals("visualisator"))
                    {
                        Console.WriteLine("Визуализатор подключился");
                        visualisators.Add(socket);
                    }
                }
            }
        }

        private void CreateInhabitantsOnTheRandomPlaces(Settings options)
        {
            var rnd = new Random();
            for (var i = 0; i < options.CountOfPlayers; i++)
            {
                var canEnterObjects = new List<ForestObject>();
                foreach (var rowForestObjects in options.Forest.Map)
                    canEnterObjects.AddRange(rowForestObjects.Where(forestObject => forestObject.CanMove));
                var randomObject = canEnterObjects[rnd.Next(canEnterObjects.Count)];
                canEnterObjects.Remove(randomObject);
                var purpose = canEnterObjects[rnd.Next(canEnterObjects.Count)];
                canEnterObjects.Add(randomObject);
                var inhabitant = new Inhabitant(RandomStringGenerator(rnd.Next(4, 8)), 5);
                options.Forest.CreateInhabitant(ref inhabitant, randomObject.Place, purpose.Place);
            }
        }


        private Settings LoadSettings(string forestPath,int playersCount, int visible)
        {
            var options = new Settings
            {
                Forest = new ForestLoader(new StreamReader(forestPath)).Load(),
                CountOfPlayers = playersCount, 
                Visible = visible
            };
            CreateInhabitantsOnTheRandomPlaces(options);
            return options;
        }

        private static string RandomStringGenerator(int length)
        {
            var rng = RandomNumberGenerator.Create();
            var chars = new char[length];
            var validChars = "abcdefghijklmnopqrstuvwxyzABCEDFGHIJKLMNOPQRSTUVWXYZ1234567890";
            for (var i = 0; i < length; i++)
            {
                var bytes = new byte[1];
                rng.GetBytes(bytes);
                var rnd = new Random(bytes[0]);
                chars[i] = validChars[rnd.Next(0, 61)];
            }
            return (new string(chars));
        }
    }
}
