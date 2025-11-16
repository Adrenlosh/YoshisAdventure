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
        protected readonly TiledMap _tilemap;

        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsCaptured { get; set; } = false;
        public bool IsCapturable { get; set; } = true;
        public virtual bool IsOnGround { get; set; }
        public virtual Vector2 CenterBottomPosition { get; set; }
        public abstract Rectangle CollisionBox { get; }
        public virtual Vector2 Velocity { get; set; } = Vector2.Zero;
        public Point Size { get; set; } = Point.Zero;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Rectangle ScreenBounds { get; set; } = Rectangle.Empty;

        protected GameObject(TiledMap tilemap)
        {
            _tilemap = tilemap;
            PhysicsSystem.Instance.Register(this, GetPhysicsConfig());
        }

        ~GameObject()
        {
            PhysicsSystem.Instance.Unregister(this);
        }

        protected virtual PhysicsConfig GetPhysicsConfig() => new PhysicsConfig();

        protected virtual void ApplyPhysics(GameTime gameTime)
        {
            PhysicsResult physicsResult = PhysicsSystem.Instance.Apply(this, gameTime);
            IsOnGround = physicsResult.IsOnGround;
        }

        public virtual void OnCollision(GameObject other, ObjectCollisionResult collision) { }

        #region Collision

        public bool IsCollidingWithTile(Rectangle objectRect, out TileCollisionResult result, string layerName = "Ground")
        {
            TiledMapTileLayer tileLayer = _tilemap.GetLayer<TiledMapTileLayer>(layerName);
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
                    if (tileLayer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile) &&
                        tile.HasValue && !tile.Value.IsBlank)
                    {
                        Rectangle tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                        if (objectRect.Intersects(tileRect))
                        {
                            result = new TileCollisionResult(tile, Rectangle.Intersect(objectRect, tileRect),
                                                           objectRect, tileRect, _tilemap);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual Rectangle GetCollisionBox(Vector2 position) => new Rectangle((int)position.X, (int)position.Y, Size.X, Size.Y);

        public virtual Rectangle GetCollisionBoxBottomCenter(Vector2 position, Point spriteSize)
        {
            int x = (int)(position.X + spriteSize.X / 2 - Size.X / 2);
            int y = (int)(position.Y + spriteSize.Y - Size.Y);
            return new Rectangle(x, y, Size.X, Size.Y);
        }

        public virtual Rectangle GetCollisionBoxCenter(Vector2 position, Point spriteSize)
        {
            Point centerPosition = new Point((int)position.X, (int)position.Y);
            Rectangle collisionBox = GetCollisionBox(position);
            return new Rectangle(centerPosition.X, centerPosition.Y, collisionBox.Width, collisionBox.Height);
        }

        #endregion

        #region Boundary

        public virtual bool IsOutOfTilemap(Vector2 position)
        {
            Rectangle worldRect = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            return !worldRect.Contains(position);
        }

        public virtual bool IsOutOfTilemapBottom(Vector2 position)
        {
            return position.Y > _tilemap.HeightInPixels;
        }

        public virtual bool IsOutOfTilemapBottom() => IsOutOfTilemapBottom(Position);

        public virtual bool IsOutOfTilemapSidePosition(Vector2 position)
        {
            return position.X < 0 || position.X > _tilemap.WidthInPixels;
        }

        public virtual bool IsOutOfTilemapSidePosition() => IsOutOfTilemapSidePosition(Position);

        public virtual bool IsOutOfTilemapSideBox(Rectangle rect)
        {
            return rect.Left < 0 || rect.Right > _tilemap.WidthInPixels;
        }

        public virtual bool IsOutOfTilemapSideBox() => IsOutOfTilemapSideBox(CollisionBox);

        public bool IsOutOfScreenBounds() => !ScreenBounds.Intersects(GetCollisionBox(Position));

        #endregion

        #region Slope

        public virtual bool IsOnSlope(TileCollisionResult result, out int slopeWidth, out int slopeHeight,
                                    out int topY, out int bottomY)
        {
            slopeWidth = 16;
            slopeHeight = 16;
            topY = 16;
            bottomY = 0;

            bool isSlopeTile = result.TileType.HasFlag(TileType.SteepSlopeLeft) ||
                             result.TileType.HasFlag(TileType.SteepSlopeRight) ||
                             result.TileType.HasFlag(TileType.GentleSlopeLeft) ||
                             result.TileType.HasFlag(TileType.GentleSlopeRight);

            if (isSlopeTile || result.Direction == CollisionDirection.Top)
            {
                ExtractSlopeProperties(result, ref slopeWidth, ref slopeHeight, ref topY, ref bottomY);
                return true;
            }

            return false;
        }

        public virtual float GetOnSlopeHeight(Vector2 position, TileCollisionResult result,
                                            int slopeWidth, int slopeHeight, int topY, int bottomY)
        {
            Vector2 tileLocation = result.TileRectangle.Location.ToVector2();
            float relativeX = CenterBottomPosition.X - tileLocation.X;

            return result.TileType switch
            {
                var type when type.HasFlag(TileType.SteepSlopeRight) =>
                    tileLocation.Y + relativeX * (slopeHeight / (float)slopeWidth),

                var type when type.HasFlag(TileType.SteepSlopeLeft) =>
                    tileLocation.Y + (slopeHeight - relativeX * (slopeHeight / (float)slopeWidth)),

                var type when type.HasFlag(TileType.GentleSlopeLeft) =>
                    tileLocation.Y + bottomY + (result.TileRectangle.X - CenterBottomPosition.X) *
                    (topY - bottomY) / (float)slopeWidth,

                var type when type.HasFlag(TileType.GentleSlopeRight) =>
                    tileLocation.Y + topY + (result.TileRectangle.X - CenterBottomPosition.X) *
                    (bottomY - topY) / (float)slopeWidth,

                _ => 0f
            };
        }

        private void ExtractSlopeProperties(TileCollisionResult result, ref int slopeWidth,
                                          ref int slopeHeight, ref int topY, ref int bottomY)
        {
            if (result.Properties.TryGetValue("Width", out string slopeWidthStr))
                slopeWidth = Convert.ToInt32(slopeWidthStr);

            if (result.Properties.TryGetValue("Height", out string slopeHeightStr))
                slopeHeight = Convert.ToInt32(slopeHeightStr);

            if (result.Properties.TryGetValue("TopY", out string topYStr))
                topY = Convert.ToInt32(topYStr);

            if (result.Properties.TryGetValue("BottomY", out string bottomYStr))
                bottomY = Convert.ToInt32(bottomYStr);
        }

        #endregion

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}