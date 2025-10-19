using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;
using YoshisAdventure.Enums;
using YoshisAdventure.Interfaces;
using YoshisAdventure.Models;
using YoshisAdventure.Systems;

namespace YoshisAdventure.GameObjects
{
    public abstract class GameObject : ICollidable
    {
        protected TiledMap _tilemap;
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsCaptured { get; set; } = false;

        public bool IsCapturable { get; set; } = true;

        public Vector2 Position { get; set; } = Vector2.Zero;

        public virtual Vector2 CenterBottomPosition { get; set; }

        public Point Size { get; set; } = new Point(0, 0);

        public Rectangle ScreenBounds { get; set; } = Rectangle.Empty;

        public abstract Rectangle CollisionBox { get; }

        public virtual Vector2 Velocity { get; set; } = Vector2.Zero;


        protected GameObject(TiledMap tilemap)
        {
            _tilemap = tilemap;
        }

        public virtual void OnCollision(GameObject other, ObjectCollisionResult collision) { }

        protected bool IsCollidingWithTile(Rectangle objectRect, out TileCollisionResult result)
        {
            TiledMapTileLayer tileLayer = _tilemap.GetLayer<TiledMapTileLayer>("Ground");
            int tileSize = _tilemap.TileWidth;
            int left = objectRect.Left / tileSize;
            int right = objectRect.Right / tileSize;
            int top = objectRect.Top / tileSize;
            int bottom = objectRect.Bottom / tileSize;
            result = new TileCollisionResult();
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    if (tileLayer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile))
                    {
                        if (tile.HasValue && !tile.Value.IsBlank)
                        {
                            Rectangle rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                            result = new TileCollisionResult(tile, Rectangle.Intersect(objectRect, rect), objectRect, rect, _tilemap);
                            if (objectRect.Intersects(rect))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        protected bool IsCollidingWithTile(Rectangle objectRect, TileType tileType, out TileCollisionResult result)
        {
            var isCollided = IsCollidingWithTile(objectRect, out result);
            if (result.TileType == tileType)
            {
                return isCollided;
            }
            else
            {
                result = new TileCollisionResult();
                return false;
            }
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

        public virtual bool IsOutOfTilemap(Vector2 position)
        {
            Rectangle worldRect = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            if (!worldRect.Contains(position))
                return true;
            else
                return false;
        }

        public virtual bool IsOutOfTilemapBottom(Vector2 position)
        {
            Rectangle worldRect = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            if (position.Y > worldRect.Bottom)
                return true;
            else
                return false;
        }

        public virtual bool IsOutOfTilemapBottom() => IsOutOfTilemapBottom(Position);

        public virtual bool IsOutOfTilemapSidePosition(Vector2 position)
        {
            Rectangle worldRect = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            if (position.X < worldRect.Left || position.X > worldRect.Right)
                return true;
            else
                return false;
        }
        public virtual bool IsOutOfTilemapSidePosition() => IsOutOfTilemapSidePosition(Position);

        public virtual bool IsOutOfTilemapSideBox(Rectangle rect)
        {
            Rectangle worldRect = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            if (rect.Left < worldRect.Left || rect.Right > worldRect.Right)
                return true;
            else
                return false;
        }

        public virtual bool IsOutOfTilemapSideBox() => IsOutOfTilemapSideBox(CollisionBox);

        public bool IsOutOfScreenBounds()
        {
            return !ScreenBounds.Intersects(GetCollisionBox(Position));
        }

        public virtual bool IsOnSlope(TileCollisionResult result, out int slopeWidth, out int slopeHeight, out int topY, out int bottomY)
        {
            slopeHeight = 16;
            slopeWidth = 16;
            topY = 16;
            bottomY = 0;
            Debug.WriteLine(result.Direction);
            if ((result.TileType.HasFlag(TileType.SteepSlopeLeft) || result.TileType.HasFlag(TileType.SteepSlopeRight) || result.TileType.HasFlag(TileType.GentleSlopeLeft) || result.TileType.HasFlag(TileType.GentleSlopeRight)) && result.Direction == CollisionDirection.Top)
            {
                if (result.Properties.TryGetValue("Width", out string slopeWidthStr))
                {
                    slopeWidth = Convert.ToInt32(slopeWidthStr);
                }
                if (result.Properties.TryGetValue("Height", out string slopeHeightStr))
                {
                    slopeHeight = Convert.ToInt32(slopeHeightStr);
                }
                if (result.Properties.TryGetValue("TopY", out string topYStr))
                {
                    topY = Convert.ToInt32(topYStr);
                }
                if (result.Properties.TryGetValue("BottomY", out string bottomYStr))
                {
                    bottomY = Convert.ToInt32(bottomYStr);
                }
                return true;
            }
            return false;
        }

        public virtual float GetOnSlopeHeight(Vector2 position, TileCollisionResult result, int slopeWidth, int slopeHeight, int topY, int bottomY)
        {
            float onSlopeHeight = 0;
            if (result.TileType.HasFlag(TileType.SteepSlopeRight))
            {
                onSlopeHeight = result.TileRectangle.Location.Y + (CenterBottomPosition.X - result.TileRectangle.Location.X) * (slopeHeight / slopeWidth);
            }
            else if (result.TileType.HasFlag(TileType.SteepSlopeLeft))
            {
                onSlopeHeight = result.TileRectangle.Location.Y + (slopeHeight - (CenterBottomPosition.X - result.TileRectangle.Location.X) * (slopeHeight / slopeWidth));
            }
            else if (result.TileType.HasFlag(TileType.GentleSlopeLeft))
            {
                onSlopeHeight = result.TileRectangle.Y + bottomY + (result.TileRectangle.X - CenterBottomPosition.X) * (topY - bottomY) / slopeWidth;
            }
            else if (result.TileType.HasFlag(TileType.GentleSlopeRight))
            {
                onSlopeHeight = result.TileRectangle.Y + topY + (result.TileRectangle.X - CenterBottomPosition.X) * (bottomY - topY) / slopeWidth;
            }
            else
            {
                return 0;
            }
            float targetY = onSlopeHeight - slopeHeight;
            return targetY;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}