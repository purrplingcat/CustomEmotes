using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CustomEmotes.Model
{
    internal class EmoteDefinition
    {
        public string Image { get; set; }
        public Dictionary<int, string> Map { get; set; }
        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }
    }
}
