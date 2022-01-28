using Microsoft.Xna.Framework.Graphics;

namespace CustomEmotes
{
    internal class Emote
    {
        public string name;
        public Texture2D texture;
        public int index;

        public Emote(string name, Texture2D texture, int index)
        {
            this.name = name;
            this.texture = texture;
            this.index = index;
        }
    }
}