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
        private readonly LocalizedContentManager _vanillaContent = new(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);

        internal Dictionary<string, int> EmoteIndexMap { get; } = new();

        internal static CustomEmotes Instance { get; private set; }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("TileSheets\\emotes");
        }

        public void Edit<T>(IAssetData asset)
        {
            var emotes = this.LoadEmotes().Deduplicate(e => e.name).ToList();
            var original = this._vanillaContent.Load<Texture2D>("TileSheets\\emotes");
            var image = asset.AsImage();
            int startIndex = (original.Width / 16) * (original.Height / 16);

            this.EmoteIndexMap.Clear();
            
            image.ExtendImage(0, original.Height + emotes.Count * 16);

            int cursor = 0;
            foreach (var emote in emotes)
            {
                if (emote.index * 16 > emote.texture.Height)
                {
                    this.Monitor.Log($"Emote '{emote.name}' is out of image bounds", LogLevel.Error);
                    continue;
                }

                image.PatchImage(emote.texture, new Rectangle(0, emote.index * 16, original.Width, 16), new Rectangle(0, original.Height + cursor * 16, original.Width, 16));
                this.EmoteIndexMap[emote.name] = startIndex + cursor * (original.Width / 16);
                ++cursor;
            }

            // This loop adds vanilla emote names to lookup map
            // Vanilla emote names are immutable, so they can't be overriden
            foreach (var emote in Farmer.EMOTES)
            {
                if (emote.emoteIconIndex > 0)
                    this.EmoteIndexMap[emote.emoteString] = emote.emoteIconIndex;
            }

            this.Monitor.Log($"Injected {this.EmoteIndexMap.Count} new custom emotes");
        }

        private IEnumerable<Emote> LoadEmotes()
        {
            foreach (var pack in this.Helper.ContentPacks.GetOwned())
            {
                if (!pack.HasFile("emotes.json"))
                {
                    this.Monitor.Log($"Custom emotes pack {pack.Manifest.UniqueID} has no 'emotes.json' file", LogLevel.Error);
                    continue;
                }

                foreach (var definition in pack.ReadJsonFile<EmoteDefinition[]>("emotes.json"))
                {
                    if (!pack.HasFile(definition.Image))
                    {
                        this.Monitor.Log($"Missing image '{definition.Image}' in custom emotes pack {pack.Manifest.UniqueID}", LogLevel.Error);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(definition.EnableWithMod) && !this.Helper.ModRegistry.IsLoaded(definition.EnableWithMod))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(definition.DisableWithMod) && this.Helper.ModRegistry.IsLoaded(definition.DisableWithMod))
                    {
                        continue;
                    }

                    var texture = pack.LoadAsset<Texture2D>(definition.Image);

                    foreach (var pair in definition.Map)
                    {
                        yield return new Emote(pair.Value, texture, pair.Key);
                    }
                }
            }
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            helper.Content.AssetEditors.Add(this);
            helper.Events.Display.Rendered += this.Display_Rendered;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_emote)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Prefix_command_emote))
            );
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady && e.Button == SButton.F2)
            {
                ICustomEmotesApi api = (ICustomEmotesApi)this.GetApi();

                api.DoEmote(Game1.player, "confused");
                api.DoEmote("Abigail", "check_ok");
            }
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            e.SpriteBatch.Draw(Game1.emoteSpriteSheet, Vector2.Zero, Color.White);
        }

        public override object GetApi()
        {
            return new CustomEmotesApi(this);
        }
    }
}
