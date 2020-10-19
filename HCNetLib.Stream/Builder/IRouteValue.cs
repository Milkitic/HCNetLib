namespace HCNetLib.Stream.Builder
{
    public interface IRouteValue
    {
        public string Route { get; }
    }

    public enum BitStream
    {
        Main, Sub, Third
    }
}