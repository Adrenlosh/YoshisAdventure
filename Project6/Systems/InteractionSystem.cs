// InteractionSystem.cs
using Microsoft.Xna.Framework;
using Project6.GameObjects;
using Project6.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project6.Systems
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
            var objs = GameObjectsManager.GameObjects.OfType<Egg>();
            foreach (var obj in objs)
            {
                if (!obj.IsHeldAndThrew && !obj.IsOutOfScreenBounds())
                {
                    obj.Position = GameObjectsManager.Player.EggHoldingPosition;
                    obj.ScreenBounds = GameObjectsManager.Player.ScreenBounds;
                }
            }
        }

        private void HandleCollisions()
        {
            //var collidables = GameObjectsManager.GetObjectsOfType<ICollidable>();

            //// 简化的碰撞检测
            //for (int i = 0; i < collidables.Count; i++)
            //{
            //    for (int j = i + 1; j < collidables.Count; j++)
            //    {
            //        var a = collidables[i] as GameObject;
            //        var b = collidables[j] as GameObject;

            //        if (a.CollisionRectangle.Intersects(b.CollisionRectangle))
            //        {
            //            a.OnCollision(b);
            //            b.OnCollision(a);
            //        }
            //    }
            //}
        }

        private void HandleTriggers()
        {
            // 处理触发器逻辑
            //var triggers = GameObjectsManager.GetObjectsOfType<ITrigger>();
            //var players = GameObjectsManager.GetObjectsOfType<Player>();

            //foreach (var trigger in triggers)
            //{
            //    foreach (var player in players)
            //    {
            //        if (trigger.CollisionRectangle.Intersects(player.CollisionRectangle))
            //        {
            //            trigger.OnTrigger(player);
            //        }
            //    }
            //}
        }

        private void HandleProjectileFlysAndHits()
        {
            // 处理抛射物命中逻辑
            var projectiles = GameObjectsManager.GetObjectsOfInterface<IProjectile>();
            var damageables = GameObjectsManager.GetObjectsOfInterface<IDamageable>();

            foreach (var projectile in projectiles)
            {
                if (projectile is Egg egg)
                {
                    // 访问 Egg 的特有成员
                    egg.ScreenBounds = GameObjectsManager.Player.ScreenBounds;
                }
            }
            //foreach (var projectile in projectiles)
            //{
            //    foreach (var damageable in damageables)
            //    {
            //        // 确保抛射物不会伤害其所有者
            //        if (projectile.Owner != damageable &&
            //            projectile.CollisionRectangle.Intersects(damageable.CollisionRectangle))
            //        {
            //            damageable.TakeDamage(projectile.Damage, projectile.Owner);
            //            projectile.OnHit(damageable);
            //        }
            //    }
            //}
        }

        private void HandlePlayerSpecificInteractions()
        {
            // 处理玩家特定交互，如拾取物品、与NPC对话等
            //var player = GameObjectsManager.Player;
            //if (player == null) return;

            //var interactables = GameObjectsManager.GetObjectsInRange(player.Position, 50f)
            //    .OfType<IInteractable>();

            //foreach (var interactable in interactables)
            //{
                // 检查玩家是否按下交互键
                //if (/* 交互键按下 */)
                //{
                //    interactable.OnInteract(player);
                //}
            //}
        }
    }
}