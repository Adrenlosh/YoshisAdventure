using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Interfaces;
using System;
using YoshisAdventure.Models;
using YoshisAdventure.Enums;

namespace YoshisAdventure.GameObjects
{
    public class Egg : GameObject, IProjectile
    {
        private readonly Sprite _sprite;
        private Vector2 _throwDirection;
        private Vector2 _velocity;
        private float _throwTime = 0f;
       
        public int Damage { get; private set; } = 1;

        public bool IsHeldAndThrew { get; set; } = false;

        public GameObject Owner { get; private set; }

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public override Vector2 Velocity { get => _velocity; set => _velocity = value; }

        public event Action OnOutOfBounds;

        public Egg(Texture2D texture, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new Sprite(texture);
            Size = new Point(16, 16);
            _velocity = new Vector2(10f, 10f);
            IsCapturable = false;
        }

        protected override Rectangle GetCollisionBox(Vector2 position)
        {
            int centerX = (int)(position.X + _sprite.Size.X / 2 - Size.X / 2);
            int centerY = (int)(position.Y + _sprite.Size.Y / 2 - Size.Y / 2);
            return new Rectangle(centerX, centerY, Size.X, Size.Y);
        }

        public void Throw(Vector2 throwDirection)
        {
            IsHeldAndThrew = true;
            _throwDirection = throwDirection;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsHeldAndThrew) return;
            _throwTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position = GetPositionAtTime(_throwTime, Position, _throwDirection,55f , 222.8f);
            if ((IsCollidingWithTile(CollisionBox, out TileCollisionResult result) && !result.TileType.HasFlag(TileType.Penetrable) && !result.TileType.HasFlag(TileType.Platform)) || IsOutOfScreenBounds())
            {
                IsHeldAndThrew = false;
                _throwTime = 0;
                OnOutOfBounds.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        Vector2 GetPositionAtTime(float t, Vector2 startPos, Vector2 throwDirection, float initialSpeed, float gravity)
        {
            float vx = throwDirection.X * initialSpeed;
            float vy = throwDirection.Y * initialSpeed;
            float x = startPos.X + vx * t;
            float y = startPos.Y + vy * t + 0.5f * gravity * t * t;
            return new Vector2(x, y);
        }
    }
}