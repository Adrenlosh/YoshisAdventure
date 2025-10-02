using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;

namespace YoshisAdventure.GameObjects
{
    public class Enemy : GameObject, IDamageable
    {
        private AnimatedSprite _sprite;
        private Vector2 _velocity;

        public override Vector2 Velocity { get => _velocity; set => _velocity = value; }

        public override Rectangle CollisionBox => GetCollisionBoxBottomCenter(Position, _sprite.Size);

        public int Health => throw new NotImplementedException();

        public Enemy(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("Normal");
            Size = new Point(16, 32);
        }

        public void Die()
        {

        }

        public void TakeDamage(int damage, GameObject source)
        {

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
