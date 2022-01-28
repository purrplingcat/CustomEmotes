using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace CustomEmotes
{
    public class CustomEmotesApi : ICustomEmotesApi
    {
        private readonly CustomEmotes _mod;

        internal CustomEmotesApi(CustomEmotes mod)
        {
            this._mod = mod;
        }

        public void DoEmote(Character character, string emoteName)
        {
            int index = this._mod.GetEmoteIndex(emoteName);

            if (Context.IsWorldReady && index > 0 && character != null)
            {
                character.isEmoting = false;
                character.doEmote(index);
            }
        }

        public void DoEmote(string characterName, string emoteName)
        {
            if (Context.IsWorldReady)
            {
                if (characterName.Contains("farmer"))
                {
                    var farmer = Utility.getFarmerFromFarmerNumberString(characterName, Game1.player);
                    this.DoEmote(farmer, emoteName);
                    return;
                }

                var npc = Game1.getCharacterFromName(characterName);
                this.DoEmote(npc, emoteName);
            }
        }

        public Dictionary<string, int> GetEmoteMap()
        {
            return this._mod.EmoteIndexMap;
        }
    }
}
