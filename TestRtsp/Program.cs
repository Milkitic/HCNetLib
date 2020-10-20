using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HCNetLib.Stream;
using HCNetLib.Stream.Builder;

namespace TestRtsp
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = File.ReadAllLines("secret.txt");
            var username = file[0];
            var password = file[1];
            var host = file[2];

            var management = new StreamManagement(@"e:\m3u8", username, password);
            //var size = new Size(1280, 720);
            var size = new Size(480, 270);
            for (int i = 1; i <= 8; i++)
            {
                management.QueueTask(host, i, BitStream.Sub, size);
            }

            size = new Size(1280, 720);
            for (int i = 1; i <= 3; i++)
            {
                management.QueueTask(host, i, BitStream.Main, size);
            }
            Console.ReadLine();
        }
    }
}
