using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ForestServer
{
    public class Player
    {
        public string Name;
        public Socket Socket;

        public Player(string name, Socket socket)
        {
            Name = name;
            Socket = socket;
        }
    }
}
