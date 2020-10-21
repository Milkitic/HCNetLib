using System;

namespace HCNetLib.Stream
{
    public class RtspIdentity : IEquatable<RtspIdentity>
    {
        public RtspIdentity(string host, int port, int channel, BitStream bitStream)
        {
            Host = host;
            Port = port;
            Channel = channel;
            BitStream = bitStream;
        }
        public RtspIdentity(string host, int channel, BitStream bitStream)
        {
            Host = host;
            Channel = channel;
            BitStream = bitStream;
            Port = 554;
        }

        public string Host { get; }
        public int Port { get; }
        public int Channel { get; }
        public BitStream BitStream { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RtspIdentity)obj);
        }

        public bool Equals(RtspIdentity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Host == other.Host && Port == other.Port && Channel == other.Channel && BitStream == other.BitStream;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Host, Port, Channel, (int)BitStream);
        }

        public static bool operator ==(RtspIdentity left, RtspIdentity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RtspIdentity left, RtspIdentity right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{Host}:{Port}/{Channel}0{(int)BitStream + 1}";
        }
    }
}
