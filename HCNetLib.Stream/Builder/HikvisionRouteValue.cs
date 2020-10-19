using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HCNetLib.Stream.Builder
{
    public class HikvisionRouteValue : IRouteValue
    {
        public HikvisionRouteValue(string route)
        {
            Route = route;
        }

        public string Route { get; }

        /// <summary>
        /// 新版协议
        /// </summary>
        /// <param name="channel">通道，从1开始（0为零通道）</param>
        /// <param name="bitStream">码流，从1开始</param>
        /// <param name="parameters">其他参数</param>
        /// <returns></returns>
        public static HikvisionRouteValue FromSettings(int channel, BitStream bitStream,
            Dictionary<string, string> parameters = null)
        {
            var bitStreamI = bitStream switch
            {
                BitStream.Main => 1,
                BitStream.Sub => 2,
                BitStream.Third => 3,
                _ => 1
            };

            var route = $"/Streaming/Channels/{channel}0{bitStreamI}";
            if (parameters != null && parameters.Count > 0)
            {
                route += "?" + string.Join("&",
                    parameters.Select(pair => HttpUtility.UrlEncode((string)pair.Key) + "=" + HttpUtility.UrlEncode((string)pair.Value))
                );
            }

            return new HikvisionRouteValue(route);
        }

        /// <summary>
        /// 旧版协议
        /// </summary>
        /// <param name="channel">通道，从1开始。1-32模拟通道，33~IP通道</param>
        /// <param name="bitStream">码流，从1开始</param>
        /// <returns></returns>
        public static HikvisionRouteValue FromOldProtocolSettings(int channel, BitStream bitStream)
        {
            var bitStreamStr = bitStream switch
            {
                BitStream.Main => "main",
                BitStream.Sub => "sub",
                BitStream.Third => "stream3",
                _ => "main"
            };

            var route = $"/h264/ch{channel}/{bitStreamStr}";
            return new HikvisionRouteValue(route);
        }
    }
}