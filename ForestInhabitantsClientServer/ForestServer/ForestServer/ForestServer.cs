using System;
using System.Collections;
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
        private Settings settings;
        private readonly List<Socket> players = new List<Socket>();
        private readonly List<Socket> visualisators = new List<Socket>();
        private readonly List<Inhabitant> reservedInhabitants = new List<Inhabitant>(); 
        private readonly List<Thread> bots = new List<Thread>();

        public ForestServer(string settingsPath)
        {
            //XmlLoader.SaveData(settingsPath, LoadSettings(@"Maps\Maze3.txt", 3, 3));
            settings = XmlLoader.DeserializeForest(settingsPath);
        }

        public void Start(string ipAddr,int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ipAddr), port);
            var listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(10);
            Console.WriteLine("Прослушиваю порт {0}:",port);
            WaitConnections(listener);
            SendForestToVisualisators();
            foreach (var player in players)
            {
                reservedInhabitants.Add(BeforeStart(player));
                bots.Add(new Thread(Worker));
            }
            for (var i = 0; i < reservedInhabitants.Count; i++)
                bots[i].Start(i);              
            while (bots.Any(worker => worker.IsAlive)) { }
            Thread.Sleep(1000);
            foreach (var vis in visualisators)
            {
                vis.Shutdown(SocketShutdown.Both);
                vis.Close();
            }
        }

        private void Worker(object i)
        {
            var iLocal = (int) i;
            lock (reservedInhabitants)
            {
                AfterStart(reservedInhabitants[iLocal], players[iLocal]);
            }
        }

        private void WaitConnections(Socket listener)
        {
            while (visualisators.Count == 0 || players.Count != settings.CountOfPlayers)
            {
                var buffer = new byte[128];
                var socket = listener.Accept();
                if (socket.Connected)
                {
                    socket.Receive(buffer);
                    if (Encoding.UTF8.GetString(buffer).Replace("\0", "").Equals("I am player"))
                    {
                        Console.WriteLine("Игрок подключился");
                        players.Add(socket);
                    }
                    else if (Encoding.UTF8.GetString(buffer).Replace("\0", "").Equals("I am visualisator"))
                    {
                        Console.WriteLine("Визуализатор подключился");
                        visualisators.Add(socket);
                    }
                }
            }
        }
        private void SendForestToVisualisators()
        {
            byte[] buffer;
            XmlLoader.SaveData("forestNow.xml", settings.Forest);
            using (var fs = new StreamReader("forestNow.xml"))
            {
                buffer = Encoding.UTF8.GetBytes(fs.ReadToEnd());
            }
            foreach (var vis in visualisators)
                vis.Send(buffer, buffer.Length, SocketFlags.None);
        }

        private Inhabitant BeforeStart(Socket player)
        {
            var buffer = new byte[1024];
            player.Send(Encoding.UTF8.GetBytes(string.Join(" ", settings.Forest.Inhabitants
                .Where(y => !reservedInhabitants.Contains(y))
                .Select(x => x.Name)
                .ToArray())));
            player.Receive(buffer);
            var name = Encoding.UTF8.GetString(buffer).Replace("\0", "");
            var selectedInhabitant = settings.Forest.Inhabitants
                .First(x => x.Name.Equals(name));
            var serializedInhabitant = JsonConvert.SerializeObject(selectedInhabitant);
            player.Send(Encoding.UTF8.GetBytes(serializedInhabitant));
            Thread.Sleep(50);
            var size = string.Format("{0} {1}", settings.Forest.Map.Length, settings.Forest.Map[0].Length);
            player.Send(Encoding.UTF8.GetBytes(size));
            return selectedInhabitant;
        }

        private void AfterStart(Inhabitant inhabitant,Socket player)
        {
            while (!inhabitant.Place.Equals(inhabitant.Purpose))
            {
                var buffer = new byte[32];
                player.Receive(buffer);
                var data = Encoding.UTF8.GetString(buffer
                    .Where(y => y != 0)
                    .ToArray());
                var command = JsonConvert.DeserializeObject<Coordinates>(data);
                Thread.Sleep(200);
                if (settings.Forest.Move(ref inhabitant, command))
                {
                    Thread.Sleep(50);
                    if (inhabitant.Place.Equals(inhabitant.Purpose))
                        break;
                    if (inhabitant.Life <= 0)
                    {                      
                        EndConnectionWithPlayer(player);
                        return;
                    }
                    SendForestToVisualisators();
                    SendData(inhabitant, player);
                    if (bots.Count(worker => worker.IsAlive) > 1)
                    {
                        Monitor.Pulse(reservedInhabitants);
                        Monitor.Wait(reservedInhabitants);
                    }
                    continue;
                }
                SendData(inhabitant, player);
            }
            settings.Forest.DestroyInhabitant(ref inhabitant);
            EndConnectionWithPlayer(player);
        }

        private void SendData(Inhabitant inhabitant, Socket player)
        {
            var buffer = new byte[8192];
            var visObjects = GetVisibleObjects(inhabitant);
            var formatter = new XmlSerializer(typeof(ForestObject[]), new[] { typeof(Bush), typeof(Trap), typeof(Inhabitant), typeof(Life), typeof(Footpath) });
            using (var fs = new FileStream("visObj.xml", FileMode.Create))
            {
                formatter.Serialize(fs, visObjects);
            }
            using (var fs = new FileStream("visObj.xml", FileMode.Open))
            {
                fs.Read(buffer, 0, buffer.Length);
            }
            File.Delete("visObj.xml");
            var buff = buffer.Where(x=>x!=0).ToList();
            var serializedInhabitant = JsonConvert.SerializeObject(inhabitant);
            buff.AddRange(Encoding.UTF8.GetBytes("###"));
            buff.AddRange(Encoding.UTF8.GetBytes(serializedInhabitant));
            player.Send(buff.ToArray());
        }

        private ForestObject[] GetVisibleObjects(Inhabitant inhabitant)
        {
            var neighbours = new List<Coordinates>();
            var queue = new Queue<Coordinates>();
            queue.Enqueue(inhabitant.Place);
            while (queue.Count != 0)
            {
                var top = queue.Dequeue();
                foreach (var neighbour in GetNeighbours(top).Where(x => !neighbours.Contains(x)))
                {
                    if (Math.Abs(neighbour.Substract(inhabitant.Place).X) + Math.Abs(neighbour.Substract(inhabitant.Place).Y) <= settings.Visible)
                    {
                        queue.Enqueue(neighbour);
                        neighbours.Add(neighbour);
                    }
                }
            }
            neighbours.Remove(inhabitant.Place);
            return neighbours.Select(neigh => settings.Forest.Map[neigh.Y][neigh.X]).ToArray();
        }

        private IEnumerable<Coordinates> GetNeighbours(Coordinates top)
        {
            var neigh = new List<Coordinates>();
            if (!OutOfBorders(top.Add(MoveCommand.Right)))
                neigh.Add(top.Add(MoveCommand.Right));
            if (!OutOfBorders(top.Add(MoveCommand.Up)))
                neigh.Add(top.Add(MoveCommand.Up));
            if (!OutOfBorders(top.Add(MoveCommand.Left)))
                neigh.Add(top.Add(MoveCommand.Left));
            if (!OutOfBorders(top.Add(MoveCommand.Down)))
                neigh.Add(top.Add(MoveCommand.Down));
            return neigh;
        }

        private void EndConnectionWithPlayer(Socket player)
        {
            SendForestToVisualisators();
            player.Shutdown(SocketShutdown.Both);
            player.Close();
            Monitor.Pulse(reservedInhabitants);
        }

        private void CreateInhabitantsOnTheRandomPlaces()
        {
            var rnd = new Random();
            for (var i = 0; i < settings.CountOfPlayers; i++)
            {
                var canEnterObjects = new List<ForestObject>();
                foreach (var rowForestObjects in settings.Forest.Map)
                    canEnterObjects.AddRange(rowForestObjects.Where(forestObject => forestObject.CanMove));
                var randomObject = canEnterObjects[rnd.Next(canEnterObjects.Count)];
                canEnterObjects.Remove(randomObject);
                var purpose = canEnterObjects[rnd.Next(canEnterObjects.Count)];
                canEnterObjects.Add(randomObject);
                var inhabitant = new Inhabitant(RandomStringGenerator(rnd.Next(4, 8)), rnd.Next(2, 4));
                settings.Forest.CreateInhabitant(ref inhabitant, randomObject.Place, purpose.Place);
            }
        }


        private Settings LoadSettings(string path,int playersCount, int visible)
        {
            settings = new Settings
            {
                Forest = new ForestLoader(new StreamReader(path)).Load(),
                CountOfPlayers = playersCount, 
                Visible = visible
            };
            CreateInhabitantsOnTheRandomPlaces();
            return settings;
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

        private bool OutOfBorders(Coordinates position)
        {
            if (position == null)
                return true;
            return position.X < 0 || position.Y >= settings.Forest.Map.Length || position.Y < 0 || position.X >= settings.Forest.Map[0].Length;
        }
    }
}
