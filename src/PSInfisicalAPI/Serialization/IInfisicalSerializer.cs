namespace PSInfisicalAPI.Serialization
{
    public interface IInfisicalSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string value);
    }
}
