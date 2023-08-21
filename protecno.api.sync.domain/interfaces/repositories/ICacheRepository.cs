namespace protecno.api.sync.domain.interfaces.repositories
{
    public interface ICacheRepository
    {
        string GetKeyInMemory(string key);

        void SetKeyInMemory(string key, string value, int minutesTTL);

        void RemoveKeysInMemoryCacheByPartKey(string partKey);
    }
}
