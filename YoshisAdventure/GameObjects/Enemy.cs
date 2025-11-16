using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;

namespace YoshisAdventure.GameObjects
{
    public class Enemy : GameObject, IDamageable
    {
        private AnimatedSprite _sprite;
        private Vector2 _velocity;

        public override Vector2 Velocity { get => _velocity; set => _velocity = value; }

        public override Rectangle CollisionBox => GetCollisionBoxBottomCenter(Position, _sprite.Size);

        public int Health => throw new NotImplementedException();

        public int MaxHealth => throw new NotImplementedException();

        public Enemy(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("idle");
            Size = new Point(16, 16);
        }

        public void Die(bool ClearHealth)
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
            ApplyPhysics(gameTime);
            Position += Velocity;
            if(Velocity.X < 0)
            {
                _sprite.Effect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _sprite.Effect = SpriteEffects.None;
            }
            if ((int)gameTime.TotalGameTime.TotalSeconds % 2 == 0)
            {
                Velocity = new Vector2(0.5f, Velocity.Y);
            }
            else
            {
                Velocity = new Vector2(-0.5f, Velocity.Y);
            }
            _sprite.Update(gameTime);
        }
    }
}
