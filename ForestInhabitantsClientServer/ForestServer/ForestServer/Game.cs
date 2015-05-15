using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ForestServer.ForestObjects;
using Newtonsoft.Json;

namespace ForestServer
{
    public class Game
    {
        public List<Player> Players;
        public Socket Visualisator;
        public Settings GameOptions;
        private readonly List<Inhabitant> reservedInhabitants = new List<Inhabitant>();
        private readonly List<Thread> bots = new List<Thread>();
        public Player Winner;
        public Player Looser;
        public bool NeedContinue;

        public Game(bool needContinue,Settings gameOptions, Socket visualisator, params Player[] players)
        {
            NeedContinue = needContinue;
            GameOptions = gameOptions;
            Visualisator = visualisator;
            Players = players.ToList();
        }

        public void Start()
        {
            foreach (var player in Players)
            {
                reservedInhabitants.Add(BeforeStart(player));
                bots.Add(new Thread(Worker));
            }
            for (var i = 0; i < reservedInhabitants.Count; i++)
                bots[i].Start(i);
            while (bots.Any(bot => bot.IsAlive)) { }
        }

        private void SendForestToVisualisator()
        {
            byte[] buffer;
            XmlLoader.SaveData("forestNow.xml", GameOptions.Forest);
            using (var fs = new StreamReader("forestNow.xml"))
            {
                buffer = Encoding.UTF8.GetBytes(fs.ReadToEnd());
            }
            Visualisator.Send(buffer, buffer.Length, SocketFlags.None);
        }   

         private Inhabitant BeforeStart(Player player)
        {
             var inhabitant =
                 GameOptions.Forest.Inhabitants
                 .Where(x => !reservedInhabitants.Contains(x))
                 .First();
             inhabitant.Name = player.Name;
            player.Socket.Send(Encoding.UTF8.GetBytes(string.Format("OK {0} {1} {2} {3} {4}",GameOptions.Visible,inhabitant.Place,inhabitant.Purpose,inhabitant.Life,inhabitant.PrevObject.GetType().ToString().Substring(27))));
            return inhabitant;
        }

         private void Worker(object i)
         {
             var iLocal = (int)i;
             lock (reservedInhabitants)
             {
                 AfterStart(reservedInhabitants[iLocal], Players[iLocal]);
             }
         }

         private void AfterStart(Inhabitant inhabitant, Player player)
         {
             while (!inhabitant.Place.Equals(inhabitant.Purpose))
             {
                 var buffer = new byte[32];
                 player.Socket.Receive(buffer);
                 var data = Encoding.UTF8.GetString(buffer
                     .Where(y => y != 0)
                     .ToArray()).ToLower().Split(' ')[1];
                 var command = GetCommand(data);
                 Thread.Sleep(200);
                 if (GameOptions.Forest.Move(ref inhabitant, command))
                 {
                     Thread.Sleep(50);
                     SendForestToVisualisator();
                     if (inhabitant.Place.Equals(inhabitant.Purpose))
                     {
                         EndGame(player,Players.First(x => x != player));
                         return;
                     }
                     if (inhabitant.Life <= 0)
                     {
                         EndGame(Players.First(x=>x != player),player);
                         return;
                     }
                     SendData(true,inhabitant, player);
                     if (bots.Count(bot => bot.IsAlive) > 1)
                     {
                         Monitor.Pulse(reservedInhabitants);
                         Monitor.Wait(reservedInhabitants);
                     }
                     continue;
                 }
                 SendData(false,inhabitant, player);
             }
         }

         private void EndGame(Player winner,Player looser)
         {
             winner.Socket.Send(NeedContinue
                 ? Encoding.UTF8.GetBytes("win continue")
                 : Encoding.UTF8.GetBytes("win end"));
             looser.Socket.Send(Encoding.UTF8.GetBytes("game over"));      
             Winner = winner;
             Looser = looser;
         }

         private Coordinates GetCommand(string data)
         {
             switch (data)
             {
                 case "up":
                     return MoveCommand.Up;
                 case "down":
                     return MoveCommand.Down;
                 case "right":
                     return MoveCommand.Right;
                 case "left":
                     return MoveCommand.Left;
             }
             return new Coordinates(0,0);
         }

         private void SendData(bool isSuccess, Inhabitant inhabitant, Player player)
         {
             var visObjects = GetVisibleObjects(inhabitant);
             var receivedLetter = new StringBuilder();
             receivedLetter.Append(string.Format("{0} {1}\n", isSuccess, inhabitant.PrevObject.GetType().ToString().Substring(27)));
             foreach (var forestObject in visObjects)
                 receivedLetter.Append(string.Format("{0} {1}\n",
                     forestObject.GetType().ToString().Substring(27).ToLower(), forestObject.Place.ToString()));
             var buffer = Encoding.UTF8.GetBytes(receivedLetter.ToString());
             player.Socket.Send(buffer, buffer.Length, 0);
         }

         private IEnumerable<ForestObject> GetVisibleObjects(Inhabitant inhabitant)
         {
             var neighbours = new List<Coordinates>();
             var queue = new Queue<Coordinates>();
             queue.Enqueue(inhabitant.Place);
             while (queue.Count != 0)
             {
                 var top = queue.Dequeue();
                 foreach (var neighbour in GetNeighbours(top).Where(x => !neighbours.Contains(x)))
                 {
                     if (Math.Abs(neighbour.Substract(inhabitant.Place).X) + Math.Abs(neighbour.Substract(inhabitant.Place).Y) <= GameOptions.Visible)
                     {
                         queue.Enqueue(neighbour);
                         neighbours.Add(neighbour);
                     }
                 }
             }
             neighbours.Remove(inhabitant.Place);
             return neighbours.Select(neigh => GameOptions.Forest.Map[neigh.Y][neigh.X]).ToArray();
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

         private bool OutOfBorders(Coordinates position)
         {
             if (position == null)
                 return true;
             return position.X < 0 || position.Y >= GameOptions.Forest.Map.Length || position.Y < 0 || position.X >= GameOptions.Forest.Map[0].Length;
         }
    }
}
