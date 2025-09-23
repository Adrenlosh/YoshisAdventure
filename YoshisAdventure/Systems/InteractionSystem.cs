// InteractionSystem.cs
using Microsoft.Xna.Framework;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Interfaces;
using System.Diagnostics;
using System.Linq;

namespace YoshisAdventure.Systems
{
    public class InteractionSystem
    {
        public void Update(GameTime gameTime)
        {
            // 处理所有可能的交互类型
            HandleCollisions();
            HandleTriggers();
            HandleProjectileFlysAndHits();
            HandlePlayerSpecificInteractions();
            HandlePlayerDecorations();
        }

        private void HandlePlayerDecorations()
        {
            var objs = GameObjectsSystem.GameObjects.OfType<Egg>();
            foreach (var obj in objs)
            {
                if (!obj.IsHeldAndThrew && !obj.IsOutOfScreenBounds())
                {
                    obj.Position = GameObjectsSystem.Player.EggHoldingPosition;
                    obj.ScreenBounds = GameObjectsSystem.Player.ScreenBounds;
                }
            }
        }

        private void HandleCollisions()
        {
            var collidables = GameObjectsSystem.GetObjectsOfInterface<ICollidable>();
            Yoshi player = GameObjectsSystem.Player;
            Rectangle playerRect = player.CollisionRectangle;

            foreach (var collidable in collidables)
            {
                if (collidable is Spring spring && player.CapturedObject != spring)
                {
                    Rectangle springRect = spring.CollisionRectangle;
                    //springRect.Size += new Point(2, 0);
                    //springRect.Location -= new Point(1, 0);
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(springRect);
                    if (collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        // 先处理弹簧状态
                        if (spring.Status == SpringStatus.CompressedToMax)
                        {
                            player.Bounce();
                            spring.Release();
                        }
                        else if (spring.Status == SpringStatus.Normal)
                        {
                            // 只有当玩家从上方碰撞时才压缩弹簧
                            if (collisionResult.Direction == CollisionDirection.Top)
                            {
                                spring.Compress();

                                // 将玩家放置在弹簧顶部，防止穿透
                                if (player.Velocity.Y >= 0)
                                {
                                    player.Position = new Vector2(player.Position.X, springRect.Top - playerRect.Height);
                                    player.Velocity = new Vector2(player.Velocity.X, 0);
                                }
                            }
                        }

                        switch (collisionResult.Direction)
                        {
                            case CollisionDirection.Bottom:
                                player.Velocity = new Vector2(1f, player.Velocity.Y);
                                break;
                            case CollisionDirection.Left:
                                player.Position = new Vector2(springRect.Left - playerRect.Width - 1, player.Position.Y);
                                player.Velocity = new Vector2(0, player.Velocity.Y);
                                break;
                            case CollisionDirection.Right:
                                player.Position = new Vector2(springRect.Right + 1, player.Position.Y);
                                player.Velocity = new Vector2(0.5f, player.Velocity.Y);
                                break;
                        }
                    }
                }
            }
        }

        private void HandleTriggers()
        {
        }

        private void HandleProjectileFlysAndHits()
        {
            // 处理抛射物命中逻辑
            var projectiles = GameObjectsSystem.GetObjectsOfInterface<IProjectile>();
            var damageables = GameObjectsSystem.GetObjectsOfInterface<IDamageable>();

            foreach (var projectile in projectiles)
            {
                if (projectile is Egg egg)
                {
                    // 访问 Egg 的特有成员
                    egg.ScreenBounds = GameObjectsSystem.Player.ScreenBounds;
                }
            }
        }

        private void HandlePlayerSpecificInteractions()
        {
        }
    }
}