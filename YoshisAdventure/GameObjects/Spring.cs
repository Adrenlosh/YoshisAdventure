using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System.Diagnostics;
using YoshisAdventure.Enums;
using YoshisAdventure.Models;
using YoshisAdventure.Systems;

namespace YoshisAdventure.GameObjects
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
        private const float Friction = 0.5f;

        private readonly Point _normalCollisionBox = new Point(16, 16);
        private readonly Point _minimumCollisionBox = new Point(16, 8);
        private float _keepTimer = 0f;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private const float KeepDuration = 0.1f;
        private Vector2 _basePosition;
        AnimatedSprite _sprite;
        private bool _isOnSlope = false;

        public SpringStatus Status { get; private set; } = SpringStatus.Normal;

        public override Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public Spring(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("normal");
            Size = _normalCollisionBox;
            IsCapturable = true;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            base.OnCollision(other, collision);
            if (collision.Direction == CollisionDirection.Top && Status == SpringStatus.Normal)
            {
                Compress();
            }
        }

        public void Compress()
        {
            if (Status != SpringStatus.Normal) return;
            Status = SpringStatus.Compressing;
            _basePosition = Position;
            _keepTimer = KeepDuration;
            if (_sprite.CurrentAnimation != "compress")
            {
                _sprite.SetAnimation("compress");
            }
        }

        public void Release()
        {
            if (Status == SpringStatus.CompressedToMax)
            {
                Status = SpringStatus.Expanding;
                if (_sprite.CurrentAnimation != "expand")
                {
                    _sprite.SetAnimation("expand");
                }
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
                    UpdateCompressing(ref newPosition);
                    break;

                case SpringStatus.CompressedToMax:
                    UpdateCompressedToMax(deltaTime);
                    break;

                case SpringStatus.Expanding:
                    UpdateExpanding(ref newPosition);
                    break;

                case SpringStatus.Normal:
                    break;
            }

            if (_velocity.X > 0)
            {
                _velocity.X -= MathHelper.Lerp(0, Friction, 0.3f);
                if (_velocity.X < 0)
                    _velocity.X = 0;
            }
            else if (_velocity.X < 0)
            {
                _velocity.X += MathHelper.Lerp(0, Friction, 0.3f);
                if (_velocity.X > 0)
                    _velocity.X = 0;
            }

            UpdateTileCollision(ref newPosition);

            Position = newPosition;

            _sprite.Update(gameTime);
        }

        private void UpdateTileCollision(ref Vector2 newPosition)
        {
            if (_velocity.X != 0) //水平碰撞检测
            {
                Vector2 horizontalMove = new Vector2(_velocity.X, 0);
                Vector2 testPosition = newPosition + horizontalMove;
                Rectangle testRect = GetCollisionBox(testPosition);
                if (!IsOutOfTilemapSideBox(testRect))
                {
                    bool isCollided = IsCollidingWithTile(testRect, out TileCollisionResult result);
                    if (IsOnSlope(result, out int slopeWidth, out int slopeHeight, out int topY, out int bottomY))
                    {
                        _isOnSlope = true;
                        UpdateSlopeMovement(ref newPosition, result, slopeWidth, slopeHeight, topY, bottomY);
                        newPosition += horizontalMove;
                    }
                    else
                    {
                        _isOnSlope = false;
                        if (isCollided && !result.TileType.HasFlag(TileType.Penetrable) && !result.TileType.HasFlag(TileType.Platform))
                        {
                            _velocity.X = 0;
                        }
                        else
                        {
                            newPosition += horizontalMove;
                        }
                    }
                }
            }

            if (_velocity.Y != 0) //垂直碰撞检测
            {
                Vector2 verticalMove = new Vector2(0, _velocity.Y);
                Vector2 testPosition = newPosition + verticalMove;

                if (testPosition.Y < 0)
                {
                    // 处理上边界碰撞
                    newPosition.Y = 0;
                    _velocity.Y = 0;
                }
                else
                {
                    Rectangle testRect = GetCollisionBox(testPosition);
                    // 首先检查与任何瓦片的碰撞
                    if (IsCollidingWithTile(testRect, out TileCollisionResult result))
                    {
                        if (IsOnSlope(result, out int slopeWidth, out int slopeHeight, out int topY, out int bottomY) && _velocity.Y > 0)
                        {
                            _isOnSlope = true;
                            UpdateSlopeMovement(ref newPosition, result, slopeWidth, slopeHeight, topY, bottomY);

                            // 如果是从上方落到斜坡上，重置状态
                            if (_velocity.Y > 0)
                            {
                                _isOnGround = true;
                            }
                        }
                        else
                        {
                            _isOnSlope = false;
                            // 对于不可穿透的瓦片，正常处理碰撞
                            if (!result.TileType.HasFlag(TileType.Penetrable))
                            {
                                if (_velocity.Y > 0.5) // 向下碰撞
                                {
                                    if (result.TileType.HasFlag(TileType.Platform))
                                    {
                                        // 平台瓦片：物体从上方下落时可以站上去
                                        float objectBottom = newPosition.Y + _sprite.Size.Y;
                                        float platformTop = result.TileRectangle.Top;

                                        // 只有当物体是从平台上方下落时才站在平台上
                                        if (objectBottom <= platformTop + 5)
                                        {
                                            newPosition.Y = platformTop - _sprite.Size.Y;
                                            _velocity.Y = 0;
                                            _isOnGround = true;
                                        }
                                        else
                                        {
                                            // 从下方接近平台，允许穿过
                                            newPosition += verticalMove;
                                            _isOnGround = false;
                                        }
                                    }
                                    else
                                    {
                                        float tileTop = result.TileRectangle.Top;
                                        newPosition.Y = tileTop - _sprite.Size.Y;
                                        _velocity.Y = 0;
                                        _isOnGround = true;
                                    }
                                }
                                else if (_velocity.Y < 0) // 向上碰撞
                                {
                                    if (result.TileType.HasFlag(TileType.Platform))
                                    {
                                        newPosition += verticalMove;
                                        _isOnGround = false;
                                    }
                                    else
                                    {
                                        newPosition.Y = result.TileRectangle.Bottom;
                                        _velocity.Y = 0;
                                    }
                                }
                            }
                            else
                            {
                                // 可穿透瓦片，允许穿过
                                newPosition += verticalMove;
                                _isOnGround = false;
                            }
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
                if (IsCollidingWithTile(new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3), out TileCollisionResult groundResult))
                {
                    if (!groundResult.TileType.HasFlag(TileType.Penetrable))
                    {
                        _isOnGround = true;
                    }
                    else
                    {
                        _isOnGround = false;
                    }
                }
                else
                {
                    _isOnGround = false;
                }
            }

            if (!_isOnGround && !_isOnSlope)
            {
                _velocity.Y += Gravity;
                if (_velocity.Y > MaxGravity)
                    _velocity.Y = MaxGravity;
            }
        }

        private void UpdateSlopeMovement(ref Vector2 position, TileCollisionResult result, int slopeWidth, int slopeHeight, int topY, int bottomY)
        {
            float targetY = GetOnSlopeHeight(position, result, slopeWidth, slopeHeight, topY, bottomY);
            float minY = result.TileRectangle.Bottom - Size.Y;
            if (targetY < minY)
            {
                targetY = minY;
            }
            position.Y = targetY - _sprite.Size.Y / 2; //转换为基于左上角的坐标
            _isOnGround = true;
        }

        private void UpdateCompressing(ref Vector2 position)
        {
            int newHeight = Size.Y - 2;
            if (newHeight <= _minimumCollisionBox.Y)
            {
                float bottomY = position.Y + Size.Y;
                Size = _minimumCollisionBox;
                position.Y = bottomY - Size.Y;
                Status = SpringStatus.CompressedToMax;
                _keepTimer = KeepDuration;
            }
            else
            {
                float bottomY = position.Y + Size.Y;
                Size = new Point(Size.X, newHeight);
                position.Y = bottomY - Size.Y; 
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

        private void UpdateExpanding(ref Vector2 position)
        {
            int newHeight = Size.Y + 4;
            if (newHeight >= _normalCollisionBox.Y)
            {
                float bottomY = position.Y + Size.Y;
                Size = _normalCollisionBox;
                position.Y = bottomY - Size.Y;
                Rectangle testRect = GetCollisionBox(position);
                if (IsCollidingWithTile(testRect, out TileCollisionResult result) && !result.TileType.HasFlag(TileType.Penetrable))
                {
                    position.Y = result.TileRectangle.Top - _sprite.Size.Y;
                }

                Status = SpringStatus.Normal;
                if (_sprite.CurrentAnimation != "normal")
                    _sprite.SetAnimation("normal");
            }
            else
            {
                float bottomY = position.Y + Size.Y;
                Size = new Point(Size.X, newHeight);
                position.Y = bottomY - Size.Y;
            }
        }

        public void Reset()
        {
            Status = SpringStatus.Normal;
            Size = _normalCollisionBox;
            Position = _basePosition;
            _sprite.SetAnimation("normal");
        }
    }
}