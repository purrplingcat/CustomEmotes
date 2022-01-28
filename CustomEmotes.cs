using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomEmotes
{
    /// <summary>The mod entry point.</summary>
    public class CustomEmotes : Mod, IAssetEditor
    {
        private readonly Dictionary<string, int> _emoteIndexMap = new();
        private readonly LocalizedContentManager _vanillaContent = new(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);

        internal static CustomEmotes Instance { get; private set; }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("TileSheets\\emotes");
        }

        public void Edit<T>(IAssetData asset)
        {
            var emoteMap = new Dictionary<string, Texture2D>();
            var original = this._vanillaContent.Load<Texture2D>("TileSheets\\emotes");
            var image = asset.AsImage();
            int startIndex = (original.Width / 16) * (original.Height / 16);

            this._emoteIndexMap.Clear();
            this.LoadEmotes(emoteMap);
            image.ExtendImage(0, original.Height + emoteMap.Count * 16);

            int cursor = 0;
            foreach (var emote in emoteMap)
            {
                image.PatchImage(emote.Value, null, new Rectangle(0, original.Height + cursor * 16, original.Width, 16));
                this._emoteIndexMap[emote.Key] = startIndex + cursor;
                ++cursor;
            }

            // This loop adds vanilla emote names to lookup map
            // Vanilla emote names are immutable, so they can't be overriden
            foreach (var emote in Farmer.EMOTES)
            {
                if (emote.emoteIconIndex > 0)
                    this._emoteIndexMap[emote.emoteString] = emote.emoteIconIndex;
            }

            this.Monitor.Log($"Injected {this._emoteIndexMap.Count} new custom emotes");
        }

        private void LoadEmotes(Dictionary<string, Texture2D> emoteMap)
        {
            foreach (var pack in this.Helper.ContentPacks.GetOwned())
            {
                if (!pack.HasFile("emotes.json"))
                {
                    this.Monitor.Log($"Custom emotes pack {pack.Manifest.UniqueID} has no 'emotes.json' file", LogLevel.Error);
                    continue;
                }

                emoteMap.AddRange(
                    pack.ReadJsonFile<Dictionary<string, string>>("emotes.json")
                        .ToDictionary(d => d.Key, d => pack.LoadAsset<Texture2D>(d.Value))
                );
            }
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            helper.Content.AssetEditors.Add(this);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_emote)),
                prefix: new HarmonyMethod(this.GetType(), nameof(PATCH_prefix_command_emote))
            );
        }

        public override object GetApi()
        {
            return new CustomEmotesApi(this);
        }

        private static bool PATCH_prefix_command_emote(Event __instance, GameLocation location, GameTime time, ref string[] split)
        {
            if (split.Length > 2 && !int.TryParse(split[2], out var _))
            {
                if (Instance._emoteIndexMap.TryGetValue(split[2], out int index))
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

        internal int GetEmoteIndex(string emoteName)
        {
            if (this._emoteIndexMap.TryGetValue(emoteName, out int index))
            {
                return index;
            }

            this.Monitor.Log($"Unknown emote '{emoteName}' ");

            return -1;
        }

        internal Dictionary<string, int> EmoteIndexMap => this._emoteIndexMap;
    }
}
