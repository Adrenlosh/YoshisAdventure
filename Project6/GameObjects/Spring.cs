using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using Project6.Enums;
using Project6.Structures;

namespace Project6.GameObjects
{
    public enum SpringStatus
    {
        Normal,
        Compressing,
        CompressedToMax,
        Expanding
    }

    public class Spring : GameObject
    {
        private const float Gravity = 0.5f;
        private const float MaxGravity = 8f;

        private readonly Point _normalCollisionBox = new Point(16, 16);
        private readonly Point _minimumCollisionBox = new Point(16, 8);
        private float _keepTimer = 0f;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private const float KeepDuration = 0.1f;
        private Vector2 _basePosition;
        AnimatedSprite _sprite;

        public SpringStatus Status { get; private set; } = SpringStatus.Normal;

        public override void OnCollision(GameObject other, CollisionResult collision)
        {
            base.OnCollision(other, collision);
            if (collision.Direction == CollisionDirection.Top && Status == SpringStatus.Normal)
            {
                Compress();
            }
        }

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        public Spring(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("Normal");
            Size = _normalCollisionBox;
        }

        public void Compress()
        {
            if (Status != SpringStatus.Normal) return;

            _sprite.SetAnimation("Compress");
            Status = SpringStatus.Compressing;
            _basePosition = Position;
            _keepTimer = KeepDuration;
        }

        public void Release()
        {
            if (Status == SpringStatus.CompressedToMax)
            {
                Status = SpringStatus.Expanding;
                _sprite.SetAnimation("Expand");
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 newPosition = Position;
            switch (Status)
            {
                case SpringStatus.Compressing:
                    UpdateCompressing();
                    break;

                case SpringStatus.CompressedToMax:
                    UpdateCompressedToMax(deltaTime);
                    break;

                case SpringStatus.Expanding:
                    UpdateExpanding();
                    break;

                case SpringStatus.Normal:
                    break;
            }

            if (!_isOnGround)
            {
                _velocity.Y += Gravity;
                if (_velocity.Y > MaxGravity)
                    _velocity.Y = MaxGravity;
            }
            if (_velocity.Y != 0) //垂直碰撞检测
            {
                Vector2 verticalMove = new Vector2(0, _velocity.Y);
                Vector2 testPosition = newPosition + verticalMove;
                if (testPosition.Y < 0)
                {
                    newPosition.Y = 0;
                    _velocity.Y = 0;
                }
                else
                {
                    Rectangle testRect = GetCollisionBox(testPosition);
                    if (IsCollidingWithTile(testRect, out Rectangle tileRect))
                    {
                        if (_velocity.Y > 0.5)
                        {
                            float tileTop = tileRect.Top;
                            newPosition.Y = tileTop - _sprite.Size.Y;
                            _velocity.Y = 0;
                            _isOnGround = true;
                        }
                        else if (_velocity.Y < 0)
                        {
                            newPosition.Y = tileRect.Bottom;
                            _velocity.Y = 0;
                        }
                    }
                    else
                    {
                        newPosition += verticalMove;
                        _isOnGround = false;
                    }
                }
            }
            else
            {
                Rectangle collisionBox = GetCollisionBox(newPosition);
                if (!IsCollidingWithTile(new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3), out _))
                {
                    _isOnGround = false;
                }
                else
                {
                    _isOnGround = true;
                }
            }
            Position = newPosition;

            _sprite.Update(gameTime);
        }

        private void UpdateCompressing()
        {
            int newHeight = Size.Y - 2;
            if (newHeight <= _minimumCollisionBox.Y)
            {
                Size = _minimumCollisionBox;
                Status = SpringStatus.CompressedToMax;
                _keepTimer = KeepDuration;
            }
            else
            {
                Size = new Point(Size.X, newHeight);
            }
        } 

        private void UpdateCompressedToMax(float deltaTime)
        {
            _keepTimer -= deltaTime;
            if (_keepTimer <= 0)
            {
                Release();
            }
        }

        private void UpdateExpanding()
        {
            int newHeight = Size.Y + 4;
            if (newHeight >= _normalCollisionBox.Y)
            {
                Size = _normalCollisionBox;
                Position = _basePosition;
                Status = SpringStatus.Normal;
                _sprite.SetAnimation("Normal");
            }
            else
            {
                Size = new Point(Size.X, newHeight);
            }
        }

        public void Reset()
        {
            Status = SpringStatus.Normal;
            Size = _normalCollisionBox;
            Position = _basePosition;
            _sprite.SetAnimation("Normal");
        }
    }
}