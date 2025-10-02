using Microsoft.Xna.Framework;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Interfaces;
using System.Linq;
using System;

namespace YoshisAdventure.Systems
{
    public class InteractionSystem
    {
        public event Action<string> OnDialogue;

        public void Update(GameTime gameTime)
        {
            HandleCollisions();
            HandleTriggers();
            HandleProjectileFlysAndHits();
            HandlePlayerSpecificInteractions();
            HandlePlayerDecorations();
            HandleDialogue();
        }

        private void HandleDialogue()
        {
            var dialogable = GameObjectsSystem.GetObjectsOfInterface<IDialogable>();
            Yoshi player = GameObjectsSystem.Player;
            foreach (var obj in dialogable)
            {
                if (obj is Sign sign)
                {
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(sign.CollisionBox);
                    if (GameController.MoveUp() && collisionResult.CollidedObject == player)
                    {
                        sign.ScreenBounds = GameObjectsSystem.Player.ScreenBounds;
                        OnDialogue?.Invoke(sign.MessageID);
                    }
                }
            }
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
            Rectangle playerRect = player.CollisionBox;

            foreach (var collidable in collidables)
            {
                if (collidable is Spring spring && player.CapturedObject != spring)
                {
                    Rectangle springRect = spring.CollisionBox;
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(springRect);
                    if (collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        if (spring.Status == SpringStatus.CompressedToMax)
                        {
                            player.Bounce();
                            spring.Release();
                        }
                        else if (spring.Status == SpringStatus.Normal)
                        {
                            if (collisionResult.Direction == CollisionDirection.Top)
                            {
                                spring.Compress();
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
                else if(collidable is Enemy enemy && player.CapturedObject != enemy)
                {
                    Rectangle enemyRect = enemy.CollisionBox;
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(enemyRect);
                    if(collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        player.TakeDamage(1, enemy);
                    }
                }
            }
        }

        private void HandleTriggers()
        {
        }

        private void HandleProjectileFlysAndHits()
        {
            var projectiles = GameObjectsSystem.GetObjectsOfInterface<IProjectile>();
            var damageables = GameObjectsSystem.GetObjectsOfInterface<IDamageable>();

            foreach (var projectile in projectiles)
            {
                if (projectile is Egg egg)
                {
                    egg.ScreenBounds = GameObjectsSystem.Player.ScreenBounds;
                }
            }
        }

        private void HandlePlayerSpecificInteractions()
        {
        }
    }
}