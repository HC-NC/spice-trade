namespace SpiceTrade.Core.Interfaces;

public interface ILocalizationService
{
    string Get(string key);
    string Get(string key, params object[] args);
}