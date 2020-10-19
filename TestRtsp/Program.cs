using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HCNetLib.Stream;

namespace TestRtsp
{
    class Program
    {
        static void Main(string[] args)
        {
            var o = GPUSelectHelper.EnumerateSupportedGPU().ToList();
        }
    }
}
