using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using Project6.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project6.GameObjects
{
    public class Spring : GameObject
    {
        AnimatedSprite _sprite;

        public Spring(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("Compress");
            Size = new Point(16, 16);
        }

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        public override void Draw(SpriteBatch spriteBatch)
        {
            //throw new NotImplementedException();
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }
    }
}
