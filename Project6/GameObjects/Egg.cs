using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System.Drawing;
using System.Numerics;

namespace Project6.GameObjects
{
    public class Egg
    {
        public Vector2 Position = new Vector2(0, 0);
        public Size Size = new Size(0, 0);
        private Sprite _sprite;

        public Egg(TextureAtlas atlas, Tilemap tilemap)
        {
            _sprite = atlas.CreateSprite("egg");
        }

        public void Update()
        {

        }

        public void Draw()
        {
            _sprite.Draw(Core.SpriteBatch, new Vector2(100, 100));
        }
    }
}