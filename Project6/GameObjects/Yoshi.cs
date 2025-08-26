using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using RenderingLibrary.Graphics;
using System;
using System.Diagnostics;

namespace Project6.GameObjects
{
    public class Yoshi
    {
        private AnimatedSprite _sprite;
        public Vector2 Position = new Vector2(0, 0);

        private Vector2 _velocity = Vector2.Zero;
        private Vector2 _acceleration = Vector2.Zero;
        private const float Gravity = 0.5f;
        private const float JumpForce = -10f;
        private const float MoveSpeed = 3f;
        private const float AccelerationRate = 0.3f; // 加速度
        private const float Friction = 0.2f;         // 阻力
        private bool _isOnGround = false;

        // 引用Tilemap
        private Tilemap _tilemap;

        public Yoshi(AnimatedSprite sprite, Tilemap tilemap)
        {
            _sprite = sprite;
            _tilemap = tilemap;
        }

        public void HandleInput()
        {
            // 水平加速度
            if (GameController.MoveLeft())
            {
                _acceleration.X = -AccelerationRate;
                _sprite.Effects = SpriteEffects.FlipHorizontally;
            }
            else if (GameController.MoveRight())
            {
                _acceleration.X = AccelerationRate;
                _sprite.Effects = SpriteEffects.None;
            }
            else
            {
                _acceleration.X = 0;
            }

            if (GameController.APressed() && _isOnGround)
            {
                _velocity.Y = JumpForce;
                _isOnGround = false;
            }
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
            HandleInput();

            // 应用惯性
            _velocity.X += _acceleration.X;

            // 限制最大速度
            if (_velocity.X > MoveSpeed) _velocity.X = MoveSpeed;
            if (_velocity.X < -MoveSpeed) _velocity.X = -MoveSpeed;

            // 应用阻力（摩擦力），只有在没有按键时才生效
            if (_acceleration.X == 0)
            {
                if (_velocity.X > 0)
                {
                    _velocity.X -= Friction;
                    if (_velocity.X < 0) _velocity.X = 0;
                }
                else if (_velocity.X < 0)
                {
                    _velocity.X += Friction;
                    if (_velocity.X > 0) _velocity.X = 0;
                }
            }

            // 应用重力
            if (!_isOnGround)
            {
                _velocity.Y += Gravity;
            }

            Vector2 newPosition = Position;

            // 水平移动
            if (_velocity.X != 0)
            {
                Vector2 horizontalMove = new Vector2(_velocity.X, 0);
                Vector2 testPosition = newPosition + horizontalMove;
                Rectangle testRect = new Rectangle(
                    (int)testPosition.X,
                    (int)testPosition.Y,
                    (int)_sprite.Width,
                    (int)_sprite.Height);

                if (!IsCollidingWithTile(testRect, out _))
                {
                    newPosition += horizontalMove;
                }
                else
                {
                    _velocity.X = 0;
                }
            }

            // 垂直移动
            if (_velocity.Y != 0)
            {
                Vector2 verticalMove = new Vector2(0, _velocity.Y);
                Vector2 testPosition = newPosition + verticalMove;
                Rectangle testRect = new Rectangle(
                    (int)testPosition.X,
                    (int)testPosition.Y,
                    (int)_sprite.Width,
                    (int)_sprite.Height);

                if (IsCollidingWithTile(testRect, out Rectangle tileRect))
                {
                    if (_velocity.Y > 0)
                    {
                        // 下落，站在瓦片上
                        newPosition.Y = tileRect.Top - _sprite.Height;
                        _velocity.Y = 0;
                        _isOnGround = true;
                    }
                    else if (_velocity.Y < 0)
                    {
                        // 跳跃撞到顶部
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
            else
            {
                // 检查玩家是否还在地面上（防止走出瓦片后悬空不掉落）
                Rectangle groundCheckRect = new Rectangle(
                    (int)newPosition.X,
                    (int)(newPosition.Y + _sprite.Height),
                    (int)_sprite.Width,
                    2 // 检查脚下2像素
                );
                if (!IsCollidingWithTile(groundCheckRect, out _))
                {
                    _isOnGround = false;
                }
            }

            Position = newPosition;
        }

        // 检查玩家碰撞箱是否与阻挡瓦片重叠
        private bool IsCollidingWithTile(Rectangle playerRect, out Rectangle tileRect)
        {
            int tileSize = (int)_tilemap.TileWidth;
            int left = playerRect.Left / tileSize;
            int right = playerRect.Right / tileSize;
            int top = playerRect.Top / tileSize;
            int bottom = playerRect.Bottom / tileSize;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    var tile = _tilemap.GetTile(x, y);
                    if (tile != null && tile.IsBlocking)
                    {
                        tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                        if (playerRect.Intersects(tileRect))
                            return true;
                    }
                }
            }
            tileRect = Rectangle.Empty;
            return false;
        }

        public void Draw()
        {
            _sprite.Draw(Core.SpriteBatch, Position);
        }
    }
}
