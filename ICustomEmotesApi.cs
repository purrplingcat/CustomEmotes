using StardewValley;
using System.Collections.Generic;

namespace CustomEmotes
{
    public interface ICustomEmotesApi
    {
        void DoEmote(Character character, string emoteName);
        void DoEmote(string characterName, string emoteName);
        Dictionary<string, int> GetEmoteMap();
    }
}