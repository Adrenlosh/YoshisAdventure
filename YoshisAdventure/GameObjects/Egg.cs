using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Interfaces;
using System;

namespace YoshisAdventure.GameObjects
{
    public class Egg : GameObject, IProjectile
    {
        private const int MaxCollisionCount = 10;

        private readonly Sprite _sprite;
        private Vector2 _throwDirection;
        private int _collisionCount = 0;
        private bool _shouldFlyOut = false;

        public int CollisionCount => _collisionCount;

        public int Damage { get; private set; } = 1;

        public bool IsHeldAndThrew { get; set; } = false;

        public GameObject Owner { get; private set; }

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        public event Action OnOutOfBounds;

        public Egg(Texture2D texture, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new Sprite(texture);
            Size = new Point(16, 16);
            Velocity = new Vector2(10f, 10f);
        }

        protected override Rectangle GetCollisionBox(Vector2 position)
        {
            int centerX = (int)(position.X + _sprite.Size.X / 2 - Size.X / 2);
            int centerY = (int)(position.Y + _sprite.Size.Y / 2 - Size.Y / 2);
            return new Rectangle(centerX, centerY, Size.X, Size.Y);
        }

        private bool IsCollidingWithTileDetailed(Rectangle rect, out Rectangle tileRect, out Vector2 normal, out float penetrationDepth)
        {
            normal = Vector2.Zero;
            penetrationDepth = 0;
            if (IsCollidingWithTile(rect, out tileRect))
            {
                CalculateCollisionDetails(rect, tileRect, out normal, out penetrationDepth);
                return true;
            }
            return false;
        }

        private void CalculateCollisionDetails(Rectangle rect, Rectangle tileRect, out Vector2 normal, out float penetrationDepth)
        {
            normal = Vector2.Zero;
            penetrationDepth = 0;
            Vector2 eggCenter = new Vector2(rect.Center.X, rect.Center.Y);
            Vector2 tileCenter = new Vector2(tileRect.Center.X, tileRect.Center.Y);
            float overlapX = Math.Min(rect.Right, tileRect.Right) - Math.Max(rect.Left, tileRect.Left);
            float overlapY = Math.Min(rect.Bottom, tileRect.Bottom) - Math.Max(rect.Top, tileRect.Top);
            if (overlapX < overlapY)
            {
                penetrationDepth = overlapX;
                normal.X = (eggCenter.X < tileCenter.X) ? -1 : 1;
            }
            else
            {
                penetrationDepth = overlapY;
                normal.Y = (eggCenter.Y < tileCenter.Y) ? -1 : 1;
            }
        }

        public void Bounce(Vector2 normal, float penetrationDepth)
        {
            Position += normal * penetrationDepth;
            _collisionCount++;
            if (_collisionCount >= MaxCollisionCount)
            {
                _shouldFlyOut = true;
                return;
            }
            Velocity = Vector2.Reflect(Velocity, normal);
        }

        public void Throw(Vector2 throwDirection)
        {
            IsHeldAndThrew = true;
            _throwDirection = throwDirection;
            _shouldFlyOut = false;
            _collisionCount = 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsHeldAndThrew) return;
            Vector2 newPosition = Position + Velocity * _throwDirection;
            if (_shouldFlyOut)
            {
                Position = newPosition;
            }
            else
            {
                try
                {
                    Rectangle collisionBox = GetCollisionBox(newPosition);
                    if (IsCollidingWithTileDetailed(collisionBox, out Rectangle tileRect, out Vector2 normal, out float penetrationDepth))
                    {
                        Bounce(normal, penetrationDepth);
                    }
                    else
                    {
                        Position = newPosition;
                    }
                }
                catch
                {
                    IsHeldAndThrew = false;
                    OnOutOfBounds.Invoke();
                }
            }
            if (IsOutOfScreenBounds())
            {
                IsHeldAndThrew = false;
                _collisionCount = 0;
                _shouldFlyOut = false;
                OnOutOfBounds.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }
    }
}