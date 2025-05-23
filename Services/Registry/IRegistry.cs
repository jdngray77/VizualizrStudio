namespace Services.Registry
{
    public interface IRegistry
    {
        int ReadInt(RegistryKey key);

        float ReadFloat(RegistryKey key);

        bool ReadBool(RegistryKey key);

        string ReadString(RegistryKey key);
    }
}