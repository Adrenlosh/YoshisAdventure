using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;

namespace YoshisAdventure.GameObjects
{
    public class Coin : GameObject
    {        
        public override Vector2 Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Rectangle CollisionBox => throw new NotImplementedException();

        public Coin(TiledMap tilemap) : base(tilemap)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
