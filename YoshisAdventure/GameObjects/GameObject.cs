using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using YoshisAdventure.Interfaces;
using YoshisAdventure.Models;

namespace YoshisAdventure.GameObjects
{
    public abstract class GameObject : ICollidable
    {
        protected TiledMap _tilemap;

        public Vector2 Position { get; set; } = Vector2.Zero;

        public Point Size { get; set; } = new Point(0, 0);

        public Rectangle ScreenBounds { get; set; }

        public abstract Vector2 Velocity { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEatable { get; set; } = true;

        public string Name { get; set; } = string.Empty;

        public abstract Rectangle CollisionBox { get; }

        protected GameObject(TiledMap tilemap)
        {
            _tilemap = tilemap;
        }

        public virtual void OnCollision(GameObject other, CollisionResult collision) { }

        protected bool IsCollidingWithTile(Rectangle objectRect, out Rectangle tileRect)
        {
            TiledMapTileLayer tileLayer = _tilemap.GetLayer<TiledMapTileLayer>("Ground");
            int tileSize = _tilemap.TileWidth;
            int left = objectRect.Left / tileSize;
            int right = objectRect.Right / tileSize;
            int top = objectRect.Top / tileSize;
            int bottom = objectRect.Bottom / tileSize;
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
                            if (objectRect.Intersects(tileRect))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        protected virtual Rectangle GetCollisionBox(Vector2 position)
        {
            return new Rectangle((int)position.X, (int)position.Y, Size.X, Size.Y);
        }

        protected virtual Rectangle GetCollisionBoxBottomCenter(Vector2 position, Point spriteSize)
        {
            int X = (int)(position.X + spriteSize.X / 2 - Size.X / 2);
            int Y = (int)(position.Y + spriteSize.Y - Size.Y);
            return new Rectangle(X, Y, Size.X, Size.Y);
        }

        protected virtual Rectangle GetCollisionBoxCenter(Vector2 position, Point spriteSize)
        {
            Point centerPosition = new Point((int)Position.X, (int)Position.Y);
            var collisionBox = GetCollisionBox(position);
            return new Rectangle(centerPosition.X, centerPosition.Y, collisionBox.Width, collisionBox.Height);
        }

        public bool IsOutOfScreenBounds()
        {
            return !ScreenBounds.Intersects(GetCollisionBox(Position));
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}