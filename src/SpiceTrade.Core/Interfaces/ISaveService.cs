namespace SpiceTrade.Core.Interfaces;

public interface ISaveService
{
    void Save(string path, object state);
    T? Load<T>(string path);
}