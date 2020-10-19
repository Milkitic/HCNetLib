using HCNetLib.Stream.Builder;
using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace HCNetLib.Stream
{
    public class GPUSelectHelper
    {
        public static IEnumerable<ManufactureInfo> EnumerateSupportedGPU()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");
                int i = 0;
                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (name?.Contains("nvidia", StringComparison.OrdinalIgnoreCase) == true)
                        yield return new ManufactureInfo(name, i, Manufacture.NVIDIA);
                    else if (name?.Contains("amd", StringComparison.OrdinalIgnoreCase) == true)
                        yield return new ManufactureInfo(name, i, Manufacture.AMD);
                    else if (name?.Contains("intel", StringComparison.OrdinalIgnoreCase) == true)
                        yield return new ManufactureInfo(name, i, Manufacture.INTEL);

                    i++;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var cmd = "lspci | grep VGA";
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public struct ManufactureInfo
    {
        public ManufactureInfo(string name, int index, Manufacture manufacture)
        {
            Name = name;
            Index = index;
            Manufacture = manufacture;
        }

        public string Name { get; }
        public int Index { get; }
        public Manufacture Manufacture { get; }
    }
}