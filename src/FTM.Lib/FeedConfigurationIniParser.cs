using IniParser.Model;
using IniParser.Parser;

namespace FTM.Lib;

public class FeedConfigurationIniParser : IniDataParser
{
    public const char ConcatenateSeparator = (char)0x02;

    protected override void HandleDuplicatedKeyInCollection(string key, string value,
        KeyDataCollection keyDataCollection, string sectionName)
    {
        if (Configuration.ConcatenateDuplicateKeys)
        {
            keyDataCollection[key] += ConcatenateSeparator + value;
        }
        else
        {
            base.HandleDuplicatedKeyInCollection(key, value, keyDataCollection, sectionName);
        }
    }
}