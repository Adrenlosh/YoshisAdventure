using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Models;

namespace YoshisAdventure.GameObjects
{
    public class Goal : GameObject
    {
        private AnimatedSprite _sprite;

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public Goal(SpriteSheet sheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(sheet);
            _sprite.SetAnimation("NormalGreenStar");
            Size = new Point(28, 134);
            IsEatable = false;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            base.OnCollision(other, collision);
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