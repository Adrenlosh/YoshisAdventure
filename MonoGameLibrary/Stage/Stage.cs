using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace MonoGameLibrary.Stage
{
    public class Stage
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        public Tilemap Tilemap { get; set; }

        public Point PlayerIntroTilePosition { get; set; }

        public Stage() { }
    }
}
