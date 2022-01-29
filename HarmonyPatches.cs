using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomEmotes
{
    internal static class HarmonyPatches
    {
        private static CustomEmotes Instance => CustomEmotes.Instance;

        public static bool Prefix_command_emote(Event __instance, GameLocation location, GameTime time, ref string[] split)
        {
            if (split.Length > 2 && !int.TryParse(split[2], out var _))
            {
                if (Instance.EmoteIndexMap.TryGetValue(split[2], out int index))
                {
                    split[2] = index.ToString();
                    return true;
                }

                Instance.Monitor.Log($"Unknown emote '{split[2]}' ");
                __instance.CurrentCommand++;
                __instance.checkForNextCommand(location, time);

                return false;
            }

            return true;
        }
    }
}
