using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;

namespace Project6.GameObjects
{
    public class TestObject : GameObject
    {
        private readonly Sprite _sprite;

        public TestObject(Texture2D texture, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new Sprite(texture);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
