using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;

namespace Project6.GameObjects
{
    public class Egg
    {
        private const int MaxCollisionCount = 10;

        private readonly Sprite _sprite;
        private readonly TiledMap _tilemap;


        private Vector2 _throwDirection;
        private Vector2 _previousPosition;
        private int _collisionCount = 0;
        private bool _isActive = false;
        private bool _shouldFlyOut = false;

        public Vector2 Position { get; set; } = Vector2.Zero;

        public Point Size { get; set; } = new Point(16, 16);

        public Vector2 Velocity { get; set; } = new Vector2(10f, 10f);

        public Rectangle ScreenBounds { get; set; }

        public bool IsActive => _isActive;

        public int CollisionCount => _collisionCount;

        public Egg(Texture2D texture, TiledMap tilemap)
        {
            _sprite = new Sprite(texture);
            _tilemap = tilemap;
        }

        // 修改碰撞检测方法，返回更详细的碰撞信息
        private bool IsCollidingWithTile(Rectangle eggRect, out Rectangle tileRect, out Vector2 normal, out float penetrationDepth)
        {
            TiledMapTileLayer tileLayer = _tilemap.GetLayer<TiledMapTileLayer>("Ground");
            int tileSize = _tilemap.TileWidth;
            int left = eggRect.Left / tileSize;
            int right = eggRect.Right / tileSize;
            int top = eggRect.Top / tileSize;
            int bottom = eggRect.Bottom / tileSize;
            normal = Vector2.Zero;
            penetrationDepth = 0;
            tileRect = Rectangle.Empty;
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    if (tileLayer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile))
                    {
                        if (tile.HasValue && !tile.Value.IsBlank)
                        {
                            tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                            if (eggRect.Intersects(tileRect))
                            {
                                // 计算更精确的碰撞信息
                                CalculateCollisionDetails(eggRect, tileRect, out normal, out penetrationDepth);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        // 改进碰撞细节计算
        private void CalculateCollisionDetails(Rectangle eggRect, Rectangle tileRect, out Vector2 normal, out float penetrationDepth)
        {
            normal = Vector2.Zero;
            penetrationDepth = 0;

            // 计算中心点
            Vector2 eggCenter = new Vector2(eggRect.Center.X, eggRect.Center.Y);
            Vector2 tileCenter = new Vector2(tileRect.Center.X, tileRect.Center.Y);

            // 计算重叠量
            float overlapX = Math.Min(eggRect.Right, tileRect.Right) - Math.Max(eggRect.Left, tileRect.Left);
            float overlapY = Math.Min(eggRect.Bottom, tileRect.Bottom) - Math.Max(eggRect.Top, tileRect.Top);

            // 确定最小重叠方向
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

        private Rectangle GetCollisionBox(Vector2 position)
        {
            int centerX = (int)(position.X + _sprite.Size.X / 2 - Size.X / 2);
            int centerY = (int)(position.Y + _sprite.Size.Y / 2 - Size.Y / 2);
            return new Rectangle(centerX, centerY, Size.X, Size.Y);
        }

        // 改进反弹方法，使用穿透深度进行精确调整
        public void Bounce(Vector2 normal, float penetrationDepth)
        {
            // 根据穿透深度和法线方向调整位置
            Position += normal * penetrationDepth;

            _collisionCount++;

            // 如果达到最大碰撞次数，设置飞出标志
            if (_collisionCount >= MaxCollisionCount)
            {
                _shouldFlyOut = true;
                return;
            }
            Velocity = Vector2.Reflect(Velocity, normal);
        }

        public void Throw(Vector2 throwDirection)
        {
            _isActive = true;
            _throwDirection = throwDirection;
            _previousPosition = Position;
            _shouldFlyOut = false;
            _collisionCount = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isActive) return;

            // 保存上一帧位置
            _previousPosition = Position;

            // 更新位置
            Vector2 newPosition = Position + Velocity * _throwDirection;

            // 如果应该飞出屏幕，则不检测碰撞
            if (_shouldFlyOut)
            {
                Position = newPosition;
            }
            else
            {
                // 检测碰撞
                try
                {
                    Rectangle collisionBox = GetCollisionBox(newPosition);
                    if (IsCollidingWithTile(collisionBox, out Rectangle tileRect, out Vector2 normal, out float penetrationDepth))
                    {
                        // 使用改进的反弹方法
                        Bounce(normal, penetrationDepth);
                    }
                    else
                    {
                        // 没有碰撞，更新位置
                        Position = newPosition;
                    }
                }
                catch
                {
                    _isActive = false;
                }
            }

            // 检测是否超出边界
            if (IsOutOfBounds())
            {
                _isActive = false;
                _collisionCount = 0;
                _shouldFlyOut = false;
            }
        }

        private bool IsOutOfBounds()
        {
            Rectangle eggBounds = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
            return !ScreenBounds.Intersects(eggBounds);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isActive)
            {
                _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
            }
        }
    }
}