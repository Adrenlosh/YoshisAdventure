using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoshisAdventure.GameObjects
{
    public class Goal : GameObject
    {
        AnimatedSprite _sprite;

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        public Goal(SpriteSheet sheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(sheet);
            _sprite.SetAnimation("NormalWhite");
            Size = new Point(28, 134);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }
    }
}
