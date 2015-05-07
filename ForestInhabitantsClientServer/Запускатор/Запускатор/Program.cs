using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Запускатор
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                var playerProcess = new Process();
                playerProcess.StartInfo.FileName =
                    @"C:\Users\Александр\Desktop\My projects\ForestInhabitantsClientServer\ClientPlayer\ClientPlayer\bin\Debug\ClientPlayer.exe";
                playerProcess.Start();
                Thread.Sleep(1000);
            }
        }
    }
}
