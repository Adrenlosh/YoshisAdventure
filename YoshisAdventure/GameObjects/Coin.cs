using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Interfaces;
using YoshisAdventure.Models;
using YoshisAdventure.Systems;

namespace YoshisAdventure.GameObjects
{
    public class Coin : GameObject, IValuable
    {
        private AnimatedSprite _sprite;

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public int Value { get; } = 3;

        public Coin(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("normal");
            Size = new Point(16, 16);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            SFXSystem.Play("coin");
            base.OnCollision(other, collision);
        }
    }
}
