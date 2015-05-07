using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientVisualisator
{
    class Program
    {
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
                socket.Send(Encoding.UTF8.GetBytes("I am visualisator"));
                while (socket.Connected)
                {
                    try
                    {
                        var buffer = new byte[socket.ReceiveBufferSize];
                        socket.Receive(buffer);
                        var visualisator = new ConsoleVisualisator();
                        using (var fs = new FileStream("forest.xml", FileMode.Create))
                        {
                            fs.Write(buffer, 0, buffer.Length);
                        }
                        var forest = XmlLoader.DeserializeForest("forest.xml");
                        foreach (var inhabitant in forest.Inhabitants.Where(inh => inh != null))
                            visualisator.AddInhabitantToDictionary(inhabitant);
                        visualisator.DrawForest(forest);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
        }
    }
}
