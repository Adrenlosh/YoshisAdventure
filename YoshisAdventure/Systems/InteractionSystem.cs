using Microsoft.Xna.Framework;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Interfaces;
using System.Linq;
using System;

namespace YoshisAdventure.Systems
{
    public class InteractionSystem
    {
        private bool _isGoal = false;
        public event Action<string> OnDialogue;
        public event Action<string, string> OnSwitchMap;
        public event Action<int> OnCollectACoin;
        public event Action OnGoal;

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
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(sign);
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
                else if (collidable is Enemy enemy && player.CapturedObject != enemy)
                {
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(enemy);
                    if (collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        player.TakeDamage(1, enemy);
                    }
                } 
                else if (collidable is Goal goal)
                {
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(goal);
                    if (collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        player.OnCollision(goal, collisionResult);
                        goal.OnCollision(player, collisionResult);
                        if (!_isGoal)
                        {
                            OnGoal?.Invoke();
                        }
                        _isGoal = true;
                    }
                }
                else if(collidable is Coin coin)
                {
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(coin);
                    if(collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        player.OnCollision(coin, collisionResult);
                        coin.OnCollision(player, collisionResult);
                        OnCollectACoin?.Invoke(coin.Value);
                        GameObjectsSystem.RemoveGameObject(coin);
                    }
                }
                else if(collidable is Door door)
                {
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(door);
                    if (GameController.MoveUp() && collisionResult.CollidedObject != null && collisionResult.CollidedObject == player)
                    {
                        player.OnCollision(door, collisionResult);
                        door.OnCollision(player, collisionResult);
                        OnSwitchMap?.Invoke(door.TargetMap, door.TargetPoint);
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
            Yoshi player = GameObjectsSystem.Player;
            foreach (var projectile in projectiles)
            {
                if (projectile is Egg egg)
                {
                    egg.ScreenBounds = GameObjectsSystem.Player.ScreenBounds;
                    var collisionResult = GameObjectsSystem.CheckObjectCollision(egg);
                   if(collisionResult.CollidedObject != null && collisionResult.CollidedObject != player)
                   {
                        if(collisionResult.CollidedObject is Coin coin)
                        {
                            player.OnCollision(coin, collisionResult);
                            coin.OnCollision(player, collisionResult);
                            OnCollectACoin?.Invoke(coin.Value);
                            GameObjectsSystem.RemoveGameObject(coin);
                        }
                        else if(collisionResult.CollidedObject is Enemy enemy)
                        {
                            GameObjectsSystem.RemoveGameObject(enemy);
                        }
                   }
                }
            }
        }

        private void HandlePlayerSpecificInteractions()
        {
        }
    }
}