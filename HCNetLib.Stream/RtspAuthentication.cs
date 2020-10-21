using System;
using System.Net;

namespace HCNetLib.Stream
{
    public class RtspAuthentication
    {
        public RtspAuthentication(string username, string password)
        {
            Credential = new NetworkCredential(username, password);
        }

        public NetworkCredential Credential { get; }
    }
}