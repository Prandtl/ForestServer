using System.Diagnostics;
using System.IO;

namespace ForestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ForestServer("config.xml");
            server.Start("127.0.0.1", 11000);
        }
    }
}
