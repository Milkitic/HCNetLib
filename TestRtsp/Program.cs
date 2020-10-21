using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HCNetLib;
using HCNetLib.Stream;
using HCNetLib.Stream.Builder;

namespace TestRtsp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var file = File.ReadAllLines("secret.txt");
            var username = file[0];
            var password = file[1];
            var host = file[2];

            var management = new AutoStreamManagement(@"e:\m3u8", TimeSpan.FromSeconds(2000), 
                username, password);
            //var size = new Size(1280, 720);
            var size = new Size(480, 270);
            for (int i = 1; i <= 8; i++)
            {
                try
                {
                    management.AddTaskWithHeartBeat(new RtspIdentity(host, i, BitStream.Sub), size);
                    //await management.RemoveTask(host, i, BitStream.Sub);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError(ex.Message, "main");
                }
            }

            size = new Size(1280, 720);
            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    management.AddTaskWithHeartBeat(new RtspIdentity(host, i, BitStream.Main), size);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError(ex.Message, "main");
                }
            }

            Console.ReadLine();

            management.Dispose();
        }
    }
}
