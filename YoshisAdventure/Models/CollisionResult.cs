using Microsoft.Xna.Framework;

using YoshisAdventure.GameObjects;
using YoshisAdventure.Systems;
using System;

namespace YoshisAdventure.Models
{
    public struct CollisionResult
    {
        public GameObject CollidedObject;
        public Rectangle Intersection;
        public CollisionDirection Direction;

        public CollisionResult(GameObject obj, Rectangle intersection, Rectangle originalRect, Rectangle otherRect)
        {
            CollidedObject = obj;
            Intersection = intersection;
            Direction = CalculateCollisionDirection(originalRect, otherRect, intersection);
        }

        private CollisionDirection CalculateCollisionDirection(Rectangle rectA, Rectangle rectB, Rectangle intersection)
        {
            // 计算各边的重叠量
            float overlapLeft = Math.Abs(rectA.Right - rectB.Left);
            float overlapRight = Math.Abs(rectB.Right - rectA.Left);
            float overlapTop = Math.Abs(rectA.Bottom - rectB.Top);
            float overlapBottom = Math.Abs(rectB.Bottom - rectA.Top);

            // 找到最小的重叠量，即碰撞方向
            float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

            // 根据最小重叠量确定碰撞方向
            if (minOverlap == overlapTop) return CollisionDirection.Top;
            if (minOverlap == overlapBottom) return CollisionDirection.Bottom;
            if (minOverlap == overlapLeft) return CollisionDirection.Left;
            return CollisionDirection.Right;
        }
    }
}