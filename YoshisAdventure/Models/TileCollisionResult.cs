using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;
using YoshisAdventure.Enums;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Models
{
    public struct TileCollisionResult
    {
        public TiledMapTile? CollidedTile { get; set; }

        public Rectangle TileRectangle { get; set; }

        public Rectangle Intersection { get; set; }

        public CollisionDirection Direction { get; set; }

        public TileType TileType { get; set; } = TileType.Penetrable;

        public Point PositionInMap { get; set; } = Point.Zero;

        public TileCollisionResult(TiledMapTile? tile, Rectangle intersection, Rectangle originalRect, Rectangle otherRect, TiledMap tileMap)
        {
            CollidedTile = tile;
            Intersection = intersection;
            Direction = CalculateCollisionDirection(originalRect, otherRect, intersection);
            TileRectangle = otherRect;
            PositionInMap = new Point(tile.Value.X, tile.Value.Y);
            TiledMapTileset tileset = tileMap.GetTilesetByTileGlobalIdentifier(tile.Value.GlobalIdentifier);
            if (tileset != null)
            {
                if(tileset.Tiles[tile.Value.GlobalIdentifier - tileMap.GetTilesetFirstGlobalIdentifier(tileset)].Properties.TryGetValue("TileType", out string tileTypeStr))
                {
                    Enum.TryParse(tileTypeStr, out TileType type);
                    TileType = type;
                }
            }
        }

        private CollisionDirection CalculateCollisionDirection(Rectangle rectA, Rectangle rectB, Rectangle intersection)
        {
            float overlapLeft = Math.Abs(rectA.Right - rectB.Left);
            float overlapRight = Math.Abs(rectB.Right - rectA.Left);
            float overlapTop = Math.Abs(rectA.Bottom - rectB.Top);
            float overlapBottom = Math.Abs(rectB.Bottom - rectA.Top);
            float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));
            if (minOverlap == overlapTop) return CollisionDirection.Top;
            if (minOverlap == overlapBottom) return CollisionDirection.Bottom;
            if (minOverlap == overlapLeft) return CollisionDirection.Left;
            return CollisionDirection.Right;
        }
    }
}